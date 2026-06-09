using backend.Data;
using backend.DTOs.Abonnement;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class AbonnementService
    {
        private readonly AppDbContext _db;
        private readonly NotificationService _notif;

        public AbonnementService(AppDbContext db, NotificationService notif)
        {
            _db = db;
            _notif = notif;
        }

        // ── ADMIN : affecter abonnement + créer paiement ──────────
        public async Task<AbonnementResponseDto> AffecterAbonnement(AffecterAbonnementDto dto)
        {
            var membre = await _db.Membres.FindAsync(dto.MembreId)
                            ?? throw new KeyNotFoundException("Membre introuvable.");

            var plan = await _db.PlansAbonnement.FindAsync(dto.PlanAbonnementId)
                ?? throw new KeyNotFoundException("Plan introuvable.");

            if (!plan.EstActif)
                throw new InvalidOperationException("Ce plan n'est pas actif.");

            // Annuler abonnement actif existant
            var actif = await _db.Abonnements
                .Where(a => a.MembreId == dto.MembreId && a.Statut == StatutAbonnement.Actif)
                .FirstOrDefaultAsync();

            if (actif != null)
            {
                actif.Statut = StatutAbonnement.Annule;
                actif.ModifieLe = DateTime.UtcNow;
            }

            var dateDebut = dto.DateDebut?.ToUniversalTime() ?? DateTime.UtcNow;
            var dateFin = dateDebut.AddMonths(plan.DureeEnMois);

            var abonnement = new Abonnement
            {
                MembreId = dto.MembreId,
                PlanAbonnementId = plan.Id,
                DateDebut = dateDebut,
                DateFin = dateFin,
                Statut = StatutAbonnement.Actif
            };

            _db.Abonnements.Add(abonnement);
            await _db.SaveChangesAsync(); // nécessaire pour avoir abonnement.Id

            // Créer le paiement lié
            var paiement = new Paiement
            {
                MembreId = dto.MembreId,
                AbonnementId = abonnement.Id,
                Montant = plan.Prix,
                Statut = StatutPaiement.Valide,
                Reference = GenererReference(),
                DatePaiement = DateTime.UtcNow
            };

            _db.Paiements.Add(paiement);
            await _db.SaveChangesAsync();

            // Notifier via votre NotificationService existant
            await _notif.NotifierAbonnementAffecte(
     dto.MembreId, plan.Nom, dateDebut, dateFin);

            return await GetResponseById(abonnement.Id);
        }

        // ── ADMIN : liste tous les abonnements ────────────────────
        public async Task<List<AbonnementResponseDto>> GetTous(int? membreId = null)
        {
            var query = _db.Abonnements
                .Include(a => a.Membre)
                .Include(a => a.Plan)
                .AsQueryable();

            if (membreId.HasValue)
                query = query.Where(a => a.MembreId == membreId.Value);

            var list = await query.OrderByDescending(a => a.CreeLe).ToListAsync();
            return list.Select(MapDto).ToList();
        }

        // ── MEMBRE : son abonnement actif ─────────────────────────
        public async Task<AbonnementResponseDto?> GetAbonnementActif(int membreId)
        {
            var ab = await _db.Abonnements
                .Include(a => a.Membre)
                .Include(a => a.Plan)
                .Where(a => a.MembreId == membreId && a.Statut == StatutAbonnement.Actif)
                .FirstOrDefaultAsync();

            return ab == null ? null : MapDto(ab);
        }

        // ── HELPERS ───────────────────────────────────────────────
        private async Task<AbonnementResponseDto> GetResponseById(int id)
        {
            var a = await _db.Abonnements
                .Include(a => a.Membre)
                .Include(a => a.Plan)
                .FirstAsync(a => a.Id == id);
            return MapDto(a);
        }

        private static AbonnementResponseDto MapDto(Abonnement a) => new()
        {
            Id = a.Id,
            MembreId = a.MembreId,
            MembreNom = a.Membre != null ? $"{a.Membre.Prenom} {a.Membre.Nom}" : "", // 👈
            MembreEmail = a.Membre?.Email ?? "",                                       // 👈
            PlanId = a.PlanAbonnementId,
            PlanNom = a.Plan?.Nom ?? "",
            DureeEnMois = a.Plan?.DureeEnMois ?? 0,
            DureeLibelle = (a.Plan?.DureeEnMois) switch
            {
                1 => "1 Mois",
                3 => "3 Mois",
                6 => "6 Mois",
                12 => "1 An",
                _ => $"{a.Plan?.DureeEnMois} Mois"
            },
            Prix = a.Plan?.Prix ?? 0,
            DateDebut = a.DateDebut,
            DateFin = a.DateFin,
            Statut = a.Statut.ToString(),
            JoursRestants = Math.Max(0, (int)(a.DateFin - DateTime.UtcNow).TotalDays),
            CreeLe = a.CreeLe
        };


        private static string GenererReference()
            => $"PAY-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }
}