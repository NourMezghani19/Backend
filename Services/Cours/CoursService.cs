using backend.Data;
using backend.DTOs.Cours;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class CoursService(AppDbContext db)
{
    // SUPER ADMIN — CRUD COURS
    ////////////////////////////////////////////////////////////////////////  Get tous les  cours  
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
            c.JourSemaine ?? "", // Évite le null
            "", // HeureDebut (Le cours parent n'a pas d'heure fixe)
            ""  // HeureFin (Paramètre manquant qui causait l'erreur)
        )).ToList();
    }

    ////////////////////////////////////////////////////////////////////////  Get tous cours par id  
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
            c.Actif, nbSessions,
            c.JourSemaine ?? "",
            "", // HeureDebut
            ""  // HeureFin
        );
    }

    ////////////////////////////////////////////////////////////////////////  Create cours  
    public async Task<CoursResponseDto> Create(CreateCoursDto dto)
    {
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
             cours.Actif, 0,
             cours.JourSemaine,
             "", // HeureDebut
             ""  // HeureFin
         );
    }

    ////////////////////////////////////////////////////////////////////////  Update cours  
    public async Task<CoursResponseDto> Update(int id, UpdateCoursDto dto)
    {
        var c = await db.Cours.FindAsync(id)
            ?? throw new KeyNotFoundException("Cours introuvable");

        // Garder l'ancienne capacité pour vérifier si elle change
        int ancienneCapacite = c.CapaciteMax;

        if (dto.Nom != null) c.Nom = dto.Nom.Trim();
        if (dto.Description != null) c.Description = dto.Description;
        if (dto.CapaciteMax != null) c.CapaciteMax = dto.CapaciteMax.Value;
        if (dto.Genre != null) c.Genre = dto.Genre.Value;
        if (dto.Actif != null) c.Actif = dto.Actif.Value;

        // SI LA CAPACITÉ A CHANGÉ
        if (dto.CapaciteMax.HasValue && dto.CapaciteMax.Value != ancienneCapacite)
        {
            // 1. Récupérer toutes les sessions planifiées de ce cours
            var sessions = await db.Sessions
                .Where(s => s.CoursId == id && s.Statut == "Planifie")
                .ToListAsync();

            foreach (var session in sessions)
            {
                // 2. Compter les réservations en utilisant l'Enum StatutReservation
                var inscrits = await db.Reservations
                    .CountAsync(r => r.SessionCoursId == session.Id &&
                                (r.Statut == StatutReservation.Confirmee || r.Statut == StatutReservation.EnAttente));

                // 3. Mettre à jour les places disponibles
                session.PlacesDisponibles = c.CapaciteMax - inscrits;

                if (session.PlacesDisponibles < 0) session.PlacesDisponibles = 0;
            }
        }

        await db.SaveChangesAsync();
        return await GetById(id);
    }

    ////////////////////////////////////////////////////////////////////////  delete cours  
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

    // SUPER ADMIN — PLANIFIER SESSION
    ////////////////////////////////////////////////////////////////////////  Planifier sessions
    // SUPER ADMIN — PLANIFIER SESSION
    //////////////////////////////////////////////////////////////////////  Planifier sessions
    public async Task<SessionResponseDto> PlanifierSession(PlanifierSessionDto dto)
    {
        var cours = await db.Cours.FindAsync(dto.CoursId)
            ?? throw new KeyNotFoundException("Cours introuvable");

        var coach = await db.Coachs.FindAsync(dto.CoachId)
            ?? throw new KeyNotFoundException("Coach introuvable");

        // 🟢 Sécurité supplémentaire : Vérifier si la salle demandée existe vraiment en BDD
        if (dto.SalleId.HasValue && dto.SalleId.Value > 0)
        {
            var salleExiste = await db.Salles.AnyAsync(s => s.Id == dto.SalleId.Value);
            if (!salleExiste)
            {
                throw new KeyNotFoundException($"La salle avec l'ID {dto.SalleId.Value} n'existe pas en base de données.");
            }
        }

        // 🟢 NETTOYAGE ET PARSING SÉCURISÉ DES HORAIRES (Supprime AM/PM si présent)
        var heureDebutNettoyee = dto.HeureDebut.Replace("AM", "").Replace("PM", "").Trim();
        var heureFinNettoyee = dto.HeureFin.Replace("AM", "").Replace("PM", "").Trim();

        var debut = TimeSpan.Parse(heureDebutNettoyee);
        var fin = TimeSpan.Parse(heureFinNettoyee);
        string jourCible = dto.JourSemaine.Trim().ToLower();

        // 🟢 INTERCEPTION DES CONFLITS (Ignore les espaces et la casse)
        bool dejaPris = await db.Sessions.AnyAsync(s =>
            s.CoachId == dto.CoachId &&
            s.Statut == "Planifie" &&
            s.JourSemaine.Trim().ToLower() == jourCible &&
            ((debut >= s.HeureDebut && debut < s.HeureFin) ||
             (fin > s.HeureDebut && fin <= s.HeureFin) ||
             (debut <= s.HeureDebut && fin >= s.HeureFin)));

        if (dejaPris)
            throw new InvalidOperationException("Le coach a déjà un cours sur ce créneau horaire.");

        var session = new Session_Cours
        {
            CoursId = dto.CoursId,
            CoachId = dto.CoachId,
            // 🟢 Nettoyage strict pour éviter l'erreur FK MySQL
            SalleId = (dto.SalleId.HasValue && dto.SalleId.Value > 0) ? dto.SalleId.Value : null,
            JourSemaine = dto.JourSemaine,
            HeureDebut = debut,
            HeureFin = fin,
            PlacesDisponibles = cours.CapaciteMax,
            Statut = "Planifie"
        };

        db.Sessions.Add(session);
        await db.SaveChangesAsync();

        // 🟢 Chargement explicite de toutes les relations pour alimenter correctement le MapSessionToDto
        await db.Entry(session).Reference(s => s.Cours).LoadAsync();
        await db.Entry(session).Reference(s => s.Coach).LoadAsync();
        await db.Entry(session).Reference(s => s.Salle).LoadAsync();

        return MapSessionToDto(session);
    }

    // ADMINISTRATEUR — GÉRER SESSIONS
    ////////////////////////////////////////////////////////////////////////  Annuler une session 
    public async Task AnnulerSession(int sessionId)
    {
        var s = await db.Sessions.FindAsync(sessionId)
            ?? throw new KeyNotFoundException("Session introuvable");

        if (s.Statut == "Annule")
            throw new InvalidOperationException("Deja annulee");

        s.Statut = "Annule";
        await db.SaveChangesAsync();
    }

    ////////////////////////////////////////////////////////////////////////  Modifier l'horaire d'une session 
    public async Task<SessionResponseDto> ModifierHoraire(int sessionId, ModifierHoraireDto dto)
    {
        var s = await db.Sessions
            .Include(s => s.Cours)
            .Include(s => s.Coach)
            .Include(s => s.Salle)
            .FirstOrDefaultAsync(s => s.Id == sessionId)
            ?? throw new KeyNotFoundException("Session introuvable");

        if (s.Statut == "Annule")
            throw new InvalidOperationException("Session annulée, modification impossible");

        s.JourSemaine = dto.NouveauJour;
        s.HeureDebut = TimeSpan.Parse(dto.NouvelleHeureDebut);
        s.HeureFin = TimeSpan.Parse(dto.NouvelleHeureFin);

        await db.SaveChangesAsync();
        return MapSessionToDto(s);
    }

    // MEMBRE — CONSULTER SESSIONS DISPONIBLES
    ////////////////////////////////////////////////////////////////////////  Get Sessions Disponibles
    public async Task<List<SessionResponseDto>> GetSessionsDisponibles(string? genreMembre = null)
    {
        var q = db.Sessions
            .Include(s => s.Cours)
            .Include(s => s.Coach)
            .Include(s => s.Salle)
            .Where(s =>
                 s.Statut == "Planifie" &&
                s.PlacesDisponibles > 0 &&
                s.Cours!.Actif &&
                s.Coach!.Disponible);

        if (!string.IsNullOrEmpty(genreMembre))
        {
            var g = genreMembre.ToLower().Trim();
            q = q.Where(s =>
                s.Cours!.Genre == GenreCours.Mixte ||
                (g == "homme" && s.Cours!.Genre == GenreCours.Homme) ||
                (g == "femme" && s.Cours!.Genre == GenreCours.Femme) ||
                (g == "enfant" && s.Cours!.Genre == GenreCours.Enfant));
        }

        var sessions = await q.ToListAsync();

        var joursOrdre = new List<string> { "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi", "Samedi", "Dimanche" };

        return sessions
            .OrderBy(s => joursOrdre.IndexOf(s.JourSemaine))
            .ThenBy(s => s.HeureDebut)
            .Select(MapSessionToDto)
            .ToList();
    }

    ////////////////////////////////////////////////////////////////////////  Get Sessions By Cours
    public async Task<List<SessionResponseDto>> GetSessionsByCours(int coursId)
    {
        var sessions = await db.Sessions
                .Include(s => s.Cours)
                .Include(s => s.Coach)
                .Include(s => s.Salle)
                .AsNoTracking()
                .Where(s => s.CoursId == coursId)
                .ToListAsync();

        var joursOrdre = new List<string> {
            "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi", "Samedi", "Dimanche"
        };

        return sessions
            .OrderBy(s => joursOrdre.IndexOf(s.JourSemaine))
            .ThenBy(s => s.HeureDebut)
            .Select(MapSessionToDto)
            .ToList();
    }

    ////////////////////////////////////////////////////////////////////////  Genre Label
    private static string GenreLabel(GenreCours g) => g switch
    {
        GenreCours.Homme => "Hommes uniquement",
        GenreCours.Femme => "Femmes uniquement",
        GenreCours.Enfant => "Enfants uniquement",
        _ => "Mixte"
    };

    // Modification status
    public async Task<CoursResponseDto> ToggleActif(int id)
    {
        var c = await db.Cours.FindAsync(id)
            ?? throw new KeyNotFoundException("Cours introuvable");

        c.Actif = !c.Actif;
        await db.SaveChangesAsync();
        return await GetById(id);
    }

    ////////////////////////////////////////////////////////////////////////   Map Session To Dto
    private static SessionResponseDto MapSessionToDto(Session_Cours s) => new(
        s.Id,
        s.CoursId, s.Cours!.Nom,
        s.Cours!.Genre.ToString(), GenreLabel(s.Cours!.Genre),
        s.Cours!.CapaciteMax,
        s.CoachId, s.Coach!.Nom, s.Coach!.Prenom,
        $"{s.Coach!.Prenom} {s.Coach!.Nom}",
        s.Coach!.Specialite,
        s.JourSemaine,
        s.HeureDebut.ToString(@"hh\:mm"),
        s.HeureFin.ToString(@"hh\:mm"),
        s.PlacesDisponibles,
        s.Statut,          // Paramètre 16
s.SalleId,         // Paramètre 17
s.Salle != null ? s.Salle.Nom : "— Non assignée —", // Paramètre 18 👈 C'est lui !
s.Salle != null ? s.Salle.Capacite : s.Cours.CapaciteMax // Paramètre 19
    );
}