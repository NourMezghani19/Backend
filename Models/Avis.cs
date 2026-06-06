// Models/Avis.cs
using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Avis
    {
        public int Id { get; set; }

        public int? MembreId { get; set; }
        public Membre? Membre { get; set; }

        [MaxLength(100)]
        public string? NomAnonyme { get; set; }

        [Required, MaxLength(500)]
        public string Commentaire { get; set; } = "";

        [Range(1, 5)]
        public int Note { get; set; } = 5;

        public bool EstBloque { get; set; } = false;

        public DateTime CreeLe { get; set; } = DateTime.UtcNow;
    }
}