namespace backend.Models
{
    public class Coach
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Specialite { get; set; }
        public string Email { get; set; }
        public string? Telephone { get; set; }
        public string? PhotoProfile { get; set; }
        public bool Disponible { get; set; }
        public string? PhotoUrl { get; set; }
        public DateTime DateCreation { get; set; }= DateTime.UtcNow;
        public ICollection<Session_Cours> Sessions

        = new List<Session_Cours>();
    }
}
