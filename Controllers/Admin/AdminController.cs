using backend.DTOs.Admin;
using backend.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Admin
{
    [ApiController]
    [Route("api/Administrateur")]
    [Authorize(Roles = "Administrateur,SuperAdministrateur")]
    public class AdminController : ControllerBase
    {
        private readonly AdminService svc;

        public AdminController(AdminService svc) => this.svc = svc;

        [HttpGet("verifier-id/{idSalle}")]
        public IActionResult VerifierIdSalle(string idSalle)
        {
            var result = svc.VerifierIdSalle(idSalle);
            return result.Valide ? Ok(result) : BadRequest(result);
        }

        [HttpGet("ids-statut")]
        public IActionResult GetStatutIds() => Ok(svc.GetStatutIds());

        [HttpGet("membres")]
        public async Task<IActionResult> GetMembres([FromQuery] string? search = null)
        {
            var membres = await svc.GetAllMembres(search);
            return Ok(new
            {
                count = membres.Count,
                search = search ?? "(aucun filtre)",
                membres
            });
        }

        [HttpPost("membres")]
        public async Task<IActionResult> CreerMembre([FromBody] CreateMembreDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await svc.CreerCompteMembre(dto);
                return Created(
                    $"/api/Administrateur/membres/{result.Id}",
                    new
                    {
                        message = $"Compte '{result.Prenom} {result.Nom}' créé. Email envoyé ",
                        success = true,
                        membre = result
                    });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message, success = false });
            }
        }

        [HttpDelete("membres/{id}")]
        public async Task<IActionResult> SupprimerMembre(int id)
        {
            var ok = await svc.SupprimerMembre(id);
            return ok
                ? Ok(new { message = "Membre supprimé", success = true })
                : NotFound(new { message = "Membre introuvable", success = false });
        }
    }
}