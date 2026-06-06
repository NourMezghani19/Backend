using System.Text.Json.Serialization;

namespace backend.DTOs.Historique
{
    public class HistoriqueResponseDto
    {
        [JsonPropertyName("items")]
        public List<HistoriqueItemDto> Items { get; set; } = new();

        [JsonPropertyName("resume")]
        public HistoriqueResumeDto Resume { get; set; } = new();
    }
}
