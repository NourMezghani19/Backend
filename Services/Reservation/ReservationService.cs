using backend.Data;
using backend.DTOs.Reservation;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.ReservationService
{
    public class ReservationService
    {
        private readonly AppDbContext db;
        private readonly NotificationService notif;

        public ReservationService(AppDbContext db, NotificationService notif)
        {
            this.db = db;
            this.notif = notif;
        }

        // --- SECTION ADMIN : RÉCUPÉRER TOUTES LES RÉSERVATIONS ---
        public async Task<List<ReservationResponseDto>> GetAllReservationsWithDetails()
        {
            return await db.Reservations
                .Include(r => r.Membre)
                .Include(r => r.SessionCours)
                    .ThenInclude(s => s!.Cours)
                .OrderByDescending(r => r.DateReservation)
                .Select(r => new ReservationResponseDto
                {
                    Id = r.Id,
                    MembreId = r.MembreId,
                    MembreNom = r.Membre!.Nom,
                    MembrePrenom = r.Membre!.Prenom,
                    SessionId = r.SessionCoursId,
                    NomCours = r.SessionCours!.Cours!.Nom,
                    HeureDebut = r.SessionCours.HeureDebut.ToString(@"hh\:mm"),
                    HeureFin = r.SessionCours.HeureFin.ToString(@"hh\:mm"),
                    Statut = r.Statut.ToString(),
                    PlacesRestantes = r.SessionCours.PlacesDisponibles
                })
                .ToListAsync();
        }

        // --- SECTION ADMIN : CONFIRMER OU ANNULER ---
        public async Task<(bool Success, string Message)> ChangerStatut(int id, string nouveauStatut)
        {
            var resa = await db.Reservations
                .Include(r => r.Membre)
                .Include(r => r.SessionCours)
                    .ThenInclude(s => s!.Cours)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (resa == null) return (false, "Réservation introuvable");

            if (nouveauStatut == "Confirmee")
            {
                resa.Statut = StatutReservation.Confirmee;
                await notif.EnvoyerConfirmation(resa);
            }
            else if (nouveauStatut == "Rejete" || nouveauStatut == "Rejetee" )
            {
             
               if (resa.Statut == StatutReservation.EnAttente ||
            resa.Statut == StatutReservation.Confirmee)
                {
                    if (resa.SessionCours != null) resa.SessionCours.PlacesDisponibles++;
                }

                resa.Statut = StatutReservation.Rejetee; 
                await notif.EnvoyerAnnulation(resa);
            }

            await db.SaveChangesAsync();
            return (true, $"La demande a été traitée ({nouveauStatut})");
        }

        // --- RÉSERVER (CÔTÉ MEMBRE) ---
        public async Task<(bool Success, string Message, ReservationResponseDto? Data)> Effectuer(CreateReservationDto dto)
         {
             var session = await db.Sessions
                 .Include(s => s.Cours)
                 .FirstOrDefaultAsync(s => s.Id == dto.SessionId);

             if (session == null) return (false, "Session introuvable", null);
             if (session.PlacesDisponibles <= 0) return (false, "La session est complète", null);

             var resa = new Reservation
             {
                 MembreId = dto.MembreId,
                 SessionCoursId = dto.SessionId,
                 DateReservation = DateTime.Now,
                 Statut = StatutReservation.EnAttente 
             };

             session.PlacesDisponibles--;

             db.Reservations.Add(resa);
             await db.SaveChangesAsync();


             var responseDto = new ReservationResponseDto
             {
                 Id = resa.Id,
                 MembreId = resa.MembreId,
                 SessionId = resa.SessionCoursId,
                 NomCours = session.Cours!.Nom,
                 HeureDebut = session.HeureDebut.ToString(@"hh\:mm"),
                 HeureFin = session.HeureFin.ToString(@"hh\:mm"),

                 Statut = resa.Statut.ToString(), 
                 PlacesRestantes = session.PlacesDisponibles
             };

             return (true, "Demande de réservation envoyée. En attente de validation par l'admin.", responseDto);
         }
        
      

        // --- ANNULER (MEMBRE) ---
        public async Task<(bool Success, string Message)> Annuler(int id, int membreId)
        {
            var resa = await db.Reservations
                .Include(r => r.SessionCours)
                .FirstOrDefaultAsync(r => r.Id == id && r.MembreId == membreId);

            if (resa == null) return (false, "Réservation introuvable");

            if (resa.Statut == StatutReservation.Annulee)
            {
                return (false, "Déjà annulée");
            }

            if (resa.Statut == StatutReservation.EnAttente ||
                resa.Statut == StatutReservation.Confirmee)
            {
                if (resa.SessionCours != null)
                    resa.SessionCours.PlacesDisponibles++;
            }

            resa.Statut = StatutReservation.Annulee;

            await db.SaveChangesAsync();

            return (true, "Réservation annulée avec succès");

        }

        // --- MES RÉSERVATIONS (MEMBRE) ---
        public async Task<List<ReservationResponseDto>> GetMesReservations(int membreId)
        {
            return await db.Reservations
                .Include(r => r.SessionCours)
                .ThenInclude(s => s!.Cours)
                //.Where(r => r.MembreId == membreId)
                //.Where(r => r.MembreId == membreId && r.Statut != StatutReservation.Annulee)
                .Where(r => r.MembreId == membreId
                     && r.Statut != StatutReservation.Annulee
                     && r.Statut != StatutReservation.Terminee)
                .OrderByDescending(r => r.DateReservation)
                .Select(r => new ReservationResponseDto
                {
                    Id = r.Id,
                    MembreId = r.MembreId,
                    SessionId = r.SessionCoursId,
                    NomCours = r.SessionCours!.Cours!.Nom,
                    HeureDebut = r.SessionCours!.HeureDebut.ToString(@"hh\:mm"),
                    HeureFin = r.SessionCours!.HeureFin.ToString(@"hh\:mm"),
                    Statut = r.Statut.ToString(),
                    PlacesRestantes = r.SessionCours.PlacesDisponibles
                })
                .ToListAsync();
        }
        // --- TERMINER LES RÉSERVATIONS DES SESSIONS PASSÉES ---
        public async Task TerminerSessionsPassees()
        {
            var maintenant = DateTime.Now;
            var jourActuel = maintenant.DayOfWeek.ToString(); // "Monday", "Tuesday"...
            var heureActuelle = maintenant.TimeOfDay;

            // Map anglais → français
            var jourMap = new Dictionary<string, string>
    {
        { "Monday",    "Lundi"    },
        { "Tuesday",   "Mardi"    },
        { "Wednesday", "Mercredi" },
        { "Thursday",  "Jeudi"    },
        { "Friday",    "Vendredi" },
        { "Saturday",  "Samedi"   },
        { "Sunday",    "Dimanche" }
    };

            var jourFr = jourMap[jourActuel];

            // Réservations confirmées dont la session est aujourd'hui ET l'heure de fin est passée
            var reservationsTerminees = await db.Reservations
                .Include(r => r.SessionCours)
                .Where(r =>
                    r.Statut == StatutReservation.Confirmee &&
                    r.SessionCours != null &&
                    r.SessionCours.JourSemaine == jourFr &&
                    r.SessionCours.HeureFin < heureActuelle
                )
                .ToListAsync();

            foreach (var resa in reservationsTerminees)
            {
                resa.Statut = StatutReservation.Terminee;

                // Remet la place disponible pour la semaine prochaine
                if (resa.SessionCours != null)
                    resa.SessionCours.PlacesDisponibles++;
            }

            if (reservationsTerminees.Any())
                await db.SaveChangesAsync();
        }
    }
}