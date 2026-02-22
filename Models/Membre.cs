using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Membre : Utilisateur
    {
        [Required]
        [MaxLength(20)]
        public string IdSalleSport { get; set; } = string.Empty; 
        public float Taille { get; set; }
        public float Poids { get; set; }
        public string? PhotoProfile { get; set; }
        public DateTime DateInscription { get; set; } = DateTime.UtcNow;
        public override void SeConnecter() { }
        public override void SeDeconnecter() { }
        public override void ModifierProfil() { }
    }
}
