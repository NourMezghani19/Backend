namespace backend.DTOs.EmploiDuTemps
{
    public class StatsCoachDto
    {
        public int CoachId { get; set; }
        public string CoachNom { get; set; } = "";
        public int NbCreneaux { get; set; }
        public double HeuresParSemaine { get; set; }
    }
}
