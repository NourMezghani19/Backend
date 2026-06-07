using backend.DTOs.Avatar;
using backend.Services.Avatar.Interfaces;
using backend.Services.Historique;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AvatarController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AvatarController> _logger;
        private readonly IAvatarService _avatarService;
        private readonly HistoriqueService _historiqueService;

        public AvatarController(
            IHttpClientFactory httpClientFactory,
            ILogger<AvatarController> logger,
            IAvatarService avatarService,
            HistoriqueService historiqueService)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _avatarService = avatarService;
            _historiqueService = historiqueService;
        }

        /// <summary>
        /// Génère les avatars SVG via FastAPI + retourne la comparaison C# locale.
        /// </summary>
        [HttpPost("predict")]
        public async Task<IActionResult> PredictAvatar([FromBody] AvatarRequestDto request)
        {
            try
            {

                var genreStr = User.Claims
                    .FirstOrDefault(c => c.Type == "genre")?.Value ?? "Homme";

                // Convertir en int pour FastAPI (0=Femme, 1=Homme)
                int genre = genreStr.ToLower() == "femme" ? 0 : 1;

                // ── 1. Appel FastAPI ────────────────────────────────────
                var client = _httpClientFactory.CreateClient("FastAPI");

                var fastApiPayload = new
                {
                    genre = genre,
                    taille = request.Taille,
                    poids = request.Poids,
                    objectif_poids = request.ObjectifPoids
                };

                var json = JsonSerializer.Serialize(fastApiPayload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/avatar/predict", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("FastAPI error: {Error}", error);
                    return StatusCode(500, "Erreur du service IA");
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var avatarResponse = JsonSerializer.Deserialize<AvatarResponseDto>(
                    responseJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // ── 2. Comparaison locale (C#) ─────────────────────────
                var comparaison = _avatarService.Comparer(
                    taille: request.Taille,
                    poidsActuel: request.Poids,
                    poidsObjectif: request.ObjectifPoids);

                // ── 3. Enregistrer dans l'historique ───────────────────
                var membreIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(membreIdStr, out int membreId))
                {
                    await _historiqueService.EnregistrerAvatarGenere(
                        membreId,
                        request.Poids,
                        request.ObjectifPoids);
                }

                // ── 4. Réponse enrichie ────────────────────────────────
                return Ok(new
                {
                    avatar = avatarResponse,
                    comparaison
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("FastAPI non accessible: {Message}", ex.Message);
                return StatusCode(503, "Service IA non disponible");
            }
            catch (Exception ex)
            {
                _logger.LogError("Erreur avatar: {Message}", ex.Message);
                return StatusCode(500, "Erreur interne");
            }
        }

        /// <summary>
        /// Comparaison rapide sans appel FastAPI (local uniquement).
        /// </summary>
        [HttpPost("comparer")]
        public IActionResult Comparer([FromBody] AvatarRequestDto request)
        {
            var comparaison = _avatarService.Comparer(
                request.Taille,
                request.Poids,
                request.ObjectifPoids);

            return Ok(comparaison);
        }

        /// <summary>
        /// Health check FastAPI.
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("FastAPI");
                var response = await client.GetAsync("/health");
                return Ok(new { fastapi = response.IsSuccessStatusCode ? "ok" : "error" });
            }
            catch
            {
                return Ok(new { fastapi = "unreachable" });
            }
        }
    }
}