namespace backend.Models
{
    public class ReglementInterne
    {
        public int Id { get; set; }
        public string Titre { get; set; } = "Règlement Intérieur";
        public string Contenu { get; set; } = ""; // JSON structuré
        public DateTime DateModification { get; set; } = DateTime.UtcNow;
        public int ModifieParId { get; set; }
    }
}
