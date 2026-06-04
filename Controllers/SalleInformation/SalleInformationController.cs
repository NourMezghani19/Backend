using backend.DTOs.SalleInformation;
using backend.Services.SalleInformation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.SalleInformation
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalleInformationController : ControllerBase
    {
        private readonly SalleInformationService service;

        public SalleInformationController(SalleInformationService service)
        {
            this.service = service;
        }

        // ───────────── GET : récupérer info salle ─────────────
        [HttpGet]
        public async Task<IActionResult> GetSalle()
        {
            var result = await service.GetInfoSalle();

            if (result == null)
                return NotFound(new { message = "Salle non trouvée" });

            return Ok(result);
        }

        // ───────────── POST : initialiser salle ─────────────
        [HttpPost("init")]
        [Authorize(Roles = "SuperAdministrateur")]
        public async Task<IActionResult> InitSalle()
        {
            try
            {
                var result = await service.InitSalle();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ───────────── PUT : update info globale ─────────────
        [HttpPatch("info")]
        [Authorize(Roles = "SuperAdministrateur")]
        public async Task<IActionResult> UpdateInfoGlobale([FromBody] UpdateInfoGlobaleRequestDto req)
        {
            var result = await service.UpdateInfoGlobale(req);
            if (result == null)
                return NotFound(new { message = "Salle non trouvée" });
            return Ok(result);
        }

        // ───────────── PUT : update horaire d'un jour ─────────────
        [HttpPatch("horaire/{jour}")]
        [Authorize(Roles = "SuperAdministrateur")]
        public async Task<IActionResult> UpdateHoraireJour(string jour, [FromBody] UpdateHoraireJourRequestDto req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await service.UpdateHoraireJour(jour, req);
            if (result == null)
                return NotFound(new { message = "Salle ou jour introuvable" });
            return Ok(result);
        }

        // ───────────── DELETE : supprimer salle ─────────────
        [HttpDelete]
        public async Task<IActionResult> DeleteSalle()
        {
            var deleted = await service.DeleteSalle();

            if (!deleted)
                return NotFound(new { message = "Salle non trouvée" });

            return Ok(new { message = "Salle supprimée avec succès" });
        }

        // ───────────── GET : statut actuel ─────────────
        [HttpGet("statut")]
        public async Task<IActionResult> GetStatut()
        {
            var result = await service.GetStatutSalle();
            return Ok(result);
        }

        // ───────────── POST : valider créneau ─────────────
        [HttpPost("valider-creneau")]
        public async Task<IActionResult> ValiderCreneau([FromBody] ValiderCreneauRequestDto req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await service.ValiderCreneau(req);

            return Ok(result);
        }
    }
}

