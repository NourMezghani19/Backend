using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.ReglementInterne
{
    public class UpdateReglementInterneDto
    {
        [Required]
        public string Titre { get; set; } = "";

        [Required]
        public string Contenu { get; set; } = "";
    }
}
