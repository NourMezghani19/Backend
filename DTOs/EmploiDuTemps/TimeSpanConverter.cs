using System.Text.Json;
using System.Text.Json.Serialization;

namespace backend.DTOs.EmploiDuTemps
{
    public class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type t, JsonSerializerOptions o)
        {
            var s = reader.GetString() ?? "00:00:00";
            // Normaliser : "12:00" → "12:00:00"
            if (s.Length == 5) s += ":00";
            return TimeSpan.Parse(s);
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions o)
            => writer.WriteStringValue(value.ToString(@"hh\:mm\:ss"));
    }
}
