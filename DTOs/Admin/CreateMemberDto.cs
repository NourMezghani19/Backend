using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Admin
{
    public class CreateMembreDto
    {
        [Required(ErrorMessage = "L'ID salle de sport est obligatoire")]
        [RegularExpression(@"^SPORT-\d{4}-\d{3}$",
        ErrorMessage = "Format ID invalide. Format attendu : SPORT-XXXX-XXX")]
        public string IdSalleSport { get; set; } = string.Empty;  // ← NOUVEAU

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [MaxLength(80)]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est obligatoire")]
        [MaxLength(80)]
        public string Prenom { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "Format email invalide")]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        [Phone(ErrorMessage = "Numéro de téléphone invalide")]
        public string? Telephone { get; set; }

        [Range(50, 250, ErrorMessage = "Taille entre 50 et 250 cm")]
        public float Taille { get; set; }

        [Range(20, 300, ErrorMessage = "Poids entre 20 et 300 kg")]
        public float Poids { get; set; }
        
    }
}
