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

        if (dto.Nom != null) c.Nom = dto.Nom.Trim();
        if (dto.Description != null) c.Description = dto.Description;
        if (dto.CapaciteMax != null) c.CapaciteMax = dto.CapaciteMax.Value;
        if (dto.Genre != null) c.Genre = dto.Genre.Value;
        if (dto.Actif != null) c.Actif = dto.Actif.Value;

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

    public async Task<SessionResponseDto> PlanifierSession(PlanifierSessionDto dto)
    {

        var cours = await db.Cours.FindAsync(dto.CoursId)
            ?? throw new KeyNotFoundException("Cours introuvable");
        // ✅ Vérification cours actif
      /*  if (!cours.Actif)
            throw new InvalidOperationException(
                "Impossible de planifier : le cours est inactif");*/

        var coach = await db.Coachs.FindAsync(dto.CoachId)
            ?? throw new KeyNotFoundException("Coach introuvable");
        // ✅ Vérification coach disponible (déjà existante)
       /* if (!coach.Disponible)
            throw new InvalidOperationException("Coach non disponible");*/


        // --- NOUVELLE VALIDATION ANTI-CHEVAUCHEMENT ---
        var debut = TimeSpan.Parse(dto.HeureDebut);
        var fin = TimeSpan.Parse(dto.HeureFin);

        bool dejaPris = await db.Sessions.AnyAsync(s =>
            s.CoachId == dto.CoachId &&
            s.JourSemaine == dto.JourSemaine &&
            s.Statut == "Planifie" &&
            ((debut >= s.HeureDebut && debut < s.HeureFin) || // Nouveau début pendant une session existante
             (fin > s.HeureDebut && fin <= s.HeureFin) ||      // Nouvelle fin pendant une session existante
             (debut <= s.HeureDebut && fin >= s.HeureFin)));   // Session existante englobée par la nouvelle

        if (dejaPris)
            throw new InvalidOperationException("Le coach a déjà un cours sur ce créneau horaire.");

        var session = new Session_Cours
        {
            CoursId = dto.CoursId,
            CoachId = dto.CoachId,
            JourSemaine = dto.JourSemaine,
            HeureDebut = debut, // Stocké en TimeSpan
            HeureFin = fin,
            PlacesDisponibles = cours.CapaciteMax,
            Statut = "Planifie"
        };

        db.Sessions.Add(session);
        await db.SaveChangesAsync();

        await db.Entry(session).Reference(s => s.Cours).LoadAsync();
        await db.Entry(session).Reference(s => s.Coach).LoadAsync();

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
    public async Task<List<SessionResponseDto>> GetSessionsDisponibles(
        string? genreMembre = null)
    {
        var q = db.Sessions
            .Include(s => s.Cours)
            .Include(s => s.Coach)
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
                (g == "femme" && s.Cours!.Genre == GenreCours.Femme));
        }

        var sessions = await q.ToListAsync(); // On récupère la liste en mémoire pour trier

        var joursOrdre = new List<string> { "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi", "Samedi", "Dimanche" };

        return sessions
            .OrderBy(s => joursOrdre.IndexOf(s.JourSemaine)) // Tri par jour (0 à 6)
            .ThenBy(s => s.HeureDebut)                            // Puis par heure (ex: "08:00" avant "14:00")
            .Select(MapSessionToDto)
            .ToList();
    }

    ////////////////////////////////////////////////////////////////////////  Get Sessions By Cours
   public async Task<List<SessionResponseDto>> GetSessionsByCours(int coursId)
{
    // 1. Récupération des données sans le OrderBy (car SQL ne connaît pas l'ordre des jours)
    var sessions = await db.Sessions
            .Include(s => s.Cours)
            .Include(s => s.Coach)
            .AsNoTracking()
            .Where(s => s.CoursId == coursId)
            .ToListAsync();

    // 2. Définition de l'ordre logique de la semaine
    var joursOrdre = new List<string> { 
        "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi", "Samedi", "Dimanche" 
    };

    // 3. Tri en mémoire (C#) et Mapping vers le DTO
    return sessions
        .OrderBy(s => joursOrdre.IndexOf(s.JourSemaine)) // Trie par l'index (0 à 6)
        .ThenBy(s => s.HeureDebut)                            // Trie par l'heure (ex: "08:00" avant "14:00")
        .Select(MapSessionToDto)
        .ToList();
}

    ////////////////////////////////////////////////////////////////////////  Genre Label

    private static string GenreLabel(GenreCours g) => g switch
    {
        GenreCours.Homme => "Hommes uniquement",
        GenreCours.Femme => "Femmes uniquement",
        _ => "Mixte"
    };
    // modification status
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
    s.HeureDebut.ToString(@"hh\:mm"), // Format "14:30"
    s.HeureFin.ToString(@"hh\:mm"),   // Format "16:00"
    s.PlacesDisponibles,
    s.Statut
);
}