namespace backend.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public string Titre { get; set; } = "";

        public string Contenu { get; set; } = "";

        public string Type { get; set; } = "";

        public int UtilisateurId { get; set; }

        public DateTime DateEnvoi { get; set; } = DateTime.UtcNow;

        public bool Lue { get; set; } = false;
    }
}
