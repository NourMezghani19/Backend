using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class SalleInformation
    {
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string NomSalle { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Adresse { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Telephone { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? LienFacebook { get; set; }
        [MaxLength(200)]
        public string? LienInstagram { get; set; }

        public List<HoraireJour> Horaires { get; set; } = new List<HoraireJour>();

        public bool Actif { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
