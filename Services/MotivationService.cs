// ============================================================
// FICHIER : Services/MotivationService.cs
// ============================================================
using backend.Data;
using backend.DTOs.Motivation;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class MotivationService
    {
        private readonly AppDbContext _context;

        public MotivationService(AppDbContext context)
        {
            _context = context;
        }

        // ────────────────────────────────────────────────────────
        //  SESSIONS — liste des conversations du membre
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Retourne toutes les sessions du membre, triées par date DESC.
        /// Pas les messages — juste les métadonnées (id, titre, date).
        /// </summary>
        public async Task<List<SessionSummaryDto>> GetSessions(int membreId)
        {
            return await _context.ConversationSessions
                .Where(s => s.MembreId == membreId)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new SessionSummaryDto
                {
                    Id = s.Id,
                    Titre = s.Titre,
                    CreatedAt = s.CreatedAt,
                    NbMessages = s.Messages.Count,
                })
                .ToListAsync();
        }

        // ────────────────────────────────────────────────────────
        //  MESSAGES — historique d'une session
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Retourne tous les messages d'une session.
        /// Vérifie que la session appartient bien au membre (sécurité).
        /// </summary>
        public async Task<List<MessageDto>?> GetMessages(int sessionId, int membreId)
        {
            var session = await _context.ConversationSessions
                .Include(s => s.Messages.OrderBy(m => m.SentAt))
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.MembreId == membreId);

            if (session == null) return null;

            return session.Messages
                .Select(m => new MessageDto { Role = m.Role, Content = m.Content })
                .ToList();
        }

        // ────────────────────────────────────────────────────────
        //  CRÉER UNE SESSION
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Crée une nouvelle session vide pour le membre.
        /// Le titre sera mis à jour au 1er message.
        /// </summary>
        public async Task<ConversationSession> CreerSession(int membreId)
        {
            var session = new ConversationSession
            {
                MembreId = membreId,
                CreatedAt = DateTime.UtcNow,
                Titre = "Nouvelle conversation",
            };
            _context.ConversationSessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }

        // ────────────────────────────────────────────────────────
        //  SAUVEGARDER UN ÉCHANGE (user + assistant)
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Sauvegarde le message du membre ET la réponse du chatbot.
        /// Met à jour le titre de la session si c'est le 1er message.
        /// </summary>
        public async Task SauvegarderEchange(
            int sessionId,
            string userMessage,
            string assistantReply)
        {
            var session = await _context.ConversationSessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null) return;

            // Auto-titre depuis le 1er message utilisateur (max 60 chars)
            if (!session.Messages.Any())
            {
                session.Titre = userMessage.Length > 60
                    ? userMessage[..60] + "…"
                    : userMessage;
            }

            session.Messages.Add(new ConversationMessage
            {
                SessionId = sessionId,
                Role = "user",
                Content = userMessage,
                SentAt = DateTime.UtcNow,
            });

            session.Messages.Add(new ConversationMessage
            {
                SessionId = sessionId,
                Role = "assistant",
                Content = assistantReply,
                SentAt = DateTime.UtcNow,
            });

            await _context.SaveChangesAsync();
        }

        // ────────────────────────────────────────────────────────
        //  SUPPRIMER UNE SESSION
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Supprime une session et tous ses messages (Cascade).
        /// Retourne false si la session n'appartient pas au membre.
        /// </summary>
        public async Task<bool> SupprimerSession(int sessionId, int membreId)
        {
            var session = await _context.ConversationSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.MembreId == membreId);

            if (session == null) return false;

            _context.ConversationSessions.Remove(session);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}