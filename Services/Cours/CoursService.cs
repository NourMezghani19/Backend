using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.DTOs.Cours;

namespace backend.Services;

public class CoursService
{
    private readonly AppDbContext db;

    public CoursService(AppDbContext db)
    {
        this.db = db;
    }

    // ── GET ALL ──────────────────────────────────────────
    public async Task<List<CoursResponseDto>> GetAll(string? search = null, GenreCours? genre = null)
    {
        var query = db.Cours
            .Include(c => c.SessionsCours)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.ToLower();
            query = query.Where(c => c.Nom.ToLower().Contains(keyword));
        }

        if (genre.HasValue)
        {
            query = query.Where(c => c.Genre == genre.Value);
        }

        return await query
                .Include(c => c.SessionsCours)

            .OrderBy(c => c.Nom)
            .Select(c => MapToDto(c))
            .ToListAsync();
    }

    // ── GET BY ID ────────────────────────────────────────
    public async Task<CoursResponseDto> GetById(int id)
    {
        var cours = await db.Cours
            .Include(c => c.SessionsCours)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException("Cours introuvable");

        return MapToDto(cours);
    }

    // ── CREATE ───────────────────────────────────────────
    public async Task<CoursResponseDto> Create(CreateCoursDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nom))
            throw new ArgumentException("Nom obligatoire");

        if (dto.CapaciteMax <= 0 || dto.CapaciteMax > 30)
            throw new InvalidOperationException("Capacité max entre 1 et 30");

        var cours = new Cours
        {
            Nom = dto.Nom.Trim(),
            Description = dto.Description?.Trim(),
            CapaciteMax = dto.CapaciteMax,
            Genre = dto.Genre,
            Actif = true,
            DateCreation = DateTime.UtcNow
        };

        db.Cours.Add(cours);
        await db.SaveChangesAsync();

        return MapToDto(cours);
    }

    // ── UPDATE ───────────────────────────────────────────
    public async Task<CoursResponseDto> Update(int id, UpdateCoursDto dto)
    {
        var cours = await db.Cours
            .Include(c => c.SessionsCours)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException("Cours introuvable");

        if (dto.Nom != null)
            cours.Nom = dto.Nom.Trim();

        if (dto.Description != null)
            cours.Description = dto.Description.Trim();

        if (dto.CapaciteMax.HasValue)
        {
            if (dto.CapaciteMax <= 0 || dto.CapaciteMax > 30)
                throw new InvalidOperationException("Capacité max entre 1 et 30");

            cours.CapaciteMax = dto.CapaciteMax.Value;
        }

        if (dto.Genre.HasValue)
            cours.Genre = dto.Genre.Value;

        if (dto.Actif.HasValue)
            cours.Actif = dto.Actif.Value;

        await db.SaveChangesAsync();

        return MapToDto(cours);
    }

    // ── DELETE ───────────────────────────────────────────
    public async Task Delete(int id)
    {
        var cours = await db.Cours
            .Include(c => c.SessionsCours)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException("Cours introuvable");

        if (cours.SessionsCours.Any(s => s.Statut == "Planifie"))
            throw new InvalidOperationException("Impossible : sessions planifiées existantes");

        db.Cours.Remove(cours);
        await db.SaveChangesAsync();
    }

    // ── PLANIFIER SESSION ────────────────────────────────
    public async Task<SessionResponseDto> PlanifierSession(PlanifierSessionDto dto)
    {
        var cours = await db.Cours.FindAsync(dto.CoursId)
            ?? throw new KeyNotFoundException("Cours introuvable");

        var coach = await db.Coachs.FindAsync(dto.CoachId)
            ?? throw new KeyNotFoundException("Coach introuvable");

        if (!coach.Disponible)
            throw new InvalidOperationException("Coach non disponible");

        var session = new Session_Cours
        {
            CoursId = dto.CoursId,
            CoachId = dto.CoachId,
            DateHeure = dto.DateHeure,
            PlacesDisponibles = cours.CapaciteMax,
            Statut = "Planifie"
        };

        db.Sessions.Add(session);
        await db.SaveChangesAsync();

        await db.Entry(session).Reference(s => s.Cours).LoadAsync();
        await db.Entry(session).Reference(s => s.Coach).LoadAsync();

        return MapSessionToDto(session);
    }

    // ── ANNULER SESSION ─────────────────────────────────
    public async Task AnnulerSession(int sessionId)
    {
        var session = await db.Sessions.FindAsync(sessionId)
            ?? throw new KeyNotFoundException("Session introuvable");

        if (session.Statut == "Annule")
            throw new InvalidOperationException("Session déjà annulée");

        session.Statut = "Annule";
        await db.SaveChangesAsync();
    }

    // ── MODIFIER HORAIRE ────────────────────────────────
    public async Task<SessionResponseDto> ModifierHoraire(int sessionId, ModifierHoraireDto dto)
    {
        var session = await db.Sessions
            .Include(s => s.Cours)
            .Include(s => s.Coach)
            .FirstOrDefaultAsync(s => s.Id == sessionId)
            ?? throw new KeyNotFoundException("Session introuvable");

        if (session.Statut == "Annule")
            throw new InvalidOperationException("Session annulée");

        session.DateHeure = dto.NouvelleDate;

        await db.SaveChangesAsync();

        return MapSessionToDto(session);
    }

    // ── SESSIONS DISPONIBLES ────────────────────────────
    public async Task<List<SessionResponseDto>> GetSessionsDisponibles(string? genreMembre = null)
    {
        var query = db.Sessions
            .Include(s => s.Cours)
            .Include(s => s.Coach)
            .AsNoTracking()
            .Where(s =>
                s.Statut == "Planifie" &&
                s.PlacesDisponibles > 0 &&
                s.DateHeure > DateTime.UtcNow);

        if (!string.IsNullOrEmpty(genreMembre))
{
            var g = genreMembre.ToLower().Trim();

            query = query.Where(s =>
                s.Cours!.Genre == GenreCours.Mixte ||
                (g == "homme" && s.Cours!.Genre == GenreCours.Homme) ||
                (g == "femme" && s.Cours!.Genre == GenreCours.Femme));
        }

        return await query
            .OrderBy(s => s.DateHeure)
            .Select(s => MapSessionToDto(s))
            .ToListAsync();
    }

    // ── SESSIONS PAR COURS ──────────────────────────────
    public async Task<List<SessionResponseDto>> GetSessionsByCours(int coursId)
    {
        return await db.Sessions
            .Include(s => s.Cours)
            .Include(s => s.Coach)
            .AsNoTracking()
            .Where(s => s.CoursId == coursId)
            .OrderBy(s => s.DateHeure)
            .Select(s => MapSessionToDto(s))
            .ToListAsync();
    }

    // ── MAPPINGS ────────────────────────────────────────
    private static string GenreLabel(GenreCours g) => g switch
    {
        GenreCours.Homme => "Hommes uniquement",
        GenreCours.Femme => "Femmes uniquement",
        _ => "Mixte"
    };

    private static CoursResponseDto MapToDto(Cours c) => new(
        c.Id,
        c.Nom,
        c.Description,
        c.CapaciteMax,
        c.Genre.ToString(),
        GenreLabel(c.Genre),
        c.Actif,
        c.SessionsCours?.Count ?? 0,
        c.DateCreation
    );

    private static SessionResponseDto MapSessionToDto(Session_Cours s) => new(
        s.Id,
        s.CoursId,
        s.Cours!.Nom,
        s.Cours!.Genre.ToString(),
        GenreLabel(s.Cours!.Genre),
        s.Cours!.CapaciteMax,
        s.CoachId,
        s.Coach!.Nom,
        s.Coach!.Prenom,
        $"{s.Coach!.Prenom} {s.Coach!.Nom}",
        s.Coach!.Specialite,
        s.DateHeure,
        s.PlacesDisponibles,
        s.Statut
    );
}
