namespace backend.DTOs
{
    public class UpdateProfilDto
    {
        public string Nom { get; set; } = null!;
        public string Prenom { get; set; } = null!;
        public string Telephone { get; set; } = null!;
        public string MotDePasse { get; set; } = null!;
    }
}
