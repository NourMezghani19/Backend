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
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        public GenreCours Genre { get; set; }


        // ✅ Navigation — PAS d'initialisation ici (= new List<> bloque EF)
        public virtual ICollection<Session_Cours> SessionsCours { get; set; }
            = new List<Session_Cours>();
    }
}
