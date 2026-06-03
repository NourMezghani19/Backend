using System.Text.Json.Serialization;

namespace backend.DTOs.Avatar
{
    public class MorphParamsDto
    {
        [JsonPropertyName("shoulder_width")]
        public float ShoulderWidth { get; set; }

        [JsonPropertyName("waist_width")]
        public float WaistWidth { get; set; }

        [JsonPropertyName("hip_width")]
        public float HipWidth { get; set; }

        [JsonPropertyName("belly")]
        public float Belly { get; set; }

        [JsonPropertyName("arm_thickness")]
        public float ArmThickness { get; set; }

        [JsonPropertyName("leg_thickness")]
        public float LegThickness { get; set; }
    }
}