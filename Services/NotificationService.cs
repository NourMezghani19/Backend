using backend.Data;
using backend.Models;
using backend.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using backend.DTOs;

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
                .Where(r => r.SessionCoursId == session.Id && r.Statut == StatutReservation.EnAttente)
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
                    SessionId = session.Id,
                    ReservationId = resa.Id
                });

                await _hubContext.Clients.User(resa.MembreId.ToString())
                    .SendAsync("ReceiveNotification", new { titre, contenu });
            }

            await db.SaveChangesAsync();
        }
        // Coller ces méthodes dans votre NotificationService.cs existant

        public async Task NotifierAbonnementAffecte(
    int membreId, string planNom, DateTime dateDebut, DateTime dateFin)
        {
            var titre = "Abonnement activé ✅";
            var contenu = $"Votre abonnement '{planNom}' est actif du " +
                          $"{dateDebut:dd/MM/yyyy} au {dateFin:dd/MM/yyyy}.";

            db.Notifications.Add(new Notification  
            {
                Titre = titre,
                Contenu = contenu,                 
                Type = "ABONNEMENT",
                UtilisateurId = membreId,
                DateEnvoi = DateTime.UtcNow,       
                Lue = false                         
            });

            await db.SaveChangesAsync();

            await _hubContext.Clients.User(membreId.ToString())
                .SendAsync("ReceiveNotification", new { titre, contenu });
        }
        public async Task NotifierPaiementConfirme(string membreId, decimal montant, string reference)
        {
            var titre = "Paiement confirmé ✅";
            var contenu = $"Votre paiement de {montant:F2} TND (Réf: {reference}) a été validé.";

            db.Notifications.Add(new Notification
            {
                Titre = titre,
                Contenu = contenu,
                Type = "PAIEMENT",
                UtilisateurId = int.Parse(membreId),
                DateEnvoi = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            await _hubContext.Clients.User(membreId)
                .SendAsync("ReceiveNotification", new { titre, contenu });
        }

        public async Task NotifierExpirationProche(int membreId, string planNom, DateTime dateFin)
        {
            var titre = "Abonnement bientôt expiré ⚠️";
            var contenu = $"Votre abonnement '{planNom}' expire le {dateFin:dd/MM/yyyy}. " +
                          $"Contactez l'administration pour le renouveler.";

            db.Notifications.Add(new Notification
            {
                Titre = titre,
                Contenu = contenu,
                Type = "EXPIRATION",
                UtilisateurId = membreId,
                DateEnvoi = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            await _hubContext.Clients.User(membreId.ToString())
                .SendAsync("ReceiveNotification", new { titre, contenu });
        }

        public async Task NotifierAbonnementExpire(int membreId, string planNom)
        {
            var titre = "Abonnement expiré ❌";
            var contenu = $"Votre abonnement '{planNom}' a expiré. " +
                          $"Contactez l'administration pour le renouveler.";

            db.Notifications.Add(new Notification
            {
                Titre = titre,
                Contenu = contenu,
                Type = "EXPIRATION",
                UtilisateurId = membreId,
                DateEnvoi = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            await _hubContext.Clients.User(membreId.ToString())
                .SendAsync("ReceiveNotification", new { titre, contenu });
        }

        public async Task<List<NotificationDto>> GetMesNotifications(int utilisateurId)
        {
            return await db.Notifications
                .Where(n => n.UtilisateurId == utilisateurId)
                .OrderByDescending(n => n.DateEnvoi)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Titre = n.Titre,
                    Contenu = n.Contenu,
                    Type = n.Type,
                    Lue = n.Lue,
                    DateEnvoi = n.DateEnvoi
                })
                .ToListAsync();
        }

        public async Task MarquerLue(int id, int utilisateurId)
        {
            var n = await db.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UtilisateurId == utilisateurId)
                ?? throw new KeyNotFoundException("Notification introuvable.");

            n.Lue = true;
            await db.SaveChangesAsync();
        }

    }
}