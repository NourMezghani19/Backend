using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Avis
{
    public class CreerAvisDto
    {
        [Required, MaxLength(500)]
        public string Commentaire { get; set; } = "";

        [Range(1, 5)]
        public int Note { get; set; } = 5;
        public string? NomAnonyme { get; set; }

    }
}
