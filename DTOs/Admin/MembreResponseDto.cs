namespace backend.DTOs.Admin
{
    public class MembreResponseDto
    {

        public int Id { get; set; }
        public string IdSalleSport { get; set; } = string.Empty;  

        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telephone { get; set; }
        public float Taille { get; set; }
        public float Poids { get; set; }
        public string? PhotoProfile { get; set; }
        public DateTime DateInscription { get; set; }
      
    }
}
