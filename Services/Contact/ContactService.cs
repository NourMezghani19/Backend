// Services/Contact/ContactService.cs
using backend.Data;
using backend.DTOs.Contact;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.Contact
{
    public class ContactService
    {
        private readonly AppDbContext db;

        public ContactService(AppDbContext db)
        {
            this.db = db;
        }

        // ── 1. ENVOYER (Membre) ───────────────────────────────
        public async Task<MessageResponseDto> Envoyer(
            EnvoyerMessageDto dto, int? membreId = null)
        {
            var msg = new Models.MessageContact
            {
                NomPrenom = dto.NomPrenom,
                Email = dto.Email,
                Sujet = dto.Sujet,
                Message = dto.Message,
                MembreId = membreId,
                EnvoyeLe = DateTime.UtcNow
            };

            this.db.MessagesContact.Add(msg);
            await this.db.SaveChangesAsync();

            if (msg.MembreId.HasValue)
                await this.db.Entry(msg)
                    .Reference(m => m.Membre).LoadAsync();

            return MapToResponse(msg);
        }

        // ── 2. LISTE (Admin) ──────────────────────────────────
        public async Task<List<MessageResponseDto>> GetTous(bool? nonLusSeulement = null)
        {
            var query = this.db.MessagesContact
                .Include(m => m.Membre)
                .AsQueryable();

            if (nonLusSeulement == true)
                query = query.Where(m => !m.Lu);

            return await query
                .OrderByDescending(m => m.EnvoyeLe)
                .Select(m => MapToResponse(m))
                .ToListAsync();
        }

        // ── 3. GET PAR ID (Admin) ─────────────────────────────
        public async Task<MessageResponseDto> GetById(int id)
        {
            var msg = await this.db.MessagesContact
                .Include(m => m.Membre)
                .FirstOrDefaultAsync(m => m.Id == id)
                ?? throw new KeyNotFoundException($"Message #{id} introuvable.");

            return MapToResponse(msg);
        }

        // ── 4. MARQUER LU (Admin) ─────────────────────────────
        public async Task<MessageResponseDto> MarquerLu(int id)
        {
            var msg = await this.db.MessagesContact.FindAsync(id)
                ?? throw new KeyNotFoundException($"Message #{id} introuvable.");

            msg.Lu = true;
            await this.db.SaveChangesAsync();
            await this.db.Entry(msg).Reference(m => m.Membre).LoadAsync();
            return MapToResponse(msg);
        }

        // ── 5. SUPPRIMER (Admin) ──────────────────────────────
        public async Task Supprimer(int id)
        {
            var msg = await this.db.MessagesContact.FindAsync(id)
                ?? throw new KeyNotFoundException($"Message #{id} introuvable.");

            this.db.MessagesContact.Remove(msg);
            await this.db.SaveChangesAsync();
        }

        // ── 6. STATS (Admin) ──────────────────────────────────
        public async Task<object> GetStats()
        {
            var total = await this.db.MessagesContact.CountAsync();
            var nonLus = await this.db.MessagesContact.CountAsync(m => !m.Lu);
            return new { total, nonLus, lus = total - nonLus };
        }

        // ── HELPER ────────────────────────────────────────────
        private static MessageResponseDto MapToResponse(Models.MessageContact m) => new()
        {
            Id = m.Id,
            NomPrenom = m.NomPrenom,
            Email = m.Email,
            Sujet = m.Sujet,
            Message = m.Message,
            Lu = m.Lu,
            EnvoyeLe = m.EnvoyeLe,
            MembreNom = m.Membre is null
                ? null
                : $"{m.Membre.Prenom} {m.Membre.Nom}"
        };
    }
}