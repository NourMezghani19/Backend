using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Contact
{
    public class EnvoyerMessageDto
    {
        [Required, MaxLength(100)]
        public string NomPrenom { get; set; } = "";

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = "";

        [Required, MaxLength(200)]
        public string Sujet { get; set; } = "";

        [Required]
        public string Message { get; set; } = "";
    }
}
