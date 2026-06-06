// Services/Avis/AvisService.cs
using backend.Data;
using backend.DTOs.Avis;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.Avis
{
    public class AvisService
    {
        private readonly AppDbContext db;
        public AvisService(AppDbContext db) { this.db = db; }

        // ── 1. CRÉER Membre ───────────────────────────────────
        public async Task<AvisResponseDto> CreerMembre(
      int membreId, CreerAvisDto dto)
        {
            var avis = new Models.Avis
            {
                MembreId = membreId,
                NomAnonyme = null,
                Commentaire = dto.Commentaire,
                Note = dto.Note,
                EstBloque = false,
                CreeLe = DateTime.UtcNow
            };

            db.Avis.Add(avis);
            await db.SaveChangesAsync();

            await db.Entry(avis)
                .Reference(a => a.Membre)
                .LoadAsync();

            return MapToResponse(avis);
        }

        // ── 2. CRÉER Anonyme ──────────────────────────────────
        public async Task<AvisResponseDto> CreerAnonyme(CreerAvisDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NomAnonyme))
                throw new ArgumentException(
                    "Le nom est obligatoire pour un avis anonyme.");

            var avis = new Models.Avis
            {
                MembreId = null,
                NomAnonyme = dto.NomAnonyme.Trim(),
                Commentaire = dto.Commentaire,
                Note = dto.Note,
                EstBloque = false,
                CreeLe = DateTime.UtcNow
            };

            this.db.Avis.Add(avis);
            await this.db.SaveChangesAsync();
            return MapToResponse(avis);
        }

        // ── 3. MODIFIER Membre ────────────────────────────────
        public async Task<AvisResponseDto> Modifier(
            int id, int membreId, CreerAvisDto dto)
        {
            var avis = await this.db.Avis
                .Include(a => a.Membre)
                .FirstOrDefaultAsync(a => a.Id == id)
                ?? throw new KeyNotFoundException("Avis introuvable.");

            if (avis.MembreId != membreId)
                throw new UnauthorizedAccessException();

            avis.Commentaire = dto.Commentaire;
            avis.Note = dto.Note;

            await this.db.SaveChangesAsync();
            return MapToResponse(avis);
        }

        // ── 4. SUPPRIMER Membre ───────────────────────────────
        public async Task Supprimer(int id, int membreId)
        {
            var avis = await this.db.Avis.FindAsync(id)
                ?? throw new KeyNotFoundException("Avis introuvable.");

            if (avis.MembreId != membreId)
                throw new UnauthorizedAccessException();

            this.db.Avis.Remove(avis);
            await this.db.SaveChangesAsync();
        }

        // ── 5. LISTE PUBLIQUE ─────────────────────────────────
        public async Task<List<AvisResponseDto>> GetVisibles()
        {
            return await this.db.Avis
                .Include(a => a.Membre)
                .Where(a => !a.EstBloque)
                .OrderByDescending(a => a.CreeLe)
                .Select(a => MapToResponse(a))
                .ToListAsync();
        }

        // ── 6. LISTE ADMIN ────────────────────────────────────
        public async Task<List<AvisResponseDto>> GetTous()
        {
            return await this.db.Avis
                .Include(a => a.Membre)
                .OrderByDescending(a => a.CreeLe)
                .Select(a => MapToResponse(a))
                .ToListAsync();
        }

        // ── 7. MON AVIS ───────────────────────────────────────
        public async Task<List<AvisResponseDto>> GetMesAvis(int membreId)
        {
            var avis = await db.Avis
                .Include(a => a.Membre)
                .Where(a => a.MembreId == membreId)
                .OrderByDescending(a => a.CreeLe)
                .Select(a => MapToResponse(a))
                .ToListAsync();

            return avis;
        }

        // ── 8. TOGGLE BLOQUE Admin ────────────────────────────
        public async Task<AvisResponseDto> ToggleBloque(int id)
        {
            var avis = await this.db.Avis
                .Include(a => a.Membre)
                .FirstOrDefaultAsync(a => a.Id == id)
                ?? throw new KeyNotFoundException("Avis introuvable.");

            avis.EstBloque = !avis.EstBloque;

            await this.db.SaveChangesAsync();
            return MapToResponse(avis);
        }

        // ── 9. SUPPRIMER Admin ────────────────────────────────
        public async Task SupprimerAdmin(int id)
        {
            var avis = await this.db.Avis.FindAsync(id)
                ?? throw new KeyNotFoundException("Avis introuvable.");

            this.db.Avis.Remove(avis);
            await this.db.SaveChangesAsync();
        }

        // ── 10. STATS Admin ───────────────────────────────────
        public async Task<object> GetStats()
        {
            var total = await this.db.Avis.CountAsync();
            var bloques = await this.db.Avis.CountAsync(a => a.EstBloque);
            var moyenne = total > 0
                ? await this.db.Avis
                    .Where(a => !a.EstBloque)
                    .AverageAsync(a => (double)a.Note)
                : 0;

            return new
            {
                total,
                bloques,
                visibles = total - bloques,
                moyenneNote = Math.Round(moyenne, 1)
            };
        }

        // ── HELPER ────────────────────────────────────────────
        private static AvisResponseDto MapToResponse(Models.Avis a) => new()
        {
            Id = a.Id,
            MembreId = a.MembreId,
            EstAnonyme = a.MembreId is null,
            AuteurNom = a.MembreId is not null
                ? (a.Membre is null ? ""
                    : $"{a.Membre.Prenom} {a.Membre.Nom}")
                : (a.NomAnonyme ?? "Visiteur"),
            Commentaire = a.Commentaire,
            Note = a.Note,
            EstBloque = a.EstBloque,
            CreeLe = a.CreeLe
        };
    }
}