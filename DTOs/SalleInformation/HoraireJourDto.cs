namespace backend.DTOs.SalleInformation
{
    public class HoraireJourDto
    {
        public string Jour { get; set; } = string.Empty; // "lundi", "mardi", etc.
        public string HeureOuverture { get; set; } = "06:00";
        public string HeureFermeture { get; set; } = "22:00";
        public bool EstOuvert { get; set; } = true;
    }
}
