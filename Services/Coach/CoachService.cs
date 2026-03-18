using backend.Data;
using backend.DTOs.Coach;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.Coach
{
    public class CoachService
    {
        private readonly AppDbContext db;

        public CoachService(AppDbContext db)
        {
            this.db = db;
        }

        // ── GET ALL ──────────────────────────────────────
        // ── GET ALL ──────────────────────────────────────
        public async Task<List<CoachResponseDto>> GetAll()
        {
            var coachs = await this.db.Coachs
                //.Include(c => c.Sessions)
                .OrderBy(c => c.Nom)
                .ToListAsync();  // ← d'abord charger en mémoire

            return coachs.Select(c => MapToDto(c)).ToList();  // ← puis mapper
        }

        // ── GET BY ID ────────────────────────────────────
        public async Task<CoachResponseDto> GetById(int id)
        {
            var c = await this.db.Coachs
               // .Include(c => c.Sessions)
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new KeyNotFoundException("Coach introuvable");

            return MapToDto(c);
        }

        // ── CREATE ───────────────────────────────────────
        public async Task<CoachResponseDto> Create(CreateCoachDto dto)
        {
            if (await this.db.Coachs.AnyAsync(c => c.Email == dto.Email.ToLower().Trim()))
                throw new InvalidOperationException("Email déjà utilisé");
            if (dto.Telephone != null &&
                await this.db.Coachs.AnyAsync(c => c.Telephone == dto.Telephone.Trim()))
                throw new InvalidOperationException("Téléphone déjà utilisé");

            var coach = new Models.Coach
            {
                Nom = dto.Nom.Trim(),
                Prenom = dto.Prenom.Trim(),
                Email = dto.Email.ToLower().Trim(),
                Telephone = dto.Telephone?.Trim(),
                Specialite = dto.Specialite.Trim(),
                PhotoUrl = dto.PhotoUrl?.Trim(),
                Disponible = true
            };

            this.db.Coachs.Add(coach);
            await this.db.SaveChangesAsync();
            return MapToDto(coach);
        }

        // ── UPDATE (patch) ───────────────────────────────
        public async Task<CoachResponseDto> Update(int id, UpdateCoachDto dto)
        {   
            var c = await this.db.Coachs
               // .Include(c => c.Sessions)
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new KeyNotFoundException("Coach introuvable");
            if (dto.Telephone != null &&
                await this.db.Coachs.AnyAsync(c => c.Telephone == dto.Telephone.Trim() && c.Id != id))
                throw new InvalidOperationException("Téléphone déjà utilisé");

            if (dto.Nom != null) c.Nom = dto.Nom.Trim();
            if (dto.Prenom != null) c.Prenom = dto.Prenom.Trim();
            if (dto.Specialite != null) c.Specialite = dto.Specialite.Trim();
            if (dto.Telephone != null) c.Telephone = dto.Telephone.Trim();
            if (dto.PhotoUrl != null) c.PhotoUrl = dto.PhotoUrl.Trim();
            if (dto.Disponible != null) c.Disponible = dto.Disponible.Value;

            await this.db.SaveChangesAsync();
            return MapToDto(c);
        }

        // ── DELETE ───────────────────────────────────────
        public async Task<bool> Delete(int id)
        {
            var coach = await this.db.Coachs.FindAsync(id);
            if (coach == null) return false;

            this.db.Coachs.Remove(coach);
            await this.db.SaveChangesAsync();
            return true;
        }

        // ── MAP ──────────────────────────────────────────
        private static CoachResponseDto MapToDto(Models.Coach c) => new CoachResponseDto
        {
            Id = c.Id,
            Nom = c.Nom,
            Prenom = c.Prenom,
            Email = c.Email,
            Telephone = c.Telephone,
            Specialite = c.Specialite,
            Disponible = c.Disponible,
            PhotoUrl = c.PhotoUrl,
            NbSessions = c.Sessions.Count,
            DateCreation = c.DateCreation
        };
    }
}