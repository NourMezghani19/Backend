using backend.DTOs.Avatar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public AvatarController(
            IHttpClientFactory httpClientFactory,
            ILogger<AvatarController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpPost("predict")]
        public async Task<IActionResult> PredictAvatar([FromBody] AvatarRequestDto request)
        {
            try
            {

                var genreStr = User.Claims
            .FirstOrDefault(c => c.Type == "genre")?.Value ?? "Homme";

                // Convertir en int pour FastAPI (0=Femme, 1=Homme)
                int genre = genreStr.ToLower() == "femme" ? 0 : 1;

                var client = _httpClientFactory.CreateClient("FastAPI");

                var fastApiPayload = new
                {
                    genre = genre,           // ← depuis JWT, pas depuis request
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

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var avatarResponse = JsonSerializer.Deserialize<AvatarResponseDto>(
                    responseJson, options);

                return Ok(avatarResponse);
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