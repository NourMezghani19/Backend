namespace backend.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Titre { get; set; } = "";
        public string Contenu { get; set; } = "";
        public string Type { get; set; } = "";
        public bool Lue { get; set; }
        public DateTime DateEnvoi { get; set; }
    }
}
