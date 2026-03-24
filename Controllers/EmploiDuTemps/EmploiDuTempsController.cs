using backend.Services.EmploiDuTemps;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs.EmploiDuTemps;


namespace backend.Controllers.EmploiDuTemps
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmploiDuTempsController : ControllerBase
    {
        private readonly EmploiDuTempsService svc;

        public EmploiDuTempsController(EmploiDuTempsService svc)
        {
            this.svc = svc;
        }

        // POST /api/EmploiDuTemps
        // Body : { coachId, jour:1, heureDebut:"08:00", heureFin:"14:00", note? }
        // jour : 0=Dim 1=Lun 2=Mar 3=Mer 4=Jeu 5=Ven 6=Sam
        [HttpPost]
        public async Task<IActionResult> Creer([FromBody] CreerCreneauDto dto)
        {
            try
            {
                var result = await this.svc.Creer(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (InvalidOperationException ex) { return Conflict(ex.Message); }
        }

        // GET /api/EmploiDuTemps/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try { return Ok(await this.svc.GetById(id)); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        // GET /api/EmploiDuTemps/coach/{coachId}
        [HttpGet("coach/{coachId:int}")]
        public async Task<IActionResult> GetByCoach(int coachId)
        {
            var result = await this.svc.GetByCoach(coachId);
            return Ok(result);
        }

        // GET /api/EmploiDuTemps/calendrier?coachId=3
        [HttpGet("calendrier")]
        public async Task<IActionResult> GetCalendrier([FromQuery] int? coachId = null)
        {
            var result = await this.svc.GetCalendrier(coachId);
            return Ok(result);
        }

        // DELETE /api/EmploiDuTemps/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Supprimer(int id)
        {
            try { await this.svc.Supprimer(id); return NoContent(); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        // GET /api/EmploiDuTemps/disponibles-aujourdhui
        [HttpGet("disponibles-aujourdhui")]
        [Authorize]
        public async Task<IActionResult> DisponiblesAujourdhui()
        {
            var result = await this.svc.GetDisponiblesAujourdhui();
            return Ok(result);
        }

        // GET /api/EmploiDuTemps/stats
        [HttpGet("stats")]
        public async Task<IActionResult> Stats()
        {
            var result = await this.svc.GetStats();
            return Ok(result);
        }
    }
}