namespace backend.DTOs.EmploiDuTemps
{
    public class CoachDisponibleDto
    {
        public int CoachId { get; set; }
        public string Prenom { get; set; } = "";
        public string Nom { get; set; } = "";
        public string? Photo { get; set; }
        public string Specialite { get; set; } = "";
        public List<PlageDto> Creneaux { get; set; } = [];
        public bool EstEnSession { get; set; } = false;
    }
}
