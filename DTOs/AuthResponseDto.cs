using Microsoft.AspNetCore.Identity;

namespace backend.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public string Message { get; set; } = "Connexion réussie";
    }
}
