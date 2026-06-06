// DTOs/Avis/AvisResponseDto.cs
namespace backend.DTOs.Avis
{
    public class AvisResponseDto
    {
        public int Id { get; set; }
        public int? MembreId { get; set; }
        public string AuteurNom { get; set; } = "";
        public bool EstAnonyme { get; set; }
        public string Commentaire { get; set; } = "";
        public int Note { get; set; }
        public bool EstBloque { get; set; }
        public DateTime CreeLe { get; set; }
    }
}