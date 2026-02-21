using backend.Data;
using backend.DTOs;
using backend.DTOs.Membre;
using backend.Services.MembreServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Membre
{
    [ApiController]
    [Route("api/membres")]
    [Authorize]
    public class MembreController : ControllerBase
    {
        private readonly MembreService svc;

        public MembreController(MembreService svc)
        {
            this.svc = svc;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMonProfil()
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);

            var profil = await svc.GetProfil(userId);

            if (profil == null)
                return NotFound();

            return Ok(profil);
        }


        [HttpPut("{id:int}")]
        public async Task<IActionResult> ModifierProfil(
            int id, [FromBody] UpdateMembreDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await svc.ModifierProfil(id, dto);

            if (result == null)
                return NotFound(new { message = $"Membre #{id} non trouvé" });

            return Ok(new
            {
                message = "Profil mis à jour avec succès ✓",
                success = true,
                profil = result
            });
        }

       
        [HttpPost("{id:int}/photo")]
        public async Task<IActionResult> UploadPhoto(
            int id, IFormFile photo)
        {
            if (photo == null)
                return BadRequest(new
                {
                    message = "Aucun fichier reçu. Envoyer un champ 'photo' en multipart/form-data"
                });

            var (success, message, url) = await svc.UploadPhotoProfile(id, photo);

            if (!success)
                return BadRequest(new { message, success = false });

            return Ok(new
            {
                message,
                success = true,
                photoUrl = url,
                fullUrl = $"{Request.Scheme}://{Request.Host}{url}"
            });
        }

        
        [HttpDelete("{id:int}/photo")]
        public async Task<IActionResult> SupprimerPhoto(
            int id, [FromServices] AppDbContext db)
        {
            var membre = await db.Membres.FindAsync(id);
            if (membre == null)
                return NotFound(new { message = $"Membre #{id} non trouvé" });

            membre.PhotoProfile = null;
            await db.SaveChangesAsync();

            return Ok(new { message = "Photo supprimée ✓", success = true });
        }
    }
}
