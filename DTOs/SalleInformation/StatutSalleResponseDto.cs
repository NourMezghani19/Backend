namespace backend.DTOs.SalleInformation
{
    public class StatutSalleResponseDto
    {
        public bool EstOuverte { get; set; }
        public string JourActuel { get; set; } = string.Empty;
        public string HeureActuelle { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

}

