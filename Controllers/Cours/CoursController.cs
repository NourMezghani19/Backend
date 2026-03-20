using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs.Cours;
using backend.Models;
using backend.Services;
namespace backend.Controllers.Cours;

[ApiController]
[Route("api/[controller]")]
public class CoursController(CoursService svc) : ControllerBase
{


    // ── GET ALL — admin (search + genre optionnel) ────────


    [HttpGet]


    [Authorize(Roles = "Administrateur,SuperAdministrateur")]


    public async Task<IActionResult> GetAll(


        [FromQuery] string? search,


        [FromQuery] GenreCours? genre)


        => Ok(await svc.GetAll(search, genre));




    // ── GET BY ID ─────────────────────────────────────────


    [HttpGet("{id:int}")]


    [Authorize]


    public async Task<IActionResult> GetById(int id)


        => Ok(await svc.GetById(id));




    // ── GET SESSIONS DISPONIBLES — Membre ────────────────


    // Le backend lit le genre du membre depuis son token JWT


    [HttpGet("disponibles")]


    [Authorize]


    public async Task<IActionResult> GetDisponibles()


    {


        var genre = User.FindFirst("genre")?.Value;


        return Ok(await svc.GetSessionsDisponibles(genre));


    }




    // ── GET SESSIONS PAR COURS ────────────────────────────


    [HttpGet("{id:int}/sessions")]


    [Authorize]


    public async Task<IActionResult> GetSessions(int id)


        => Ok(await svc.GetSessionsByCours(id));




    // ── CRÉER UN COURS — Super Admin ──────────────────────


    [HttpPost]


    [Authorize(Roles = "Administrateur")]


    public async Task<IActionResult> Create(CreateCoursDto dto)


    {


        if (!ModelState.IsValid)


            return BadRequest(ModelState);


        try


        {


            var r = await svc.Create(dto);


            return CreatedAtAction(nameof(GetById),


                new { id = r.Id }, r);


        }


        catch (InvalidOperationException ex)


        { return Conflict(new { message = ex.Message }); }


    }




    // ── MODIFIER UN COURS — Super Admin ───────────────────


    [HttpPut("{id:int}")]


    [Authorize(Roles = "Administrateur")]


    public async Task<IActionResult> Update(int id, UpdateCoursDto dto)


    {


        try { return Ok(await svc.Update(id, dto)); }


        catch (KeyNotFoundException) { return NotFound(); }


    }




    // ── SUPPRIMER — Super Admin ───────────────────────────


    [HttpDelete("{id:int}")]


    [Authorize(Roles = "Administrateur")]


    public async Task<IActionResult> Delete(int id)


    {


        try { await svc.Delete(id); return Ok(); }


        catch (KeyNotFoundException) { return NotFound(); }


        catch (InvalidOperationException ex)


        { return Conflict(new { message = ex.Message }); }


    }




    // ── PLANIFIER SESSION — Super Admin ───────────────────


    [HttpPost("sessions")]


    [Authorize(Roles = "Administrateur")]


    public async Task<IActionResult> PlanifierSession(


        PlanifierSessionDto dto)


    {


        if (!ModelState.IsValid)


            return BadRequest(ModelState);


        try { return Ok(await svc.PlanifierSession(dto)); }


        catch (KeyNotFoundException ex)


        { return NotFound(new { message = ex.Message }); }


        catch (InvalidOperationException ex)


        { return Conflict(new { message = ex.Message }); }


    }




    // ── ANNULER SESSION — Administrateur ──────────────────


    [HttpPut("sessions/{id:int}/annuler")]


    [Authorize(Roles = "Administrateur,SuperAdministrateur")]


    public async Task<IActionResult> AnnulerSession(int id)


    {


        try { await svc.AnnulerSession(id); return Ok(); }


        catch (KeyNotFoundException) { return NotFound(); }


        catch (InvalidOperationException ex)


        { return Conflict(new { message = ex.Message }); }


    }




    // ── MODIFIER HORAIRE SESSION — Administrateur ─────────


    [HttpPut("sessions/{id:int}/horaire")]


    [Authorize(Roles = "Administrateur,SuperAdministrateur")]


    public async Task<IActionResult> ModifierHoraire(


        int id, ModifierHoraireDto dto)


    {


        try { return Ok(await svc.ModifierHoraire(id, dto)); }


        catch (KeyNotFoundException) { return NotFound(); }


        catch (InvalidOperationException ex)


        { return Conflict(new { message = ex.Message }); }


    }


}

