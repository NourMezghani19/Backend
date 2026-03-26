namespace backend.DTOs.EmploiDuTemps
{
    public class CreneauResponseDto
    {
        public int Id { get; set; }
        public int CoachId { get; set; }
        public string CoachNom { get; set; } = "";
        public string? CoachPhoto { get; set; }

        public string Jour { get; set; }       
        public string HeureDebut { get; set; } = "";  
        public string HeureFin { get; set; } = "";  
        
        public string? Note { get; set; }
        public DateTime CreeLe { get; set; }
    }
}
