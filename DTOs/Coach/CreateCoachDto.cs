using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Coach
{
    public class CreateCoachDto
    {
        [Required]
        public string Nom { get; set; } = null!;

        [Required]
        public string Prenom { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;
        [RegularExpression(@"^\d{8}$", ErrorMessage = "Le téléphone doit contenir exactement 8 chiffres")]

        public string? Telephone { get; set; }

        [Required]
        public string Specialite { get; set; } = null!;

        public string? PhotoUrl { get; set; }


    }
}
