using System.Text.Json.Serialization;

namespace backend.DTOs.Historique
{
    public class HistoriqueResumeDto
    {
        [JsonPropertyName("kg_perdus")]
        public float KgPerdus { get; set; }              // ex: -1.5

        [JsonPropertyName("mois_actif")]
        public int MoisActif { get; set; }               // ex: 5

        [JsonPropertyName("variation_imc")]
        public float VariationImc { get; set; }          // ex: -0.4
    }
}
