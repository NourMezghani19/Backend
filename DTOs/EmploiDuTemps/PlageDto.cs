namespace backend.DTOs.EmploiDuTemps
{
    public class PlageDto
    {
        public string HeureDebut { get; set; } = "";  // "08:00"
        public string HeureFin { get; set; } = "";  // "14:00"
        public string? Note { get; set; }
    }
}
