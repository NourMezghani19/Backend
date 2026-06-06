// Controllers/PlanAbonnement/PlanAbonnementController.cs
using backend.DTOs.PlanAbonnement;
using backend.Services.PlanAbonnement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.PlanAbonnement
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanAbonnementController : ControllerBase
    {
        private readonly PlanAbonnementService svc;

        public PlanAbonnementController(PlanAbonnementService svc)
        {
            this.svc = svc;
        }

        // POST /api/PlanAbonnement
        [HttpPost]
        [Authorize(Roles = "SuperAdministrateur")]
        public async Task<IActionResult> Creer([FromBody] CreerPlanDto dto)
        {
            try
            {
                var result = await this.svc.Creer(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        }

        // GET /api/PlanAbonnement
        // GET /api/PlanAbonnement?actifSeulement=true
        [HttpGet]
        public async Task<IActionResult> GetTous([FromQuery] bool? actifSeulement = null)
        {
            var result = await this.svc.GetTous(actifSeulement);
            return Ok(result);
        }

        // GET /api/PlanAbonnement/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try { return Ok(await this.svc.GetById(id)); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        // PUT /api/PlanAbonnement/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "SuperAdministrateur")]
        public async Task<IActionResult> Modifier(int id, [FromBody] CreerPlanDto dto)
        {
            try
            {
                var result = await this.svc.Modifier(id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        }

        // PATCH /api/PlanAbonnement/{id}/toggle
        [HttpPatch("{id:int}/toggle")]
        [Authorize(Roles = "SuperAdministrateur")]
        public async Task<IActionResult> ToggleActif(int id)
        {
            try { return Ok(await this.svc.ToggleActif(id)); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        // DELETE /api/PlanAbonnement/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "SuperAdministrateur")]
        public async Task<IActionResult> Supprimer(int id)
        {
            try { await this.svc.Supprimer(id); return NoContent(); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }
    }
}