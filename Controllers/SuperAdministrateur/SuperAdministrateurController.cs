using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs.SuperAdministrateur;
using backend.Services.SuperAdminstrateur;

namespace backend.Controllers;

[ApiController]
[Route("api/superadmin")]

[Authorize(Roles = "SuperAdministrateur")]
public class SuperAdminController : ControllerBase
{
    private readonly SuperAdministrateurService svc;

    public SuperAdminController(SuperAdministrateurService svc)
    {
        this.svc = svc;
    }


    [HttpGet("administrateur")]
    public async Task<IActionResult> GetAdmins()
    {
        var admins = await svc.GetAllAdmins();
        return Ok(new { count = admins.Count, admins });
    }

    [HttpGet("administrateur/{id:int}")]
    public async Task<IActionResult> GetAdmin(int id)
    {
        var admin = await svc.GetAdminById(id);

        return admin == null
            ? NotFound(new { message = $"Administrateur #{id} non trouvé", success = false })
            : Ok(admin);
    }

    [HttpGet("superadmin")]
    public async Task<IActionResult> GetSuperAdmins()
    {
        var superAdmins = await svc.GetAllSuperAdmins();
        return Ok(new { count = superAdmins.Count, superAdmins });
    }

   
    [HttpGet("superadmin/{id:int}")]
    public async Task<IActionResult> GetSuperAdmin(int id)
    {
        var superAdmin = await svc.GetSuperAdminById(id);

        return superAdmin == null
            ? NotFound(new { message = $"SuperAdministrateur #{id} non trouvé", success = false })
            : Ok(superAdmin);
    }

    [HttpPost("comptes")]
    public async Task<IActionResult> CreateCompte([FromBody] CreateCompteDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await svc.CreateCompte(dto);

            return CreatedAtAction(
                dto.Role == "SuperAdministrateur"
                    ? nameof(GetSuperAdmin)
                    : nameof(GetAdmin),
                new { id = result.Id },
                new
                {
                    message = $" '{result.Nom}   {result.Prenom}' créé avec succès ✓",
                    success = true,
                    data = result
                });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message,
                success = false
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message,
                success = false
            });
        }
    }

    [HttpDelete("admins/{id:int}")]
    public async Task<IActionResult> SupprimerAdmin(int id)
    {
        var ok = await svc.SupprimerAdministrateur(id);

        return ok
            ? Ok(new { message = $"Administrateur #{id} supprimé ✓", success = true })
            : NotFound(new { message = $"Administrateur #{id} non trouvé", success = false });
    }
}