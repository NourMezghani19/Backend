namespace backend.DTOs.SuperAdministrateur
{
    public class CreateCompteDto
    {
        public string Nom { get; set; } = null!;
        public string Prenom { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Telephone { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string MotDePasse { get; set; } = null!;
    }
}
