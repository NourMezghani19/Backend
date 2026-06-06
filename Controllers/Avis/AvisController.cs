using backend.DTOs.Avis;
using backend.Services.Avis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AvisController : ControllerBase
    {
        private readonly AvisService avisService;

        public AvisController(AvisService avisService)
        {
            this.avisService = avisService;
        }

        // =====================================================
        // MEMBRE CONNECTÉ
        // =====================================================

        [Authorize]
        [HttpPost("membre")]
        public async Task<IActionResult> CreerAvisMembre(
            [FromBody] CreerAvisDto dto)
        {
            try
            {
                var membreId = int.Parse(
                    User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var result = await avisService.CreerMembre(
                    membreId, dto);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("mon-avis")]
        [Authorize]
        [HttpGet("mes-avis")]
        public async Task<IActionResult> GetMesAvis()
        {
            var membreId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await avisService.GetMesAvis(membreId);

            return Ok(result);
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Modifier(
            int id,
            [FromBody] CreerAvisDto dto)
        {
            try
            {
                var membreId = int.Parse(
                    User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var result = await avisService.Modifier(
                    id, membreId, dto);

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Supprimer(int id)
        {
            try
            {
                var membreId = int.Parse(
                    User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                await avisService.Supprimer(id, membreId);

                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // =====================================================
        // VISITEUR ANONYME
        // =====================================================

        [AllowAnonymous]
        [HttpPost("anonyme")]
        public async Task<IActionResult> CreerAvisAnonyme(
            [FromBody] CreerAvisDto dto)
        {
            try
            {
                var result = await avisService.CreerAnonyme(dto);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // =====================================================
        // PUBLIC
        // =====================================================

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAvisVisibles()
        {
            var result = await avisService.GetVisibles();

            return Ok(result);
        }

        // =====================================================
        // ADMIN
        // =====================================================

        [Authorize(Roles = "Administrateur")]
        [HttpGet("admin")]
        public async Task<IActionResult> GetTous()
        {
            var result = await avisService.GetTous();

            return Ok(result);
        }

        [Authorize(Roles = "Administrateur")]
        [HttpPatch("{id}/toggle-blocage")]
        public async Task<IActionResult> ToggleBlocage(int id)
        {
            try
            {
                var result = await avisService.ToggleBloque(id);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [Authorize(Roles = "Administrateur")]
        [HttpDelete("admin/{id}")]
        public async Task<IActionResult> SupprimerAdmin(int id)
        {
            try
            {
                await avisService.SupprimerAdmin(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [Authorize(Roles = "Administrateur")]
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await avisService.GetStats();

            return Ok(stats);
        }
    }
}