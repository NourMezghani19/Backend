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

    ////////////////////////////////////////////////////////////////////////       Get ALL cours 
    [HttpGet]
    [Authorize(Roles = "Administrateur,SuperAdministrateur")]
    public async Task<ActionResult<List<CoursResponseDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] GenreCours? genre)
        => Ok(await svc.GetAll(search, genre));

    ////////////////////////////////////////////////////////////////////////       Get le cours by id  

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
    ////////////////////////////////////////////////////////////////////////       Create un cours 
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
    ////////////////////////////////////////////////////////////////////////       Update un cours 

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
    ////////////////////////////////////////////////////////////////////////       Delete un cours 

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "SuperAdministrateur")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await svc.Delete(id);
            return NoContent();
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
    ////////////////////////////////////////////////////////////////////////  get sessions     

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
    ////////////////////////////////////////////////////////////////////////  planifier une session    

    [HttpPost("sessions")]
    [Authorize(Roles = "Administrateur,SuperAdministrateur")]
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
    ////////////////////////////////////////////////////////////////////////  Annuler une session   

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
    ////////////////////////////////////////////////////////////////////////  Modifier l'horaire d'une session    

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
    ////////////////////////////////////////////////////////////////////////  return les sessions disponible d'un cours     
    //////////////////////////////////////////////////////////////////////// return TOUTES les sessions disponibles  

    [HttpGet("disponibles")]
    [Authorize]
    public async Task<ActionResult<List<SessionResponseDto>>> GetDisponibles()
    {
        // On ne récupère plus le genre du token JWT.
        // On passe 'null' ou on appelle une surcharge du service qui ignore le genre.
        return Ok(await svc.GetSessionsDisponibles(null));
    }
    //modification de status
    [HttpPatch("{id:int}/actif")]
    public async Task<IActionResult> ToggleActif(int id)
    {
        try
        {
            var result = await svc.ToggleActif(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur interne" });
        }
    }
}