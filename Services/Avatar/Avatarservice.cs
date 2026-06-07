using backend.DTOs.Avatar;
using backend.Services.Avatar.Interfaces;

namespace backend.Services.Avatar
{
    public class AvatarService : IAvatarService
    {
        // ─── Seuils OMS ───────────────────────────────────────────────
        private const float MAIGREUR = 18.5f;
        private const float NORMAL_MAX = 24.9f;
        private const float SURPOIDS = 29.9f;
        private const float OBESITE_1 = 34.9f;
        private const float OBESITE_2 = 39.9f;

        // Perte/prise saine : ~2 kg/mois
        private const float KG_PAR_MOIS = 2f;

        // ─── IMC ──────────────────────────────────────────────────────
        public float CalculerImc(float poids, float taille)
        {
            if (taille <= 0) throw new ArgumentException("Taille invalide.");
            float tailleM = taille / 100f;
            return poids / (tailleM * tailleM);
        }

        public string GetCategorieImc(float imc) => imc switch
        {
            < MAIGREUR => "Maigreur",
            <= NORMAL_MAX => "Normal",
            <= SURPOIDS => "Surpoids",
            <= OBESITE_1 => "Obésité modérée",
            <= OBESITE_2 => "Obésité sévère",
            _ => "Obésité morbide"
        };

        // ─── Comparaison ──────────────────────────────────────────────
        public AvatarComparisonDto Comparer(float taille, float poidsActuel, float poidsObjectif)
        {
            float imcActuel = CalculerImc(poidsActuel, taille);
            float imcObjectif = CalculerImc(poidsObjectif, taille);

            float diffPoids = MathF.Round(poidsObjectif - poidsActuel, 1);  // négatif = perte
            float diffImc = MathF.Round(imcObjectif - imcActuel, 1);

            string statut = diffPoids < -0.5f ? "Perte"
                          : diffPoids > 0.5f ? "Prise"
                          : "Maintien";

            // ── Barre "% objectif poids" ─────────────────────────────
            // Représente combien de kg reste à perdre/prendre sur le total
            // Si objectif = 72, actuel = 82, total = 10 kg
            // Si l'utilisateur est à 82 (début) → 0% fait, 100% reste
            // Le frontend affiche "68% objectif" = % du chemin déjà parcouru
            // Ici on calcule depuis le poids de départ (on n'a pas le poids initial,
            // donc on utilise actuel vs objectif : 0% parcouru au moment de la génération)
            // → Le frontend peut stocker le poids initial séparément.
            // Pour la maquette : progressionPoidsPct = ratio poids_objectif / poids_actuel * 100
            float progressionPoidsPct = poidsActuel <= 0 ? 0
                : MathF.Round((poidsObjectif / poidsActuel) * 100f, 1);

            // ── Barre IMC ─────────────────────────────────────────────
            // % de progression vers la zone "Normal" (18.5 → 24.9)
            // Si IMC actuel = 25.9 et objectif = 22.5 → bien amélioré
            float imcNormalMid = (MAIGREUR + NORMAL_MAX) / 2f;  // 21.7
            float progressionImcPct = MathF.Round(
                Math.Clamp(100f - ((imcActuel - imcNormalMid) / imcNormalMid * 100f), 0f, 100f), 1);

            string labelImc = imcObjectif switch
            {
                <= NORMAL_MAX when imcActuel <= NORMAL_MAX => "Normal ✓",
                <= NORMAL_MAX => "Bonne voie",
                <= SURPOIDS => "En progrès",
                _ => "À améliorer"
            };

            // ── Durée estimée ─────────────────────────────────────────
            int moisEstimes = diffPoids == 0
                ? 0
                : (int)MathF.Ceiling(MathF.Abs(diffPoids) / KG_PAR_MOIS);

            string message = BuildMessage(statut, diffPoids, imcObjectif);

            return new AvatarComparisonDto
            {
                PoidsActuel = poidsActuel,
                PoidsObjectif = poidsObjectif,
                DifferencePoids = diffPoids,
                ImcActuel = MathF.Round(imcActuel, 1),
                ImcObjectif = MathF.Round(imcObjectif, 1),
                DifferenceImc = diffImc,
                CategorieActuel = GetCategorieImc(imcActuel),
                CategorieObjectif = GetCategorieImc(imcObjectif),
                ProgressionPoidsPct = progressionPoidsPct,
                ProgressionImcPct = progressionImcPct,
                LabelImc = labelImc,
                MoisEstimes = moisEstimes,
                Statut = statut,
                Message = message,
            };
        }

        // ─── Message motivationnel ────────────────────────────────────
        private static string BuildMessage(string statut, float diffPoids, float imcObjectif) =>
            statut switch
            {
                "Perte" => imcObjectif < MAIGREUR
                    ? $"Attention : perdre {MathF.Abs(diffPoids)} kg vous placerait en maigreur. Consultez un professionnel."
                    : $"Objectif : perdre {MathF.Abs(diffPoids)} kg. À raison de 2 kg/mois, vous y serez en {(int)MathF.Ceiling(MathF.Abs(diffPoids) / KG_PAR_MOIS)} mois !",
                "Prise" => $"Objectif : prendre {diffPoids} kg. Continuez l'entraînement !",
                _ => "Vous êtes déjà à votre poids idéal. Maintenez le cap !"
            };
    }
}