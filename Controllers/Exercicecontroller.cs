using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs.Exercices;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Membre")]
public class ExerciceController(ExerciceService svc) : ControllerBase
{
    // Récupère l'id du membre depuis le JWT
    private int GetMembreId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Token invalide."));

    // ────────────────────────────────────────────────────────
    //  GET  api/exercice
    //  Lister tous les exercices du membre connecté
    // ────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<ActionResult<List<ExerciceResponseDto>>> GetAll()
        => Ok(await svc.GetAllByMembre(GetMembreId()));

    // ────────────────────────────────────────────────────────
    //  GET  api/exercice/{id}
    //  Voir le détail d'un exercice (répétitions, séries, mode)
    // ────────────────────────────────────────────────────────
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExerciceResponseDto>> GetById(int id)
    {
        try
        {
            return Ok(await svc.GetById(id, GetMembreId()));
        }
        catch (KeyNotFoundException) { return NotFound(new { message = "Exercice introuvable." }); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    // ────────────────────────────────────────────────────────
    //  POST  api/exercice
    //  Ajouter un exercice
    // ────────────────────────────────────────────────────────
    [HttpPost]
    public async Task<ActionResult<ExerciceResponseDto>> Create(CreateExerciceDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var r = await svc.Create(dto, GetMembreId());
        return CreatedAtAction(nameof(GetById), new { id = r.Id }, r);
    }

    // ────────────────────────────────────────────────────────
    //  PUT  api/exercice/{id}
    //  Modifier un exercice
    // ────────────────────────────────────────────────────────
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ExerciceResponseDto>> Update(int id, UpdateExerciceDto dto)
    {
        try { return Ok(await svc.Update(id, dto, GetMembreId())); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    // ────────────────────────────────────────────────────────
    //  DELETE  api/exercice/{id}
    //  Supprimer un exercice
    // ────────────────────────────────────────────────────────
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await svc.Delete(id, GetMembreId());
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    // ────────────────────────────────────────────────────────
    //  GET  api/exercice/programme
    //  Générer automatiquement un programme personnalisé
    // ────────────────────────────────────────────────────────
    [HttpGet("programme")]
    public async Task<ActionResult<ProgrammeResponseDto>> GetProgramme()
    {
        try { return Ok(await svc.GenererProgramme(GetMembreId())); }
        catch (InvalidOperationException ex)
        { return Conflict(new { message = ex.Message }); }
    }
}