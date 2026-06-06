using backend.Services.Historique;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HistoriqueController : ControllerBase
    {
        private readonly HistoriqueService _historiqueService;

        public HistoriqueController(HistoriqueService historiqueService)
        {
            _historiqueService = historiqueService;
        }

        /// <summary>
        /// Retourne l'historique complet du membre connecté.
        /// GET /api/historique
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMonHistorique()
        {
            var membreIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(membreIdStr, out int membreId))
                return Unauthorized();

            var result = await _historiqueService.GetHistorique(membreId);
            return Ok(result);
        }

        /// <summary>
        /// Retourne l'historique d'un membre spécifique (Admin uniquement).
        /// GET /api/historique/{membreId}
        /// </summary>
        [HttpGet("{membreId:int}")]
        [Authorize(Roles = "Admin,SuperAdministrateur")]
        public async Task<IActionResult> GetHistoriqueMembre(int membreId)
        {
            var result = await _historiqueService.GetHistorique(membreId);
            return Ok(result);
        }
    }
}