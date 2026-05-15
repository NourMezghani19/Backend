using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.SalleInformation
{
    public class ValiderCreneauRequestDto
    {
        [Required]
        public string Jour { get; set; } = string.Empty;

        [Required, RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "Format HH:mm requis")]
        public string HeureDebut { get; set; } = string.Empty;

        [Required, RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "Format HH:mm requis")]
        public string HeureFin { get; set; } = string.Empty;
    }
}