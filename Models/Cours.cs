using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Cours
    {
        public int Id { get; set; }

        [Required]
        public string Nom { get; set; } = "";

        public string? Description { get; set; }

        [Range(1, 30)]
        public int CapaciteMax { get; set; } = 20;

        public bool Actif { get; set; } = true;

        public GenreCours Genre { get; set; }

        // ✅ Ajout de { get; set; } — c'était un field, pas une property
        public ICollection<Session_Cours> SessionsCours { get; set; } = new List<Session_Cours>();
    }
}