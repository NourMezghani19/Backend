using System.Text.Json.Serialization;

namespace backend.DTOs.Avatar
{
    public class AvatarComparisonDto
    {
        // ── Poids ──────────────────────────────────────────────────
        [JsonPropertyName("poids_actuel")]
        public float PoidsActuel { get; set; }

        [JsonPropertyName("poids_objectif")]
        public float PoidsObjectif { get; set; }

        [JsonPropertyName("difference_poids")]
        public float DifferencePoids { get; set; }          // ex: -10

        // ── IMC ────────────────────────────────────────────────────
        [JsonPropertyName("imc_actuel")]
        public float ImcActuel { get; set; }

        [JsonPropertyName("imc_objectif")]
        public float ImcObjectif { get; set; }              // "22.5 IMC cible"

        [JsonPropertyName("difference_imc")]
        public float DifferenceImc { get; set; }

        [JsonPropertyName("categorie_actuel")]
        public string CategorieActuel { get; set; } = "";   // "Normal"

        [JsonPropertyName("categorie_objectif")]
        public string CategorieObjectif { get; set; } = "";

        // ── Barres de progression (frontend les affiche en %) ──────
        [JsonPropertyName("progression_poids_pct")]
        public float ProgressionPoidsPct { get; set; }      // ex: 68  → "68% objectif"

        [JsonPropertyName("progression_imc_pct")]
        public float ProgressionImcPct { get; set; }        // ex: 55  → barre IMC

        [JsonPropertyName("label_imc")]
        public string LabelImc { get; set; } = "";          // "Bonne voie" | "Normal ✓" | etc.

        // ── Évolution prévue ───────────────────────────────────────
        [JsonPropertyName("mois_estimes")]
        public int MoisEstimes { get; set; }                 // ~4 mois

        // ── Statut & message ───────────────────────────────────────
        [JsonPropertyName("statut")]
        public string Statut { get; set; } = "";            // "Perte" | "Prise" | "Maintien"

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";
    }
}