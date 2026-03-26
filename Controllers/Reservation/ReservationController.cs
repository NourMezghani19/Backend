using backend.DTOs.Reservation;
using backend.Services.ReservationService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers.Reservation
{
    [ApiController, Route("api/[controller]")]
    [Authorize(Roles = "Membre")]
    public class ReservationController(ReservationService svc): ControllerBase
    {
        [HttpPost]   // effectuer() — Réserver
        public async Task<IActionResult> Effectuer(CreateReservationDto dto)
        {
            var (success, message, data) = await svc.Effectuer(dto);
            if (!success)
                return BadRequest(new { message });
            return Ok(new { message, data });
        }


        [HttpDelete("{id:int}")]   // annuler()
        public async Task<IActionResult> Annuler(int id)
        {
            var membreId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var (success, message) = await svc.Annuler(id, membreId);
            if (!success)
                return NotFound(new { message });
            return Ok(new { message });
        }

        [HttpGet("mes-reservations")]   // voir mes réservations
        public async Task<IActionResult> MesReservations()
        {
            var membreId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var reservations = await svc.GetMesReservations(membreId);
            return Ok(reservations);
        }
    }
}
