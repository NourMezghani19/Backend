using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Membre
{
    public class ChangePasswordDto
    {
        [Required]
        public string AncienMotDePasse { get; set; } = string.Empty;

        [Required]
        [MinLength(6, ErrorMessage = "Le nouveau mot de passe doit faire au moins 6 caractères")]
        public string NouveauMotDePasse { get; set; } = string.Empty;

        [Compare("NouveauMotDePasse", ErrorMessage = "La confirmation ne correspond pas")]
        public string ConfirmationMotDePasse { get; set; } = string.Empty;
    }
}