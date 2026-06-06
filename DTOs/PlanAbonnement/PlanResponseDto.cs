namespace backend.DTOs.PlanAbonnement
{
    public class PlanResponseDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = "";
        public string? Description { get; set; }
        public int DureeEnMois { get; set; }
        public string DureeLibelle { get; set; } = ""; // "1 Mois", "1 An"
        public decimal Prix { get; set; }
        public bool EstActif { get; set; }
        public DateTime CreeLe { get; set; }
        public DateTime ModifieLe { get; set; }
    }
}
