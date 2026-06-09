using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Abonnement
    {
        public int Id { get; set; }

        [Required]
        public int MembreId { get; set; }
        public Membre? Membre { get; set; } 

        [Required]
        public int PlanAbonnementId { get; set; }
        public PlanAbonnement? Plan { get; set; }

        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public StatutAbonnement Statut { get; set; } = StatutAbonnement.Actif;
        public DateTime CreeLe { get; set; } = DateTime.UtcNow;
        public DateTime ModifieLe { get; set; } = DateTime.UtcNow;

        public ICollection<Paiement> Paiements { get; set; } = new List<Paiement>();
    }

    public enum StatutAbonnement { Actif, Expire, Suspendu, Annule }
}