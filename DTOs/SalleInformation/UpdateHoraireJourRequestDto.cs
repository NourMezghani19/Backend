namespace backend.DTOs.SalleInformation
{
    public class UpdateHoraireJourRequestDto
    {
        public string? HeureOuverture { get; set; }
        public string? HeureFermeture { get; set; }
        public bool? EstOuvert { get; set; }
    }
}
