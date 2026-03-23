using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs.Cours;
using backend.Models;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursController(CoursService svc) : ControllerBase
{
    // ════════════════════════════════════════════════════
    // SECTION COURS
    // ════════════════════════════════════════════════════

    [HttpGet]
    [Authorize(Roles = "Administrateur,SuperAdministrateur")]
    public async Task<ActionResult<List<CoursResponseDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] GenreCours? genre)
        => Ok(await svc.GetAll(search, genre));


    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<CoursResponseDto>> GetById(int id)
    {
        try
        {
            return Ok(await svc.GetById(id));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Cours introuvable." });
        }
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdministrateur")]
    public async Task<ActionResult<CoursResponseDto>> Create(CreateCoursDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var r = await svc.Create(dto);
            return CreatedAtAction(nameof(GetById), new { id = r.Id }, r);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "SuperAdministrateur")]
    public async Task<ActionResult<CoursResponseDto>> Update(int id, UpdateCoursDto dto)
    {
        try
        {
            return Ok(await svc.Update(id, dto));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "SuperAdministrateur")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await svc.Delete(id);
            return NoContent(); // 204 est standard pour une suppression réussie
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // ════════════════════════════════════════════════════
    // SECTION SESSIONS
    // ════════════════════════════════════════════════════

    // Liste des sessions pour un cours spécifique
    [HttpGet("{id:int}/sessions")]
    [Authorize(Roles = "Administrateur,SuperAdministrateur")]
    public async Task<ActionResult<List<SessionResponseDto>>> GetSessions(int id)
    {
        try
        {
            return Ok(await svc.GetSessionsByCours(id));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Cours introuvable." });
        }
    }

    [HttpPost("sessions")]
    [Authorize(Roles = "SuperAdministrateur")]
    public async Task<ActionResult<SessionResponseDto>> PlanifierSession(PlanifierSessionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            return Ok(await svc.PlanifierSession(dto));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("sessions/{id:int}/annuler")]
    [Authorize(Roles = "Administrateur,SuperAdministrateur")]
    public async Task<IActionResult> AnnulerSession(int id)
    {
        try
        {
            await svc.AnnulerSession(id);
            return Ok(new { message = "Session annulée avec succès." });
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpPut("sessions/{id:int}/horaire")]
    [Authorize(Roles = "Administrateur,SuperAdministrateur")]
    public async Task<ActionResult<SessionResponseDto>> ModifierHoraire(int id, ModifierHoraireDto dto)
    {
        try
        {
            return Ok(await svc.ModifierHoraire(id, dto));
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpGet("disponibles")]
    [Authorize]
    public async Task<ActionResult<List<SessionResponseDto>>> GetDisponibles()
    {
        // On récupère le genre depuis le token JWT
        var genre = User.FindFirst("genre")?.Value;
        return Ok(await svc.GetSessionsDisponibles(genre));
    }
}