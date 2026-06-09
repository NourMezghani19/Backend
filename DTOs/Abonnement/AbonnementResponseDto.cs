namespace backend.DTOs.Abonnement
{
    public class AbonnementResponseDto
    {
        public int Id { get; set; }
        public int MembreId { get; set; }
        public string MembreNom { get; set; } = "";
        public string MembreEmail { get; set; } = "";
        public int PlanId { get; set; }
        public string PlanNom { get; set; } = "";
        public int DureeEnMois { get; set; }
        public string DureeLibelle { get; set; } = "";
        public decimal Prix { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public string Statut { get; set; } = "";
        public int JoursRestants { get; set; }
        public DateTime CreeLe { get; set; }
    }
}