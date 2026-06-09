using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Paiement
    {
        public int Id { get; set; }

        [Required]
        public int MembreId { get; set; }
        public Membre? Membre { get; set; } 

        [Required]
        public int AbonnementId { get; set; }
        public Abonnement? Abonnement { get; set; }

        [Required, Column(TypeName = "decimal(8,2)")]
        public decimal Montant { get; set; }

        public StatutPaiement Statut { get; set; } = StatutPaiement.Valide;
        public string Reference { get; set; } = "";
        public DateTime DatePaiement { get; set; } = DateTime.UtcNow;
        public DateTime CreeLe { get; set; } = DateTime.UtcNow;
    }

    public enum StatutPaiement { EnAttente, Valide, Refuse, Rembourse }
}