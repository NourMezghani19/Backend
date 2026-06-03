using System.Text.Json.Serialization;

namespace backend.DTOs.Motivation
{
    public class MessageDto
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }
}
