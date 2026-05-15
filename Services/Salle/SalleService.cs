using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.DTOs.Salle;

namespace backend.Services;

public class SalleService(AppDbContext db)
{
    // ── GET ALL ────────────────────────────────────────────
    public async Task<List<SalleResponseDto>> GetAll()
    {
        return await db.Salles
            .AsNoTracking()
            .OrderBy(s => s.Nom)
            .Select(s => new SalleResponseDto(
                s.Id, s.Nom, s.Capacite,
                s.Description, s.Disponible))
            .ToListAsync();
    }

    // ── CREATE (Admin + SA) ────────────────────────────────
    public async Task<SalleResponseDto> Create(CreateSalleDto dto)
    {
        var salle = new Salle
        {
            Nom = dto.Nom.Trim(),
            Capacite = dto.Capacite,
            Description = dto.Description?.Trim(),
            Disponible = true
        };
        db.Salles.Add(salle);
        await db.SaveChangesAsync();
        return new SalleResponseDto(
            salle.Id, salle.Nom, salle.Capacite,
            salle.Description, salle.Disponible);
    }

    // ── DELETE (Admin + SA) ────────────────────────────────
    public async Task Delete(int id)
    {
        var salle = await db.Salles.FindAsync(id)
            ?? throw new KeyNotFoundException("Salle introuvable");

        var hasSessions = await db.Sessions
            .AnyAsync(s => s.SalleId == id
                        && s.Statut == "Planifie");

        if (hasSessions)
            throw new InvalidOperationException(
                "Salle occupée par des sessions planifiées");

        db.Salles.Remove(salle);
        await db.SaveChangesAsync();
    }

    // ── ASSIGNER SALLE À SESSION (Admin + SA) ──────────────
    public async Task AssignerSalle(int sessionId, int salleId)
    {
        var session = await db.Sessions.FindAsync(sessionId)
            ?? throw new KeyNotFoundException("Session introuvable");

        var salle = await db.Salles.FindAsync(salleId)
            ?? throw new KeyNotFoundException("Salle introuvable");

        // ✅ Conflit : même salle + même jour + chevauchement horaire
        var conflit = await db.Sessions
            .Where(s => s.SalleId == salleId
                     && s.Id != sessionId
                     && s.Statut == "Planifie"
                     && s.JourSemaine == session.JourSemaine
                     && s.HeureDebut < session.HeureFin
                     && s.HeureFin > session.HeureDebut)
            .AnyAsync();

        if (conflit)
            throw new InvalidOperationException(
                "Cette salle est déjà occupée sur ce créneau horaire");

        session.SalleId = salleId;
        await db.SaveChangesAsync();
    }

    // ── RETIRER SALLE D'UNE SESSION (Admin + SA) ──────────
    public async Task RetirerSalle(int sessionId)
    {
        var session = await db.Sessions.FindAsync(sessionId)
            ?? throw new KeyNotFoundException("Session introuvable");

        session.SalleId = null;
        await db.SaveChangesAsync();
    }

    // ── EMPLOI DE SALLE GLOBAL (Coach + Admin + SA) ─────────
    public async Task<IEnumerable<object>> GetEmploiGlobalAsync()
    {
        return await db.Sessions
            .Include(s => s.Cours)
            .Include(s => s.Coach)
            .Include(s => s.Salle)
            // 🔴 On retire "&& s.Statut == 'Planifie'" car la colonne n'existe pas ou pose problème
            .Where(s => s.SalleId != null)
            .Select(s => new {
                Id = s.Id,
                SalleId = s.SalleId,
                SalleNom = s.Salle != null ? s.Salle.Nom : "Non assignée",
                Jour = s.JourSemaine,
                HeureDebut = s.HeureDebut.ToString(),
                CoursNom = s.Cours != null ? s.Cours.Nom : "Sans cours",
                CoachNom = s.Coach != null ? $"{s.Coach.Prenom} {s.Coach.Nom}" : "Sans coach",
                PlacesDispo = s.PlacesDisponibles,
                PlacesTotal = s.Salle != null ? s.Salle.Capacite : 0
            })
            .ToListAsync();
    }

    // ── SESSIONS SANS SALLE ────────────────────────────────
    public async Task<List<EmploiSalleDto>> GetSansSalle()
    {
        var list = await db.Sessions
            .Include(s => s.Cours)
            .Include(s => s.Coach)
            .Where(s => s.Statut == "Planifie"
                     && s.SalleId == null)
            .OrderBy(s => s.JourSemaine)
            .ThenBy(s => s.HeureDebut)
            .AsNoTracking()
            .ToListAsync();

        return list.Select(s => new EmploiSalleDto(
            s.Id,
            null,
            "— Non assignée —",
            s.Cours!.Nom,
            $"{s.Coach!.Prenom} {s.Coach!.Nom}",
            s.JourSemaine,
            s.HeureDebut.ToString(@"hh\:mm"),
            s.HeureFin.ToString(@"hh\:mm"),
            s.PlacesDisponibles,
            0
        )).ToList();
    }
}

// ── DTOs ───────────────────────────────────────────────────

public record EmploiSalleDto(
    int SessionId,
    int? SalleId,
    string SalleNom,
    string CoursNom,
    string CoachNomComplet,
    string JourSemaine,
    string HeureDebut,   // formaté "08:00"
    string HeureFin,     // formaté "09:00"
    int PlacesDisponibles,
    int SalleCapacite
);