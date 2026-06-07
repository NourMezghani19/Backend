using System.Text.Json.Serialization;

namespace backend.DTOs.Historique
{
    public class HistoriqueItemDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; } = "";           // ex: "05 Jun 2025"

        [JsonPropertyName("titre")]
        public string Titre { get; set; } = "";          // ex: "Mise à jour poids & taille"

        [JsonPropertyName("couleur_point")]
        public string CouleurPoint { get; set; } = "";   // "#4ade80" | "#fb923c" | "#e10600" | "#fff"

        [JsonPropertyName("chips")]
        public List<ChipDto> Chips { get; set; } = new();
    }
}
