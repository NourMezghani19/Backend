using backend.DTOs.Coach;
using backend.Services.Coach;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Coach
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoachController : ControllerBase
    {
        private readonly CoachService svc;
        private readonly ILogger<CoachController> logger;

        public CoachController(CoachService svc, ILogger<CoachController> logger)
        {
            this.svc = svc;
            this.logger = logger;
        }

        // GET /api/Coach
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await this.svc.GetAll();
            return Ok(result);
        }

        // GET /api/Coach/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await this.svc.GetById(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST /api/Coach
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCoachDto dto)
        {
            try
            {
                var result = await this.svc.Create(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                // 409 = Email ou Téléphone déjà utilisé
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Erreur création coach");
                return StatusCode(500, new { message = "Erreur interne du serveur" });
            }
        }

        // PUT /api/Coach/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCoachDto dto)
        {
            try
            {
                var result = await this.svc.Update(id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Erreur mise à jour coach {Id}", id);
                return StatusCode(500, new { message = "Erreur interne du serveur" });
            }
        }

        // GET /api/Coach/search?q=ali
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q = "")
        {
            var result = await this.svc.Search(q);
            return Ok(result);
        }

        // DELETE /api/Coach/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await this.svc.Delete(id);
                if (!deleted)
                    return NotFound(new { message = "Coach introuvable" });

                return NoContent();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Erreur suppression coach {Id}", id);
                return StatusCode(500, new { message = "Erreur interne du serveur" });
            }
        }
        // PATCH /api/Coach/5/disponibilite
        [HttpPatch("{id:int}/disponibilite")]
        public async Task<IActionResult> ToggleDisponibilite(int id)
        {
            try
            {
                var result = await this.svc.ToggleDisponibilite(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Erreur toggle disponibilité coach {Id}", id);
                return StatusCode(500, new { message = "Erreur interne du serveur" });
            }
        }
    }
}