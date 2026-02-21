using backend.Data;
using backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext context;
        public UserController(AppDbContext ctx) => this.context = ctx;

        // ── PUT /api/users/{id} → modifierProfil() ──
        [HttpPut("{id}")]

        public async Task<IActionResult> ModifierProfil(int id,
            [FromBody] UpdateProfilDto dto)
        {
            var user = await context.Utilisateurs.FindAsync(id);
            if (user == null) return NotFound();

            user.Nom = dto.Nom;
            user.Prenom = dto.Prenom;
            user.Telephone = dto.Telephone;

            await context.SaveChangesAsync();
            return Ok(new { message = "Profil modifié ✓" });
        }
        [HttpGet]
        [Authorize(Roles = "SuperAdministrateur")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await context.Utilisateurs
                .Where(u => u.Role == "Membre")
                .Select(u => new
                {
                    u.Id,
                    u.Nom,
                    u.Prenom,
                    u.Email,
                    u.Telephone,
                    u.Role,
                    u.DateCreation
                })
                .OrderByDescending(u => u.DateCreation)
                .ToListAsync();

            return Ok(new { count = users.Count, users });
        }
    }
}
