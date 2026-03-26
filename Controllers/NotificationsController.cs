using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext db;

        public NotificationsController(AppDbContext db)
        {
            this.db = db;
        }

        // GET api/notifications/membre/5
        [HttpGet("membre/{membreId}")]
        public async Task<IActionResult> GetByMembre(int membreId)
        {
            var notifications = await db.Notifications
                .Where(n => n.UtilisateurId == membreId)
                .OrderByDescending(n => n.DateEnvoi)
                .ToListAsync();

            return Ok(notifications);
        }

        // GET api/notifications/membre/5/non-lues
        [HttpGet("membre/{membreId}/non-lues")]
        public async Task<IActionResult> GetNonLues(int membreId)
        {
            var notifications = await db.Notifications
                .Where(n => n.UtilisateurId == membreId && !n.Lue)
                .OrderByDescending(n => n.DateEnvoi)
                .ToListAsync();

            return Ok(notifications);
        }

        // ✅ NOUVEAU — GET api/notifications/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var notif = await db.Notifications.FindAsync(id);
            if (notif == null) return NotFound();
            return Ok(notif);
        }

        // PATCH api/notifications/5/lue
        [HttpPatch("{id}/lue")]
        public async Task<IActionResult> MarquerLue(int id)
        {
            var notif = await db.Notifications.FindAsync(id);
            if (notif == null) return NotFound();

            notif.Lue = true;
            await db.SaveChangesAsync();
            return NoContent();
        }

        // PATCH api/notifications/membre/5/tout-lire
        [HttpPatch("membre/{membreId}/tout-lire")]
        public async Task<IActionResult> ToutMarquerLu(int membreId)
        {
            var notifs = await db.Notifications
                .Where(n => n.UtilisateurId == membreId && !n.Lue)
                .ToListAsync();

            notifs.ForEach(n => n.Lue = true);
            await db.SaveChangesAsync();
            return NoContent();
        }
    }
}