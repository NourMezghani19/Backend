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

        // --- SECTION ADMIN : RÉCUPÉRER TOUTES LES RÉSERVATIONS ---
        public async Task<List<ReservationResponseDto>> GetAllReservationsWithDetails()
        {
            return await db.Reservations
                .Include(r => r.Membre)
                .Include(r => r.SessionCours)
                    .ThenInclude(s => s!.Cours)
                .OrderByDescending(r => r.DateReservation)
                .Select(r => new ReservationResponseDto
                {
                    Id = r.Id,
                    MembreId = r.MembreId,
                    // Assurez-vous que ces champs existent dans votre ReservationResponseDto
                    // Sinon, ajoutez-les ou utilisez des propriétés dynamiques
                    MembreNom = r.Membre!.Nom,
                    MembrePrenom = r.Membre!.Prenom,
                    SessionId = r.SessionCoursId,
                    NomCours = r.SessionCours!.Cours!.Nom,
                    // ✅ AJOUTE CES DEUX LIGNES ICI :
                    // Dans GetAllReservationsWithDetails (lignes 38-39 sur ton image)
                    HeureDebut = r.SessionCours.HeureDebut.ToString(@"hh\:mm"),
                    HeureFin = r.SessionCours.HeureFin.ToString(@"hh\:mm"),
                    Statut = r.Statut.ToString(),
                    PlacesRestantes = r.SessionCours.PlacesDisponibles
                })
                .ToListAsync();
        }

        // --- SECTION ADMIN : CONFIRMER OU ANNULER ---
        // --- SECTION ADMIN : CONFIRMER OU ANNULER ---
        public async Task<(bool Success, string Message)> ChangerStatut(int id, string nouveauStatut)
        {
            var resa = await db.Reservations
                .Include(r => r.Membre)
                .Include(r => r.SessionCours)
                    .ThenInclude(s => s!.Cours)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (resa == null) return (false, "Réservation introuvable");

            // 🚩 Correction de la comparaison : on gère "Rejete" et "Rejetee"
            if (nouveauStatut == "Confirmee")
            {
                resa.Statut = StatutReservation.Confirmee;
                await notif.EnvoyerConfirmation(resa);
            }
            else if (nouveauStatut == "Rejete" || nouveauStatut == "Rejetee" )
            {
                // On rend la place SEULEMENT si elle n'était pas déjà annulée
                if (resa.Statut != StatutReservation.Annulee)
                {
                    if (resa.SessionCours != null) resa.SessionCours.PlacesDisponibles++;
                }

                // On force le statut à Annulee (ou Rejete si ton Enum le possède)
                resa.Statut = StatutReservation.Annulee;
                await notif.EnvoyerAnnulation(resa);
            }

            // ✅ On sauvegarde TOUJOURS les changements
            await db.SaveChangesAsync();
            return (true, $"La demande a été traitée ({nouveauStatut})");
        }

        // --- RÉSERVER (EXISTANT) ---
        // --- RÉSERVER (CÔTÉ MEMBRE) ---
        public async Task<(bool Success, string Message, ReservationResponseDto? Data)> Effectuer(CreateReservationDto dto)
        {
            var session = await db.Sessions
                .Include(s => s.Cours)
                .FirstOrDefaultAsync(s => s.Id == dto.SessionId);

            if (session == null) return (false, "Session introuvable", null);
            if (session.PlacesDisponibles <= 0) return (false, "La session est complète", null);

            var resa = new Reservation
            {
                MembreId = dto.MembreId,
                SessionCoursId = dto.SessionId,
                DateReservation = DateTime.Now, // Assurez-vous d'assigner la date actuelle
                Statut = StatutReservation.EnAttente // État initial correct
            };

            // On décrémente la place pour la bloquer en attendant la validation
            session.PlacesDisponibles--;

            db.Reservations.Add(resa);
            await db.SaveChangesAsync();

            // ❌ SUPPRIMÉ : await notif.EnvoyerConfirmation(resa); 
            // La notification sera envoyée plus tard par l'admin via ChangerStatut

            var responseDto = new ReservationResponseDto
            {
                Id = resa.Id,
                MembreId = resa.MembreId,
                SessionId = resa.SessionCoursId,
                NomCours = session.Cours!.Nom,
                HeureDebut = session.HeureDebut.ToString(@"hh\:mm"),
                HeureFin = session.HeureFin.ToString(@"hh\:mm"),

                Statut = resa.Statut.ToString(), // Sera "EnAttente"
                PlacesRestantes = session.PlacesDisponibles
            };

            return (true, "Demande de réservation envoyée. En attente de validation par l'admin.", responseDto);
        }


        // --- ANNULER (MEMBRE) ---
        public async Task<(bool Success, string Message)> Annuler(int id, int membreId)
        {
            var resa = await db.Reservations
                .Include(r => r.SessionCours)
                .FirstOrDefaultAsync(r => r.Id == id && r.MembreId == membreId);

            if (resa == null) return (false, "Réservation introuvable");

            // On ne rend une place que si la réservation était active (Confirmée ou En Attente)
            if (resa.Statut != StatutReservation.Annulee)
            {
                resa.SessionCours!.PlacesDisponibles++;
            }

            // On supprime la ligne de la base de données
            db.Reservations.Remove(resa);

            await db.SaveChangesAsync();

            // ❌ SUPPRIMÉ : await notif.EnvoyerAnnulation(resa); 
            // On ne notifie pas le membre pour une action qu'il a faite lui-même.

            return (true, "Réservation annulée avec succès");
        }

        // --- MES RÉSERVATIONS (MEMBRE) ---
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
                    HeureDebut = r.SessionCours!.HeureDebut.ToString(@"hh\:mm"),
                    HeureFin = r.SessionCours!.HeureFin.ToString(@"hh\:mm"),
                    Statut = r.Statut.ToString(),
                    PlacesRestantes = r.SessionCours.PlacesDisponibles
                })
                .ToListAsync();
        }
    }
}