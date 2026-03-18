using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Coach
{
    public class UpdateCoachDto
    {
        public string? Nom { get; set; }
        public string? Prenom { get; set; }
        public string? Specialite { get; set; }
        [RegularExpression(@"^\d{8}$", ErrorMessage = "Le téléphone doit contenir exactement 8 chiffres")]

        public string? Telephone { get; set; }
        public string? Email { get; set; }
        public string? PhotoUrl { get; set; }
      //  public bool? Disponible { get; set; }
    }
}
