using backend.DTOs.Motivation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MotivationController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<MotivationController> _logger;

        public MotivationController(
            IHttpClientFactory httpClientFactory,
            ILogger<MotivationController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] MotivationRequestDto request)
        {
            try
            {
                // ── Nom depuis JWT ──────────────────────────────
                var memberName = User.Claims
                    .FirstOrDefault(c => c.Type == "nom")?.Value
                    ?? User.Claims
                    .FirstOrDefault(c => c.Type == "prenom")?.Value
                    ?? "Membre";

                // ── Construire les messages pour FastAPI ────────
                var messages = new List<Dictionary<string, string>>();

                // Ajouter historique — filtrer les messages vides ou role invalide
                if (request.History != null && request.History.Any())
                {
                    foreach (var msg in request.History)
                    {
                        // role doit être exactement "user" ou "assistant"
                        if ((msg.Role == "user" || msg.Role == "assistant")
                            && !string.IsNullOrWhiteSpace(msg.Content))
                        {
                            messages.Add(new Dictionary<string, string>
                {
                    { "role",    msg.Role    },
                    { "content", msg.Content }
                });
                        }
                    }
                }

                // Nouveau message — role forcé "user"
                messages.Add(new Dictionary<string, string>
                {
                    { "role",    "user"           },
                    { "content", request.Content  }
                });

                var fastApiPayload = new Dictionary<string, object>
                {
                    { "member_name", memberName },
                    { "messages",    messages   }
                };

                var json = JsonSerializer.Serialize(fastApiPayload);
                _logger.LogInformation("Payload FastAPI: {Json}", json);

                var client = _httpClientFactory.CreateClient("FastAPI");
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/motivation/chat", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("FastAPI error: {Error}", error);
                    return StatusCode(500, error);
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<MotivationResponseDto>(
                    responseJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                return Ok(result);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("FastAPI non accessible: {Message}", ex.Message);
                return StatusCode(503, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Erreur: {Message}", ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
    }
}