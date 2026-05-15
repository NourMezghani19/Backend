namespace backend.DTOs.SalleInformation
{
    public class ValiderCreneauResponseDto
    {
        public bool Valide { get; set; }
        public string Message { get; set; } = string.Empty;
        public HoraireJourDto? HoraireJour { get; set; }

    }
}