using System.Text.Json.Serialization;

namespace backend.DTOs.Avatar
{
    public class AvatarResponseDto
    {
        [JsonPropertyName("imc_actuel")]
        public float ImcActuel { get; set; }

        [JsonPropertyName("categorie_actuel")]
        public string CategorieActuel { get; set; } = "";

        [JsonPropertyName("svg_actuel")]
        public string SvgActuel { get; set; } = "";

        [JsonPropertyName("morph_actuel")]
        public MorphParamsDto MorphActuel { get; set; } = new();

        [JsonPropertyName("adjustments_actuel")]
        public Dictionary<string, float> AdjustmentsActuel { get; set; } = new();

        [JsonPropertyName("imc_objectif")]
        public float ImcObjectif { get; set; }

        [JsonPropertyName("categorie_objectif")]
        public string CategorieObjectif { get; set; } = "";

        [JsonPropertyName("svg_objectif")]
        public string SvgObjectif { get; set; } = "";

        [JsonPropertyName("morph_objectif")]
        public MorphParamsDto MorphObjectif { get; set; } = new();

        [JsonPropertyName("adjustments_objectif")]
        public Dictionary<string, float> AdjustmentsObjectif { get; set; } = new();
    }
}