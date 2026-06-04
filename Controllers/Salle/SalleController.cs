// backend/Controllers/SalleController.cs
using backend.DTOs.Salle;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalleController(SalleService svc) : ControllerBase
{
    // GET /api/Salle — Tous les rôles connectés
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
        => Ok(await svc.GetAll());

    // POST /api/Salle — Admin + SA
    [HttpPost]
    [Authorize(Roles = "Administrateur,SuperAdministrateur")]
    public async Task<IActionResult> Create(CreateSalleDto dto)
    {
        try { return Ok(await svc.Create(dto)); }
        catch (Exception ex)
        { return BadRequest(new { message = ex.Message }); }
    }

    // DELETE /api/Salle/{id} — Admin + SA
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrateur,SuperAdministrateur")]
    public async Task<IActionResult> Delete(int id)
    {
        try { await svc.Delete(id); return Ok(); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex)
        { return Conflict(new { message = ex.Message }); }
    }

    // PUT /api/Salle/sessions/{id}/assigner — Admin + SA
    [HttpPut("sessions/{id:int}/assigner")]
    [Authorize(Roles = "Administrateur,SuperAdministrateur")]
    public async Task<IActionResult> Assigner(int id, AssignerSalleDto dto)
    {
        try
        {
            await svc.AssignerSalle(id, dto.SalleId);
            return Ok(new { message = "Salle assignée" });
        }
        catch (KeyNotFoundException ex)
        { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex)
        { return Conflict(new { message = ex.Message }); }
    }

    // GET /api/Salle/emploi-global — Coach + Admin + SA
    [HttpGet("emploi-global")]
    [Authorize(Roles = "Coach,Administrateur,SuperAdministrateur")] // <── Sécurisé avec tes rôles
    public async Task<IActionResult> GetEmploiGlobal()
    {
        try
        {
            var planning = await svc.GetEmploiGlobalAsync();
            return Ok(planning);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur lors de la récupération du planning unifié.", details = ex.Message });
        }
    }
}