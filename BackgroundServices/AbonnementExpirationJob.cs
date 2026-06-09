using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;

namespace backend.BackgroundServices
{
    public class AbonnementExpirationJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AbonnementExpirationJob> _logger;

        public AbonnementExpirationJob(
            IServiceScopeFactory scopeFactory,
            ILogger<AbonnementExpirationJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Traiter();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur dans AbonnementExpirationJob.");
                }
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task Traiter()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var notif = scope.ServiceProvider.GetRequiredService<NotificationService>();

            var maintenant = DateTime.UtcNow;
            var dans4Jours = maintenant.AddDays(4);

            // 1. Rappels expiration proche
            var aExpirer = await db.Abonnements
                .Include(a => a.Plan)
                .Where(a => a.Statut == StatutAbonnement.Actif
                         && a.DateFin >= maintenant
                         && a.DateFin <= dans4Jours)
                .ToListAsync();

            foreach (var ab in aExpirer)
            {
                // Éviter doublon notif dans les dernières 23h
                bool dejaNotifie = await db.Notifications.AnyAsync(n =>
                    n.UtilisateurId == ab.MembreId &&
                    n.Type == "EXPIRATION" &&
                    n.DateEnvoi >= maintenant.AddHours(-23));

                if (!dejaNotifie)
                    await notif.NotifierExpirationProche(
                        ab.MembreId, ab.Plan?.Nom ?? "", ab.DateFin);
            }

            // 2. Marquer expirés
            var expires = await db.Abonnements
                .Include(a => a.Plan)
                .Where(a => a.Statut == StatutAbonnement.Actif && a.DateFin < maintenant)
                .ToListAsync();

            foreach (var ab in expires)
            {
                ab.Statut = StatutAbonnement.Expire;
                ab.ModifieLe = maintenant;
                await notif.NotifierAbonnementExpire(ab.MembreId, ab.Plan?.Nom ?? "");
            }

            await db.SaveChangesAsync();

            _logger.LogInformation(
                "Job expirations: {R} rappels envoyés, {E} abonnements expirés.",
                aExpirer.Count, expires.Count);
        }
    }
}