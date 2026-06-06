using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.PlanAbonnement
{
    public class CreerPlanDto
    {
        [Required, MaxLength(100)]
        public string Nom { get; set; } = "";

        [MaxLength(300)]
        public string? Description { get; set; }

        [Required, Range(1, 36)]
        public int DureeEnMois { get; set; }

        [Required, Range(0.01, 99999)]
        public decimal Prix { get; set; }

        public bool EstActif { get; set; } = true;
    }
}
