using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.DTOs.EmploiDuTemps
{
    public class CreerCreneauDto
    {
        [Required]
        public int CoachId { get; set; }
        /// =Dim 1=Lun 2=Mar 3=Mer 4=Jeu 5=Ven 6=Sam

        [Required]
        public DayOfWeek Jour { get; set; }

        [Required]
        [JsonConverter(typeof(TimeSpanConverter))]
        public TimeSpan HeureDebut { get; set; }

        [Required]
        [JsonConverter(typeof(TimeSpanConverter))]
        public TimeSpan HeureFin { get; set; }

        public string? Note { get; set; }
    }
}
