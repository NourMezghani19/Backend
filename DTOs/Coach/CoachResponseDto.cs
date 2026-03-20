namespace backend.DTOs.Coach
{
    public class CoachResponseDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = null!;
        public string Prenom { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Telephone { get; set; } = null!;
        public string Specialite { get; set; } = null!;
        public bool Disponible { get; set; }
        public string? PhotoUrl { get; set; }
        public int NbSessions { get; set; }
        public DateTime DateCreation { get; set; }
    }
}
