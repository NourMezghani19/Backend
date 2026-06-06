using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class MessageContact
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string NomPrenom { get; set; } = "";

        [Required, MaxLength(150)]
        public string Email { get; set; } = "";

        [Required, MaxLength(200)]
        public string Sujet { get; set; } = "";

        [Required]
        public string Message { get; set; } = "";

        public bool Lu { get; set; } = false;

        public DateTime EnvoyeLe { get; set; } = DateTime.UtcNow;

        // Si le message vient d'un membre connecté (optionnel)
        public int? MembreId { get; set; }
        public Membre? Membre { get; set; }
    }
}
