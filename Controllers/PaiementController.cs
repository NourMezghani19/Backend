using backend.DTOs.Paiement;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaiementController : ControllerBase
    {
        private readonly PaiementService _service;
        public PaiementController(PaiementService service) => _service = service;

        // GET /api/paiement  → Admin/SuperAdmin
        [HttpGet]
        [Authorize(Roles = "Administrateur,SuperAdministrateur")]
        public async Task<IActionResult> GetTous([FromQuery] FiltragePaiementDto filtre)
            => Ok(await _service.GetTous(filtre));

        // GET /api/paiement/stats  → Admin/SuperAdmin
        [HttpGet("stats")]
        [Authorize(Roles = "Administrateur,SuperAdministrateur")]
        public async Task<IActionResult> Stats(
            [FromQuery] DateTime? debut, [FromQuery] DateTime? fin,
            [FromQuery] int? annee, [FromQuery] int? mois)
            => Ok(await _service.GetStats(debut, fin, annee, mois));

        // GET /api/paiement/{id}  → Admin/SuperAdmin
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrateur,SuperAdministrateur")]
        public async Task<IActionResult> GetById(int id)
        {
            try { return Ok(await _service.GetById(id)); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        // PUT /api/paiement/{id}/valider  → Admin/SuperAdmin
        [HttpPut("{id}/valider")]
        [Authorize(Roles = "Administrateur,SuperAdministrateur")]
        public async Task<IActionResult> Valider(int id)
        {
            try { return Ok(await _service.Valider(id)); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        // GET /api/paiement/mes-paiements?annee=  → Membre connecté
        [HttpGet("mes-paiements")]
        public async Task<IActionResult> MesPaiements([FromQuery] int? annee)
        {
            var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await _service.GetMesPaiements(id, annee));
        }
    }
}