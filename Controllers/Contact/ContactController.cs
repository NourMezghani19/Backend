// Controllers/Contact/ContactController.cs
using backend.DTOs.Contact;
using backend.Services.Contact;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers.Contact
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly ContactService svc;

        public ContactController(ContactService svc)
        {
            this.svc = svc;
        }

        // POST /api/Contact
        // Membre connecté envoie un message
        [HttpPost]
        [Authorize(Roles = "Membre")]
        public async Task<IActionResult> Envoyer([FromBody] EnvoyerMessageDto dto)
        {
            var membreIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? membreId = int.TryParse(membreIdStr, out var id) ? id : null;

            try
            {
                var result = await this.svc.Envoyer(dto, membreId);
                return CreatedAtAction(nameof(GetById),
                    new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET /api/Contact
        // GET /api/Contact?nonLusSeulement=true
        [HttpGet]
        [Authorize(Roles = "Administrateur")]
        public async Task<IActionResult> GetTous(
            [FromQuery] bool? nonLusSeulement = null)
        {
            var result = await this.svc.GetTous(nonLusSeulement);
            return Ok(result);
        }

        // GET /api/Contact/{id}
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Administrateur")]
        public async Task<IActionResult> GetById(int id)
        {
            try { return Ok(await this.svc.GetById(id)); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        // PATCH /api/Contact/{id}/lu
        [HttpPatch("{id:int}/lu")]
        [Authorize(Roles = "Administrateur")]
        public async Task<IActionResult> MarquerLu(int id)
        {
            try { return Ok(await this.svc.MarquerLu(id)); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        // DELETE /api/Contact/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrateur")]
        public async Task<IActionResult> Supprimer(int id)
        {
            try { await this.svc.Supprimer(id); return NoContent(); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        // GET /api/Contact/stats
        [HttpGet("stats")]
        [Authorize(Roles = "Administrateur")]
        public async Task<IActionResult> Stats()
        {
            return Ok(await this.svc.GetStats());
        }
    }
}