using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public abstract class Utilisateur
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
    ErrorMessage = "L'email doit respecter le format xxxx@xxxx.xxxx")]
        public string Email { get; set; } = string.Empty;
        [RegularExpression(@"^\d{8}$",
    ErrorMessage = "Le téléphone doit contenir exactement 8 chiffres")]
        public string? Telephone { get; set; }
        [Required]
        public string MotDePasse { get; set; } = string.Empty;

        [Required]
        public string Nom { get; set; } = string.Empty;

        [Required]
        public string Prenom { get; set; } = string.Empty;

       // public string? Telephone { get; set; }

        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        // Discriminator pour EF Core (SuperAdmin / Admin / Membre)
        public string Role { get; set; } = "Membre";

        // ── Méthodes du diagramme UML ──
        public abstract void SeConnecter();
        public abstract void SeDeconnecter();
        public abstract void ModifierProfil();
    }
}
