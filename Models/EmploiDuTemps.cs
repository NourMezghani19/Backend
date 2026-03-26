using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class EmploiDuTemps
    {
        public int Id { get; set; }

        [Required]
        public int CoachId { get; set; }
        public Coach? Coach { get; set; }

        [Required]
        public DayOfWeek Jour { get; set; }   

        [Required]
        public TimeSpan HeureDebut { get; set; }  

        [Required]
        public TimeSpan HeureFin { get; set; }   

        public string? Note { get; set; }

        public DateTime CreeLe { get; set; } = DateTime.UtcNow;

    }
}
