using backend.DTOs.Abonnement;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AbonnementController : ControllerBase
    {
        private readonly AbonnementService _service;
        public AbonnementController(AbonnementService service) => _service = service;

        // POST /api/abonnement/affecter  → Admin
        [HttpPost("affecter")]
        [Authorize(Roles = "Administrateur,SuperAdministrateur")]
        public async Task<IActionResult> Affecter([FromBody] AffecterAbonnementDto dto)
        {
            try { return Ok(await _service.AffecterAbonnement(dto)); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        // GET /api/abonnement?membreId=  → Admin
        [HttpGet]
        [Authorize(Roles = "Administrateur,SuperAdministrateur")]
        public async Task<IActionResult> GetTous([FromQuery] int? membreId)
            => Ok(await _service.GetTous(membreId));

        // GET /api/abonnement/mon-abonnement  → Membre connecté
        [HttpGet("mon-abonnement")]
        public async Task<IActionResult> MonAbonnement()
        {
            var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ab = await _service.GetAbonnementActif(id);
            return ab == null ? NoContent() : Ok(ab);
        }
    }
}