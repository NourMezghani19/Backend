using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.DTOs;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService authService;

        public AuthController(AuthService authService)
        {
            this.authService = authService;
            
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await authService.SeConnecter(dto.Email, dto.Password);

            if (result == null)
                return Unauthorized(new
                {
                    message = "Email ou mot de passe incorrect",
                    success = false
                });

            return Ok(result);
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        [Authorize]
        public IActionResult Logout()
        {
            var userEmail = User.FindFirst("email")?.Value;
            return Ok(new { message = "Déconnexion réussie. À bientôt !", success = true });
        }


    }

}
