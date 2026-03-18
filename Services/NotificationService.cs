using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class NotificationService
    {
        private readonly AppDbContext db;
        private readonly ILogger<NotificationService> logger;

        public NotificationService(AppDbContext db, ILogger<NotificationService> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        // CONFIRMATION DE RÉSERVATION
        public async Task EnvoyerConfirmation(Reservation resa)
        {
            logger.LogInformation("EnvoyerConfirmation appelée pour ReservationId={Id}", resa.Id);

            var session = await db.Sessions
                .Include(s => s.Cours)
                .FirstOrDefaultAsync(s => s.Id == resa.SessionCoursId);

            if (session?.Cours == null)
            {
                logger.LogWarning("Session ou Cours introuvable pour SessionId={Id}", resa.SessionCoursId);
                return;
            }

            db.Notifications.Add(new Notification
            {
                Titre = "Réservation confirmée ✅",
                Contenu = $"Votre réservation pour '{session.Cours.Nom}' le {session.DateHeure:dd/MM/yyyy à HH:mm} est confirmée.",
                Type = "RESERVATION",
                UtilisateurId = resa.MembreId
            });

            await db.SaveChangesAsync();
            logger.LogInformation("Notification de confirmation créée pour MembreId={Id}", resa.MembreId);
        }

        // ANNULATION DE RÉSERVATION
        public async Task EnvoyerAnnulation(Reservation resa)
        {
            logger.LogInformation("EnvoyerAnnulation appelée pour ReservationId={Id}", resa.Id);

            var session = await db.Sessions
                .Include(s => s.Cours)
                .FirstOrDefaultAsync(s => s.Id == resa.SessionCoursId);

            db.Notifications.Add(new Notification
            {
                Titre = "Réservation annulée ❌",
                Contenu = session?.Cours != null
                    ? $"Votre réservation pour '{session.Cours.Nom}' le {session.DateHeure:dd/MM/yyyy à HH:mm} a été annulée."
                    : "Votre réservation a été annulée.",
                Type = "ANNULATION",
                UtilisateurId = resa.MembreId
            });

            await db.SaveChangesAsync();
            logger.LogInformation("Notification d'annulation créée pour MembreId={Id}", resa.MembreId);
        }

        // ANNULATION SESSION PAR ADMIN
        public async Task NotifierAnnulationSession(Session_Cours session)
        {
            logger.LogInformation("NotifierAnnulationSession appelée pour SessionId={Id}", session.Id);

            if (session.Cours == null)
                session = await db.Sessions.Include(s => s.Cours)
                    .FirstOrDefaultAsync(s => s.Id == session.Id) ?? session;

            var membreIds = await db.Reservations
                .Where(r => r.SessionCoursId == session.Id && r.Statut == StatutReservation.Confirmee)
                .Select(r => r.MembreId)
                .ToListAsync();

            foreach (var id in membreIds)
            {
                db.Notifications.Add(new Notification
                {
                    Titre = "Session annulée ❌",
                    Contenu = $"La session '{session.Cours?.Nom ?? "inconnue"}' le {session.DateHeure:dd/MM/yyyy à HH:mm} a été annulée par l'administration.",
                    Type = "ANNULATION",
                    UtilisateurId = id
                });
            }

            await db.SaveChangesAsync();
            logger.LogInformation("{Count} notification(s) créées pour SessionId={Id}", membreIds.Count, session.Id);
        }
    }
}