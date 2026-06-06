// Services/PlanAbonnement/PlanAbonnementService.cs
using backend.Data;
using backend.DTOs.PlanAbonnement;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.PlanAbonnement
{
    public class PlanAbonnementService
    {
        private readonly AppDbContext db;

        public PlanAbonnementService(AppDbContext db)
        {
            this.db = db;
        }

        // ── 1. CRÉER ──────────────────────────────────────────────
        public async Task<PlanResponseDto> Creer(CreerPlanDto dto)
        {
            bool existe = await this.db.PlansAbonnement.AnyAsync(p =>
                p.Nom == dto.Nom && p.DureeEnMois == dto.DureeEnMois);

            if (existe)
                throw new InvalidOperationException(
                    "Un plan avec ce nom et cette durée existe déjà.");

            var plan = new Models.PlanAbonnement
            {
                Nom = dto.Nom,
                Description = dto.Description,
                DureeEnMois = dto.DureeEnMois,
                Prix = dto.Prix,
                EstActif = dto.EstActif,
                CreeLe = DateTime.UtcNow,
                ModifieLe = DateTime.UtcNow
            };

            this.db.PlansAbonnement.Add(plan);
            await this.db.SaveChangesAsync();
            return MapToResponse(plan);
        }

        // ── 2. LISTE COMPLÈTE ─────────────────────────────────────
        public async Task<List<PlanResponseDto>> GetTous(bool? actifSeulement = null)
        {
            var query = this.db.PlansAbonnement.AsQueryable();

            if (actifSeulement == true)
                query = query.Where(p => p.EstActif);

            return await query
                .OrderBy(p => p.DureeEnMois)
                .ThenBy(p => p.Prix)
                .Select(p => MapToResponse(p))
                .ToListAsync();
        }

        // ── 3. GET PAR ID ─────────────────────────────────────────
        public async Task<PlanResponseDto> GetById(int id)
        {
            var plan = await this.db.PlansAbonnement.FindAsync(id)
                ?? throw new KeyNotFoundException($"Plan #{id} introuvable.");

            return MapToResponse(plan);
        }

        // ── 4. MODIFIER ───────────────────────────────────────────
        public async Task<PlanResponseDto> Modifier(int id, CreerPlanDto dto)
        {
            var plan = await this.db.PlansAbonnement.FindAsync(id)
                ?? throw new KeyNotFoundException($"Plan #{id} introuvable.");

            bool doublon = await this.db.PlansAbonnement.AnyAsync(p =>
                p.Id != id &&
                p.Nom == dto.Nom &&
                p.DureeEnMois == dto.DureeEnMois);

            if (doublon)
                throw new InvalidOperationException(
                    "Un autre plan avec ce nom et cette durée existe déjà.");

            plan.Nom = dto.Nom;
            plan.Description = dto.Description;
            plan.DureeEnMois = dto.DureeEnMois;
            plan.Prix = dto.Prix;
            plan.EstActif = dto.EstActif;
            plan.ModifieLe = DateTime.UtcNow;

            await this.db.SaveChangesAsync();
            return MapToResponse(plan);
        }

        // ── 5. TOGGLE ACTIF/INACTIF ───────────────────────────────
        public async Task<PlanResponseDto> ToggleActif(int id)
        {
            var plan = await this.db.PlansAbonnement.FindAsync(id)
                ?? throw new KeyNotFoundException($"Plan #{id} introuvable.");

            plan.EstActif = !plan.EstActif;
            plan.ModifieLe = DateTime.UtcNow;
            await this.db.SaveChangesAsync();
            return MapToResponse(plan);
        }

        // ── 6. SUPPRIMER ──────────────────────────────────────────
        public async Task Supprimer(int id)
        {
            var plan = await this.db.PlansAbonnement.FindAsync(id)
                ?? throw new KeyNotFoundException($"Plan #{id} introuvable.");

            this.db.PlansAbonnement.Remove(plan);
            await this.db.SaveChangesAsync();
        }

        // ── HELPER ────────────────────────────────────────────────
        private static PlanResponseDto MapToResponse(Models.PlanAbonnement p) => new()
        {
            Id = p.Id,
            Nom = p.Nom,
            Description = p.Description,
            DureeEnMois = p.DureeEnMois,
            DureeLibelle = p.DureeEnMois switch
            {
                1 => "1 Mois",
                3 => "3 Mois",
                6 => "6 Mois",
                12 => "1 An",
                _ => $"{p.DureeEnMois} Mois"
            },
            Prix = p.Prix,
            EstActif = p.EstActif,
            CreeLe = p.CreeLe,
            ModifieLe = p.ModifieLe
        };
    }
}