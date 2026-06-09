namespace backend.DTOs.Paiement
{
    public class PaiementResponseDto
    {
        public int Id { get; set; }
        public string Reference { get; set; } = "";
        public int MembreId { get; set; }
        public string MembreNom { get; set; } = "";
        public string MembreEmail { get; set; } = "";
        public int AbonnementId { get; set; }
        public string PlanNom { get; set; } = "";
        public decimal Montant { get; set; }
        public string Statut { get; set; } = "";
        public DateTime DatePaiement { get; set; }
        public DateTime CreeLe { get; set; }
    }
}