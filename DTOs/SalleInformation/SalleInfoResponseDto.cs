namespace backend.DTOs.SalleInformation
{
    public class SalleInfoResponseDto
    {
        public int Id { get; set; }
        public string NomSalle { get; set; } = string.Empty;
        public string Adresse { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;
        public string? LienFacebook { get; set; }
        public string? LienInstagram { get; set; }
        public List<HoraireJourDto> Horaires { get; set; } = new List<HoraireJourDto>();
        public bool Actif { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
