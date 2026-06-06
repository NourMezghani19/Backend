using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    // Renommé en HistoriqueEntry pour éviter le conflit
    // avec le namespace backend.Services.Historique
    public class HistoriqueEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MembreId { get; set; }

        [ForeignKey("MembreId")]
        public Membre? Membre { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;

        // "MisePoids" | "MiseTaille" | "ObjectifChange" | "Inscription" | "AvatarGenere"
        [Required]
        [MaxLength(50)]
        public string TypeEvenement { get; set; } = string.Empty;

        public float? AncienPoids { get; set; }
        public float? NouveauPoids { get; set; }

        public float? AncienneTaille { get; set; }
        public float? NouvelleTaille { get; set; }

        public float? AncienObjectifPoids { get; set; }
        public float? NouvelObjectifPoids { get; set; }

        public float? ImcSnapshot { get; set; }

        [MaxLength(255)]
        public string? Note { get; set; }
    }
}