using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Abonnement
{
    public class AffecterAbonnementDto
    {
        [Required]
        public int MembreId { get; set; }

        [Required]
        public int PlanAbonnementId { get; set; }

        public DateTime? DateDebut { get; set; }
    }
}