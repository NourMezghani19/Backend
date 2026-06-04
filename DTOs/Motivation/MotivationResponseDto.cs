using System.Text.Json.Serialization;

namespace backend.DTOs.Motivation
{
    public class MotivationResponseDto
    {
        [JsonPropertyName("reply")]
        public string Reply { get; set; } = string.Empty;
    }
}