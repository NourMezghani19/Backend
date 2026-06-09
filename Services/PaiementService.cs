using backend.Data;
using backend.DTOs.Paiement;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class PaiementService
    {
        private readonly AppDbContext _db;
        private readonly NotificationService _notif;

        public PaiementService(AppDbContext db, NotificationService notif)
        {
            _db = db;
            _notif = notif;
        }

        // ── ADMIN/SUPERADMIN : liste avec filtres ─────────────────
        public async Task<List<PaiementResponseDto>> GetTous(FiltragePaiementDto f)
        {
            var q = _db.Paiements
                .Include(p => p.Membre)
                .Include(p => p.Abonnement).ThenInclude(a => a!.Plan)
                .AsQueryable();

            if (f.MembreId.HasValue) q = q.Where(p => p.MembreId == f.MembreId.Value);
            if (f.DateDebut.HasValue) q = q.Where(p => p.DatePaiement >= f.DateDebut.Value);
            if (f.DateFin.HasValue) q = q.Where(p => p.DatePaiement <= f.DateFin.Value);
            if (f.Annee.HasValue) q = q.Where(p => p.DatePaiement.Year == f.Annee.Value);
            if (f.Mois.HasValue) q = q.Where(p => p.DatePaiement.Month == f.Mois.Value);
            if (f.Statut != null && Enum.TryParse<StatutPaiement>(f.Statut, out var st))
                q = q.Where(p => p.Statut == st);

            return await q.OrderByDescending(p => p.DatePaiement)
                          .Select(p => MapDto(p)).ToListAsync();
        }

        // ── MEMBRE : son historique ───────────────────────────────
        public async Task<List<PaiementResponseDto>> GetMesPaiements(int membreId, int? annee)
        {
            var q = _db.Paiements
                .Include(p => p.Membre)
                .Include(p => p.Abonnement).ThenInclude(a => a!.Plan)
                .Where(p => p.MembreId == membreId);

            if (annee.HasValue)
                q = q.Where(p => p.DatePaiement.Year == annee.Value);

            return await q.OrderByDescending(p => p.DatePaiement)
                          .Select(p => MapDto(p)).ToListAsync();
        }

        // ── ADMIN : stats journalières ────────────────────────────
        public async Task<List<StatJournalierDto>> GetStats(
            DateTime? debut, DateTime? fin, int? annee, int? mois)
        {
            var q = _db.Paiements.Where(p => p.Statut == StatutPaiement.Valide);

            if (debut.HasValue) q = q.Where(p => p.DatePaiement >= debut.Value);
            if (fin.HasValue) q = q.Where(p => p.DatePaiement <= fin.Value);
            if (annee.HasValue) q = q.Where(p => p.DatePaiement.Year == annee.Value);
            if (mois.HasValue) q = q.Where(p => p.DatePaiement.Month == mois.Value);

            return await q
                .GroupBy(p => p.DatePaiement.Date)
                .Select(g => new StatJournalierDto
                {
                    Jour = g.Key,
                    TotalMontant = g.Sum(p => p.Montant),
                    NombrePaiements = g.Count()
                })
                .OrderBy(s => s.Jour)
                .ToListAsync();
        }

        // ── ADMIN : valider paiement ──────────────────────────────
        public async Task<PaiementResponseDto> Valider(int id)
        {
            var p = await _db.Paiements
                .Include(p => p.Membre)
                .Include(p => p.Abonnement).ThenInclude(a => a!.Plan)
                .FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new KeyNotFoundException($"Paiement #{id} introuvable.");

            p.Statut = StatutPaiement.Valide;
            await _db.SaveChangesAsync();

            await _notif.NotifierPaiementConfirme(
                p.MembreId.ToString(), p.Montant, p.Reference);

            return MapDto(p);
        }

        // ── DÉTAIL ────────────────────────────────────────────────
        public async Task<PaiementResponseDto> GetById(int id)
        {
            var p = await _db.Paiements
                .Include(p => p.Membre)
                .Include(p => p.Abonnement).ThenInclude(a => a!.Plan)
                .FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new KeyNotFoundException($"Paiement #{id} introuvable.");

            return MapDto(p);
        }

        private static PaiementResponseDto MapDto(Paiement p) => new()
        {
            Id = p.Id,
            Reference = p.Reference,
            MembreId = p.MembreId,
            MembreNom = p.Membre != null ? $"{p.Membre.Prenom} {p.Membre.Nom}" : "", 
            MembreEmail = p.Membre?.Email ?? "",
            AbonnementId = p.AbonnementId,
            PlanNom = p.Abonnement?.Plan?.Nom ?? "",
            Montant = p.Montant,
            Statut = p.Statut.ToString(),
            DatePaiement = p.DatePaiement,
            CreeLe = p.CreeLe
        };
    }
}