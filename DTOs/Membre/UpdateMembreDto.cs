using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class UpdateMembreDto
    {
        [Required(ErrorMessage = "Le nom est obligatoire")]
        [MaxLength(80)]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est obligatoire")]
        [MaxLength(80)]
        public string Prenom { get; set; } = string.Empty;

        [MaxLength(20)]
        [Phone(ErrorMessage = "Numéro invalide")]
        public string? Telephone { get; set; }

        [Range(50, 250, ErrorMessage = "Taille entre 50 et 250 cm")]
        public float Taille { get; set; }

        [Range(20, 300, ErrorMessage = "Poids entre 20 et 300 kg")]
        public float Poids { get; set; }

        public float ObjectifPoids { get; set; }

        public DateTime? DateNaissance { get; set; }
    }

}

