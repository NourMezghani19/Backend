using backend.Data;
using backend.Models;
using backend.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class NotificationService
    {
        private readonly AppDbContext db;
        private readonly ILogger<NotificationService> logger;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            AppDbContext db,
            ILogger<NotificationService> logger,
            IHubContext<NotificationHub> hubContext)
        {
            this.db = db;
            this.logger = logger;
            this._hubContext = hubContext;
        }

        // CONFIRMATION DE RÉSERVATION
        public async Task EnvoyerConfirmation(Reservation resa)
        {
            logger.LogInformation("EnvoyerConfirmation appelée pour ReservationId={Id}", resa.Id);

            var session = await db.Sessions
                .Include(s => s.Cours)
                .FirstOrDefaultAsync(s => s.Id == resa.SessionCoursId);

            if (session?.Cours == null) return;

            var titre = "Réservation confirmée ✅";
            var contenu = $"Votre réservation pour '{session.Cours.Nom}'" +
                          $"de {session.HeureDebut:hh\\:mm} à {session.HeureFin:hh\\:mm} est confirmée.";
            db.Notifications.Add(new Notification
            {
                Titre = titre,
                Contenu = contenu,
                Type = "RESERVATION",
                UtilisateurId = resa.MembreId,
                DateEnvoi = DateTime.UtcNow,
                // ✅ NOUVEAU — on lie la notification à la session et à la réservation
                SessionId = resa.SessionCoursId,
                ReservationId = resa.Id
            });

            await db.SaveChangesAsync();

            await _hubContext.Clients.User(resa.MembreId.ToString())
                .SendAsync("ReceiveNotification", new { titre, contenu });

            logger.LogInformation("Notification push envoyée au MembreId={Id}", resa.MembreId);
        }

        // ANNULATION DE RÉSERVATION
        public async Task EnvoyerAnnulation(Reservation resa)
        {
            var session = await db.Sessions
                .Include(s => s.Cours)
                .FirstOrDefaultAsync(s => s.Id == resa.SessionCoursId);

            var titre = "Réservation annulée ❌";
            var contenu = session?.Cours != null
            ? $"Votre réservation pour '{session.Cours.Nom}' de {session.HeureDebut:hh\\:mm} à {session.HeureFin:hh\\:mm} a été annulée."
            : "Votre réservation a été annulée.";

            db.Notifications.Add(new Notification
            {
                Titre = titre,
                Contenu = contenu,
                Type = "ANNULATION",
                UtilisateurId = resa.MembreId,
                DateEnvoi = DateTime.UtcNow,
                // ✅ NOUVEAU
                SessionId = resa.SessionCoursId,
                ReservationId = resa.Id
            });

            await db.SaveChangesAsync();

            await _hubContext.Clients.User(resa.MembreId.ToString())
                .SendAsync("ReceiveNotification", new { titre, contenu });
        }

        // ANNULATION SESSION PAR ADMIN
        public async Task NotifierAnnulationSession(Session_Cours session)
        {
            if (session.Cours == null)
                session = await db.Sessions.Include(s => s.Cours)
                    .FirstOrDefaultAsync(s => s.Id == session.Id) ?? session;

            var reservations = await db.Reservations
                .Where(r => r.SessionCoursId == session.Id && r.Statut == StatutReservation.Confirmee)
                .ToListAsync();

            var titre = "Session annulée ❌";
            var contenu = $"La session '{session.Cours?.Nom ?? "inconnue"}' " +
                          $"de {session.HeureDebut:hh\\:mm} à {session.HeureFin:hh\\:mm} a été annulée par l'administration.";
            foreach (var resa in reservations)
            {
                db.Notifications.Add(new Notification
                {
                    Titre = titre,
                    Contenu = contenu,
                    Type = "ANNULATION",
                    UtilisateurId = resa.MembreId,
                    DateEnvoi = DateTime.UtcNow,
                    // ✅ NOUVEAU
                    SessionId = session.Id,
                    ReservationId = resa.Id
                });

                await _hubContext.Clients.User(resa.MembreId.ToString())
                    .SendAsync("ReceiveNotification", new { titre, contenu });
            }

            await db.SaveChangesAsync();
        }
    }
}