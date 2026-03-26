namespace backend.DTOs.Coach
{
    public class CoachSearchDto
    {
        public int Id { get; set; }
        public string Prenom { get; set; } = null!;
        public string Nom { get; set; } = null!;
        public string NomComplet { get; set; } = null!;
        public string? PhotoUrl { get; set; }
        public string Specialite { get; set; } = null!;
    }
}
