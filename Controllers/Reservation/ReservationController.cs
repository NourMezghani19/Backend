using backend.DTOs.Reservation;
using backend.Services.ReservationService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers.Reservation
{
    [Route("api/reservation")] // 👈 Vérifie bien cette ligne
    [ApiController]
    public class ReservationController(ReservationService svc) : ControllerBase
    {
        // --- SECTION MEMBRE ---

        [HttpPost]
        [Authorize(Roles = "Membre")]
        /* public async Task<IActionResult> Effectuer(CreateReservationDto dto)
         {
             var (success, message, data) = await svc.Effectuer(dto);
             if (!success) return BadRequest(new { message });
             return Ok(new { message, data });
         }*/
        // version avec le controle des genres pour specialiser les messages d'erreur
        public async Task<IActionResult> Effectuer(CreateReservationDto dto)
        {
            var (success, message, data) = await svc.Effectuer(dto);

            if (!success)
            {
                // Erreur d'accès genre → 403
                if (message.Contains("réservé aux"))
                    return StatusCode(403, new { message });

                return BadRequest(new { message });
            }

            return Ok(new { message, data });
        }

        [HttpGet("mes-reservations")]
        [Authorize(Roles = "Membre")]
        public async Task<IActionResult> MesReservations()
        {
            await svc.TerminerSessionsPassees(); // On termine d'abord les sessions passées pour que le membre ait une vue à jour
            var membreId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var reservations = await svc.GetMesReservations(membreId);
            return Ok(reservations);
        }

        // --- SECTION ADMINISTRATION ---

        [HttpGet] // GET api/reservation
        [Authorize(Roles = "Administrateur,SuperAdministrateur")]
        public async Task<IActionResult> GetAll()
        {
            // Appelle une méthode dans ton service qui fait les Includes 
            // pour avoir le nom du membre et du cours
            var reservations = await svc.GetAllReservationsWithDetails();
            return Ok(reservations);
        }

        [HttpPut("{id:int}/confirmer")]
        [Authorize(Roles = "Administrateur,SuperAdministrateur")]
        public async Task<IActionResult> Confirmer(int id)
        {
            var (success, message) = await svc.ChangerStatut(id, "Confirmee");
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }
        [HttpPut("{id:int}/rejeter")]
        [Authorize(Roles = "Administrateur,SuperAdministrateur")]
        public async Task<IActionResult> RejeterAdmin(int id)
        {
            // On passe "Rejetee" pour différencier d'une annulation faite par le membre lui-même
            var (success, message) = await svc.ChangerStatut(id, "Rejetee");
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }

        // Méthode d'annulation pour le membre (Delete)
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Membre")]
        public async Task<IActionResult> AnnulerMembre(int id)
        {
            var membreId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var (success, message) = await svc.Annuler(id, membreId);
            if (!success) return NotFound(new { message });
            return Ok(new { message });
        }


    }
}