using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs.Programmes;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Membre")]
public class ProgrammeController(ProgrammeService svc) : ControllerBase
{
    private int GetMembreId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException());

    // GET api/programme
    [HttpGet]
    public async Task<ActionResult<List<ProgrammeResponseDto>>> GetAll()
        => Ok(await svc.GetAllByMembre(GetMembreId()));

    // GET api/programme/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProgrammeResponseDto>> GetById(int id)
    {
        try { return Ok(await svc.GetById(id, GetMembreId())); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }

    // POST api/programme
    [HttpPost]
    public async Task<ActionResult<ProgrammeResponseDto>> Create(CreateProgrammeDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var r = await svc.Create(dto, GetMembreId());
            return CreatedAtAction(nameof(GetById), new { id = r.Id }, r);
        }
        catch (Exception ex) { return Conflict(new { message = ex.Message }); }
    }

    // DELETE api/programme/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { await svc.Delete(id, GetMembreId()); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
    }
}