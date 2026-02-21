using backend.Data;
using backend.DTOs;
using backend.DTOs.Membre;
using backend.Services;
using backend.Services.MembreServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers.Membre
{

    [ApiController]
    [Route("api/membres")]
    [Authorize]
    // Tout utilisateur connecté peut accéder
    // (chaque membre ne voit que son propre profil — vérifié dans le service)
    public class MembreController : ControllerBase
    {
        private readonly MembreService _svc;
        private readonly AuthService _authSvc;
        public MembreController(MembreService svc, AuthService authSvc)
        {
            _svc = svc;
            _authSvc = authSvc;
        }
        [Authorize(Policy = "Membre")]
        [HttpPut("modifier-mot-de-passe")]
        public async Task<IActionResult> ModifierMotDePasse([FromBody] ChangePasswordDto dto)
        {
            // Recherche le claim standard NameIdentifier
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Identifiant utilisateur introuvable dans le token." });
            }

            int userId = int.Parse(userIdClaim.Value);

            // Appel à votre service de changement de mot de passe
            var result = await _authSvc.ChangerMotDePasse(userId, dto.AncienMotDePasse, dto.NouveauMotDePasse);

            if (!result)
                return BadRequest(new { message = "L'ancien mot de passe est incorrect ou utilisateur introuvable." });

            return Ok(new { message = "Mot de passe modifié avec succès !" });
        }
        // ════════════════════════════════════════════════
        // GET /api/membres/{id}
        // Voir le profil complet d'un membre
        // ════════════════════════════════════════════════
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProfil(int id)
        {
            var profil = await _svc.GetProfil(id);
            if (profil == null)
                return NotFound(new { message = $"Membre #{id} non trouvé" });

            return Ok(profil);
            // Retourne : { id, nom, prenom, email, taille, poids, photoProfile,
            //             dateInscription, imc, categorieIMC }
        }

        // ════════════════════════════════════════════════
        // PUT /api/membres/{id} → modifierProfil()
        // Body : { nom, prenom, telephone, taille, poids }
        // ════════════════════════════════════════════════
        [HttpPut("{id:int}")]
        public async Task<IActionResult> ModifierProfil(
            int id, [FromBody] UpdateMembreDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _svc.ModifierProfil(id, dto);

            if (result == null)
                return NotFound(new { message = $"Membre #{id} non trouvé" });

            return Ok(new
            {
                message = "Profil mis à jour avec succès ✓",
                success = true,
                profil = result
            });
        }

        // ════════════════════════════════════════════════
        // POST /api/membres/{id}/photo
        // Upload la photo de profil du membre
        // Content-Type: multipart/form-data
        // Champ : photo (fichier image)
        // ════════════════════════════════════════════════
        [HttpPost("{id:int}/photo")]
        public async Task<IActionResult> UploadPhoto(
            int id, IFormFile photo)
        {
            if (photo == null)
                return BadRequest(new
                {
                    message = "Aucun fichier reçu. Envoyer un champ 'photo' en multipart/form-data"
                });

            var (success, message, url) = await _svc.UploadPhotoProfile(id, photo);

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

        // ════════════════════════════════════════════════
        // DELETE /api/membres/{id}/photo
        // Supprimer la photo de profil
        // ════════════════════════════════════════════════
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

