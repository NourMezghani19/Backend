namespace backend.DTOs.SuperAdministrateur
{
    public class AdminReponseDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telephone { get; set; }
        public DateTime DateCreation { get; set; }
  
    }
}
