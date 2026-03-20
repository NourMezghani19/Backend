using backend.DTOs.Coach;
using backend.Services.Coach;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Coach
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdministrateur")]
    public class CoachController : ControllerBase
    {
        private readonly CoachService _svc;

        public CoachController(CoachService svc)
        {
            _svc = svc;
        }

        // GET /api/Coach
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _svc.GetAll();
            return Ok(result);
        }

        // GET /api/Coach/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _svc.GetById(id);
            return Ok(result);
        }

        // POST /api/Coach
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCoachDto dto)
        {
            var result = await _svc.Create(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        // PUT /api/Coach/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCoachDto dto)
        {
            var result = await _svc.Update(id, dto);
            return Ok(result);
        }

        // DELETE /api/Coach/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _svc.Delete(id);
            return NoContent();
        }
    }
}
