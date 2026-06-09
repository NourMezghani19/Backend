using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _service;
        public NotificationController(NotificationService service) => _service = service;

        // GET /api/notification
        [HttpGet]
        public async Task<IActionResult> GetMes()
        {
            var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await _service.GetMesNotifications(id));
        }

        // PUT /api/notification/{id}/lue
        [HttpPut("{id}/lue")]
        public async Task<IActionResult> MarquerLue(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            try { await _service.MarquerLue(id, userId); return NoContent(); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }
    }
}