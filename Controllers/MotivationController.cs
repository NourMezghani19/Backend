// ============================================================
// FICHIER : Controllers/MotivationController.cs  (version complète)
// ============================================================
using backend.DTOs.Motivation;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Membre")]
    public class MotivationController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<MotivationController> _logger;
        private readonly MotivationService _motivationService;

        public MotivationController(
            IHttpClientFactory httpClientFactory,
            ILogger<MotivationController> logger,
            MotivationService motivationService)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _motivationService = motivationService;
        }

        // ────────────────────────────────────────────────────────
        //  GET /api/motivation/sessions
        //  Liste toutes les conversations du membre connecté
        // ────────────────────────────────────────────────────────
        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions()
        {
            var membreId = GetMembreId();
            var sessions = await _motivationService.GetSessions(membreId);
            return Ok(sessions);
        }

        // ────────────────────────────────────────────────────────
        //  GET /api/motivation/sessions/{sessionId}/messages
        //  Historique complet d'une session
        // ────────────────────────────────────────────────────────
        [HttpGet("sessions/{sessionId:int}/messages")]
        public async Task<IActionResult> GetMessages(int sessionId)
        {
            var membreId = GetMembreId();
            var messages = await _motivationService.GetMessages(sessionId, membreId);

            if (messages == null)
                return NotFound(new { message = "Session introuvable" });

            return Ok(messages);
        }

        // ────────────────────────────────────────────────────────
        //  POST /api/motivation/chat
        //  Envoie un message, reçoit la réponse FastAPI,
        //  sauvegarde l'échange en DB, retourne reply + sessionId
        // ────────────────────────────────────────────────────────
        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequestDto request)
        {
            var membreId = GetMembreId();

            // 1. Récupérer ou créer la session
            int sessionId;
            List<MessageDto> historique;

            if (request.SessionId.HasValue)
            {
                // Session existante → charger l'historique pour le contexte FastAPI
                var msgs = await _motivationService.GetMessages(request.SessionId.Value, membreId);
                if (msgs == null)
                    return NotFound(new { message = "Session introuvable" });

                sessionId = request.SessionId.Value;
                historique = msgs;
            }
            else
            {
                // Nouvelle session
                var session = await _motivationService.CreerSession(membreId);
                sessionId = session.Id;
                historique = new List<MessageDto>();
            }

            // 2. Appel FastAPI (logique identique à l'original)
            try
            {
                // ── Nom depuis JWT ──────────────────────────────
                var memberName = User.Claims
                    .FirstOrDefault(c => c.Type == "nom")?.Value
                    ?? User.Claims
                    .FirstOrDefault(c => c.Type == "prenom")?.Value
                    ?? "Membre";

                // Construire les messages : historique + nouveau message
                var messages = historique
                    .Where(m => (m.Role == "user" || m.Role == "assistant")
                                && !string.IsNullOrWhiteSpace(m.Content))
                    .Select(m => new Dictionary<string, string>
                    {
                        { "role",    m.Role    },
                        { "content", m.Content }
                    })
                    .ToList();

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

                _logger.LogInformation("Payload FastAPI: {Json}", json);

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

                // 3. Sauvegarder l'échange en DB
                await _motivationService.SauvegarderEchange(
                    sessionId,
                    request.Content,
                    result?.Reply ?? ""
                );

                return Ok(new ChatResponseDto
                {
                    SessionId = sessionId,
                    Reply = result?.Reply ?? "",
                });
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

        // ────────────────────────────────────────────────────────
        //  DELETE /api/motivation/sessions/{sessionId}
        //  Supprime une conversation
        // ────────────────────────────────────────────────────────
        [HttpDelete("sessions/{sessionId:int}")]
        public async Task<IActionResult> SupprimerSession(int sessionId)
        {
            var membreId = GetMembreId();
            var ok = await _motivationService.SupprimerSession(sessionId, membreId);

            if (!ok) return NotFound(new { message = "Session introuvable" });
            return Ok(new { message = "Conversation supprimée ✓" });
        }

        // ── Helper ────────────────────────────────────────────
        private int GetMembreId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }
}