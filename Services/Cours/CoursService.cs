using backend.Data;
using backend.DTOs.Cours;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class CoursService(AppDbContext db)
{
    // ════════════════════════════════════════════════════
    // SUPER ADMIN — CRUD COURS
    // ════════════════════════════════════════════════════

    // GET ALL — admin (search + filtre genre)
    public async Task<List<CoursResponseDto>> GetAll(
        string? search = null,
        GenreCours? genre = null)
    {
        var q = db.Cours.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(c => c.Nom.ToLower()
                .Contains(search.ToLower()));

        if (genre.HasValue)
            q = q.Where(c => c.Genre == genre.Value);

        var list = await q
            .OrderBy(c => c.Nom)
            .AsNoTracking()
            .ToListAsync();

        // Compter sessions separement
        var ids = list.Select(c => c.Id).ToList();
        var counts = await db.Sessions
            .Where(s => ids.Contains(s.CoursId))
            .GroupBy(s => s.CoursId)
            .Select(g => new { CoursId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CoursId, x => x.Count);

        return list.Select(c => new CoursResponseDto(
            c.Id, c.Nom, c.Description, c.CapaciteMax,
            c.Genre.ToString(), GenreLabel(c.Genre),
            c.Actif,
            counts.GetValueOrDefault(c.Id, 0),
            c.DateCreation
        )).ToList();
    }

    // GET BY ID
    public async Task<CoursResponseDto> GetById(int id)
    {
        var c = await db.Cours
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException("Cours introuvable");

        var nbSessions = await db.Sessions
            .CountAsync(s => s.CoursId == id);

        return new CoursResponseDto(
            c.Id, c.Nom, c.Description, c.CapaciteMax,
            c.Genre.ToString(), GenreLabel(c.Genre),
            c.Actif, nbSessions, c.DateCreation);
    }

    // CREATE — Super Admin
    public async Task<CoursResponseDto> Create(CreateCoursDto dto)
    {
        if (dto.CapaciteMax > 30)
            throw new InvalidOperationException(
                "Capacite max autorisee : 30");

        var cours = new Cours
        {
            Nom = dto.Nom.Trim(),
            Description = dto.Description?.Trim(),
            CapaciteMax = dto.CapaciteMax,
            Genre = dto.Genre,
            Actif = true
        };

        db.Cours.Add(cours);
        await db.SaveChangesAsync();
        return new CoursResponseDto(
            cours.Id, cours.Nom, cours.Description, cours.CapaciteMax,
            cours.Genre.ToString(), GenreLabel(cours.Genre),
            cours.Actif, 0, cours.DateCreation);
    }

    // UPDATE — Super Admin (patch)
    public async Task<CoursResponseDto> Update(int id, UpdateCoursDto dto)
    {
        var c = await db.Cours.FindAsync(id)
            ?? throw new KeyNotFoundException("Cours introuvable");

        if (dto.Nom != null) c.Nom = dto.Nom.Trim();
        if (dto.Description != null) c.Description = dto.Description;
        if (dto.CapaciteMax != null) c.CapaciteMax = dto.CapaciteMax.Value;
        if (dto.Genre != null) c.Genre = dto.Genre.Value;
        if (dto.Actif != null) c.Actif = dto.Actif.Value;

        await db.SaveChangesAsync();
        return await GetById(id);
    }

    // DELETE — Super Admin
    public async Task Delete(int id)
    {
        var c = await db.Cours.FindAsync(id)
            ?? throw new KeyNotFoundException("Cours introuvable");

        var hasSessions = await db.Sessions
            .AnyAsync(s => s.CoursId == id && s.Statut == "Planifie");

        if (hasSessions)
            throw new InvalidOperationException(
                "Impossible : sessions planifiees existantes");

        db.Cours.Remove(c);
        await db.SaveChangesAsync();
    }

    // ════════════════════════════════════════════════════
    // SUPER ADMIN — PLANIFIER SESSION
    // ════════════════════════════════════════════════════

    public async Task<SessionResponseDto> PlanifierSession(
        PlanifierSessionDto dto)
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

    // ════════════════════════════════════════════════════
    // ADMINISTRATEUR — GÉRER SESSIONS
    // ════════════════════════════════════════════════════

    // ANNULER SESSION
    public async Task AnnulerSession(int sessionId)
    {
        var s = await db.Sessions.FindAsync(sessionId)
            ?? throw new KeyNotFoundException("Session introuvable");

        if (s.Statut == "Annule")
            throw new InvalidOperationException("Deja annulee");

        s.Statut = "Annule";
        await db.SaveChangesAsync();
    }

    // MODIFIER HORAIRE
    public async Task<SessionResponseDto> ModifierHoraire(
        int sessionId, ModifierHoraireDto dto)
    {
        var s = await db.Sessions
            .Include(s => s.Cours)
            .Include(s => s.Coach)
            .FirstOrDefaultAsync(s => s.Id == sessionId)
            ?? throw new KeyNotFoundException("Session introuvable");

        if (s.Statut == "Annule")
            throw new InvalidOperationException(
                "Session annulee, modification impossible");

        s.DateHeure = dto.NouvelleDate;
        await db.SaveChangesAsync();
        return MapSessionToDto(s);
    }

    // ════════════════════════════════════════════════════
    // MEMBRE — CONSULTER SESSIONS DISPONIBLES
    // ════════════════════════════════════════════════════

    // Sessions filtrées par genre du membre (lu depuis JWT)
    public async Task<List<SessionResponseDto>> GetSessionsDisponibles(
        string? genreMembre = null)
    {
        var q = db.Sessions
            .Include(s => s.Cours)
            .Include(s => s.Coach)
            .Where(s =>
                s.Statut == "Planifie" &&
                s.PlacesDisponibles > 0 &&
                s.DateHeure > DateTime.UtcNow);

        if (!string.IsNullOrEmpty(genreMembre))
        {
            var g = genreMembre.ToLower().Trim();
            q = q.Where(s =>
                s.Cours!.Genre == GenreCours.Mixte ||
                (g == "homme" && s.Cours!.Genre == GenreCours.Homme) ||
                (g == "femme" && s.Cours!.Genre == GenreCours.Femme));
        }

        var list = await q
            .OrderBy(s => s.DateHeure)
            .AsNoTracking()
            .ToListAsync();

        return list.Select(MapSessionToDto).ToList();
    }

    // Sessions par cours
    public async Task<List<SessionResponseDto>> GetSessionsByCours(int coursId)
    {
        var list = await db.Sessions
                .Include(s => s.Cours)
                .Include(s => s.Coach)
                .AsNoTracking()
                .Where(s => s.CoursId == coursId)
                .OrderBy(s => s.DateHeure)
                .ToListAsync();

        return list.Select(MapSessionToDto).ToList();
    }

    // ════ MAPPINGS ═══════════════════════════════════════


    private static string GenreLabel(GenreCours g) => g switch
    {
        GenreCours.Homme => "Hommes uniquement",
        GenreCours.Femme => "Femmes uniquement",
        _ => "Mixte"
    };

    private static SessionResponseDto MapSessionToDto(Session_Cours s) => new(
        s.Id,
        s.CoursId, s.Cours!.Nom,
        s.Cours!.Genre.ToString(), GenreLabel(s.Cours!.Genre),
        s.Cours!.CapaciteMax,
        s.CoachId, s.Coach!.Nom, s.Coach!.Prenom,
        $"{s.Coach!.Prenom} {s.Coach!.Nom}",
        s.Coach!.Specialite,
        s.DateHeure, s.PlacesDisponibles, s.Statut
    );
}