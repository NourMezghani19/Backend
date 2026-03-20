using backend.Data;
using backend.DTOs.Reservation;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.ReservationService
{
    public class ReservationService
    {
        private readonly AppDbContext db;
        private readonly NotificationService notif;

        public ReservationService(AppDbContext db, NotificationService notif)
        {
            this.db = db;
            this.notif = notif;
        }

        // ── RÉSERVER ─────────────────────────
        public async Task<(bool Success, string Message, ReservationResponseDto? Data)> Effectuer(CreateReservationDto dto)
        {
            var session = await db.Sessions
                .Include(s => s.Cours)
                .FirstOrDefaultAsync(s => s.Id == dto.SessionId);

            if (session == null)
                return (false, "Session introuvable", null);

            if (session.PlacesDisponibles <= 0)
                return (false, "La session est complète", null);

            var dejaInscrit = await db.Reservations.AnyAsync(r =>
                r.MembreId == dto.MembreId &&
                r.SessionCoursId == dto.SessionId &&
                r.Statut == StatutReservation.Confirmee);

            if (dejaInscrit)
                return (false, "Vous êtes déjà inscrit à cette session", null);

            session.PlacesDisponibles--;

            var resa = new Reservation
            {
                MembreId = dto.MembreId,
                SessionCoursId = dto.SessionId
            };

            db.Reservations.Add(resa);
            await db.SaveChangesAsync();

            await notif.EnvoyerConfirmation(resa); // ✅ await au lieu de fire-and-forget

            var responseDto = new ReservationResponseDto
            {
                Id = resa.Id,
                MembreId = resa.MembreId,
                SessionId = resa.SessionCoursId,
                NomCours = session.Cours!.Nom,
                DateReservation = resa.DateReservation,
                Statut = resa.Statut.ToString(),
                PlacesRestantes = session.PlacesDisponibles
            };

            return (true, "Réservation effectuée avec succès", responseDto);
        }

        // ── ANNULER ─────────────────────────
        public async Task<(bool Success, string Message)> Annuler(int id, int membreId)
        {
            var resa = await db.Reservations
                .Include(r => r.SessionCours)
                .FirstOrDefaultAsync(r => r.Id == id && r.MembreId == membreId);

            if (resa == null)
                return (false, "Réservation introuvable");

            resa.Statut = StatutReservation.Annulee;
            resa.SessionCours!.PlacesDisponibles++;

            await db.SaveChangesAsync();

            await notif.EnvoyerAnnulation(resa); // ✅ await au lieu de fire-and-forget

            return (true, "Réservation annulée avec succès");
        }

        // ── MES RÉSERVATIONS ────────────────
        public async Task<List<ReservationResponseDto>> GetMesReservations(int membreId)
        {
            return await db.Reservations
                .Include(r => r.SessionCours)
                .ThenInclude(s => s!.Cours)
                .Where(r => r.MembreId == membreId)
                .OrderByDescending(r => r.DateReservation)
                .Select(r => new ReservationResponseDto
                {
                    Id = r.Id,
                    MembreId = r.MembreId,
                    SessionId = r.SessionCoursId,
                    NomCours = r.SessionCours!.Cours!.Nom,
                    DateReservation = r.DateReservation,
                    Statut = r.Statut.ToString(),
                    PlacesRestantes = r.SessionCours.PlacesDisponibles
                })
                .ToListAsync();
        }
    }
}
