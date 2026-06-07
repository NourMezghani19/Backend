using System.Text.Json.Serialization;

namespace backend.DTOs.Historique
{
    public class ChipDto
    {
        [JsonPropertyName("label")]
        public string Label { get; set; } = "";          // ex: "-1.5 kg" | "IMC 26.4"

        [JsonPropertyName("type")]
        public string Type { get; set; } = "neutre";     // "up" | "down" | "neutre"
    }
}
