// Controllers/ReglementInterneController.cs
using backend.DTOs.ReglementInterne;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class ReglementInterneController(ReglementInterneService svc) : ControllerBase
{
    // GET /api/ReglementInterne — accessible à tous les connectés
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Get()
    {
        try
        {
            return Ok(await svc.GetActuel());
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // PUT /api/ReglementInterne — SuperAdmin seulement
    [HttpPut]
    [Authorize(Roles = "SuperAdministrateur")]
    public async Task<IActionResult> Update(UpdateReglementInterneDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await svc.Update(dto, adminId));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST /api/ReglementInterne/rappel — Envoyer rappel à tous les membres
    [HttpPost("rappel")]
    [Authorize(Roles = "SuperAdministrateur")]
    public async Task<IActionResult> EnvoyerRappel()
    {
        try
        {
            await svc.EnvoyerRappelATous();
            return Ok(new { message = "Rappel du règlement intérieur envoyé à tous les membres." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // POST /api/ReglementInterne/membre/{id} — Envoyer à un membre spécifique
    [HttpPost("membre/{membreId:int}")]
    [Authorize(Roles = "SuperAdministrateur,Administrateur")]
    public async Task<IActionResult> EnvoyerAMembre(int membreId)
    {
        try
        {
            await svc.EnvoyerAMembre(membreId);
            return Ok(new { message = "Règlement intérieur envoyé au membre." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}