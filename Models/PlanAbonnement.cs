using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class PlanAbonnement
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nom { get; set; } = "";           

        [MaxLength(300)]
        public string? Description { get; set; }

        [Required]
        public int DureeEnMois { get; set; }           

        [Required, Column(TypeName = "decimal(8,2)")]
        public decimal Prix { get; set; }              

        public bool EstActif { get; set; } = true;

        public DateTime CreeLe { get; set; } = DateTime.UtcNow;
        public DateTime ModifieLe { get; set; } = DateTime.UtcNow;
    }
}
