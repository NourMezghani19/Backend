namespace backend.DTOs.Paiement
{
    public class FiltragePaiementDto
    {
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public int? Annee { get; set; }
        public int? Mois { get; set; }
        public int? MembreId { get; set; }
        public string? Statut { get; set; }
    }

    public class StatJournalierDto
    {
        public DateTime Jour { get; set; }
        public decimal TotalMontant { get; set; }
        public int NombrePaiements { get; set; }
    }
}