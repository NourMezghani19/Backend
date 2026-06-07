using backend.Data;
using backend.DTOs.Historique;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.Historique
{
    public class HistoriqueService
    {
        private readonly AppDbContext _db;

        public HistoriqueService(AppDbContext db)
        {
            _db = db;
        }

        // ─── Enregistrer un événement ─────────────────────────────────

        public async Task EnregistrerMiseAJourPoids(int membreId, float ancienPoids, float nouveauPoids, float taille)
        {
            float imc = CalculerImc(nouveauPoids, taille);
            await Ajouter(new HistoriqueEntry
            {
                MembreId = membreId,
                TypeEvenement = "MisePoids",
                AncienPoids = ancienPoids,
                NouveauPoids = nouveauPoids,
                ImcSnapshot = MathF.Round(imc, 1),
            });
        }

        public async Task EnregistrerMiseAJourTaille(int membreId, float ancienneTaille, float nouvelleTaille, float poids)
        {
            float imc = CalculerImc(poids, nouvelleTaille);
            await Ajouter(new HistoriqueEntry
            {
                MembreId = membreId,
                TypeEvenement = "MiseTaille",
                AncienneTaille = ancienneTaille,
                NouvelleTaille = nouvelleTaille,
                ImcSnapshot = MathF.Round(imc, 1),
            });
        }

        public async Task EnregistrerChangementObjectif(int membreId, float ancienObjectif, float nouvelObjectif)
        {
            await Ajouter(new HistoriqueEntry
            {
                MembreId = membreId,
                TypeEvenement = "ObjectifChange",
                AncienObjectifPoids = ancienObjectif,
                NouvelObjectifPoids = nouvelObjectif,
            });
        }

        public async Task EnregistrerInscription(int membreId, float poids, float taille)
        {
            float imc = CalculerImc(poids, taille);
            await Ajouter(new HistoriqueEntry
            {
                MembreId = membreId,
                TypeEvenement = "Inscription",
                NouveauPoids = poids,
                NouvelleTaille = taille,
                ImcSnapshot = MathF.Round(imc, 1),
            });
        }

        public async Task EnregistrerAvatarGenere(int membreId, float poids, float objectifPoids)
        {
            await Ajouter(new HistoriqueEntry
            {
                MembreId = membreId,
                TypeEvenement = "AvatarGenere",
                NouveauPoids = poids,
                NouvelObjectifPoids = objectifPoids,
                Note = "Avatar IA généré",
            });
        }

        // ─── GET : historique complet d'un membre ─────────────────────
        public async Task<HistoriqueResponseDto> GetHistorique(int membreId)
        {
            var items = await _db.Historiques
                .Where(h => h.MembreId == membreId)
                .OrderByDescending(h => h.Date)
                .ToListAsync();

            var itemDtos = items.Select(MapToDto).ToList();
            var resume = BuildResume(items);

            return new HistoriqueResponseDto
            {
                Items = itemDtos,
                Resume = resume,
            };
        }

        // ─── Mapping HistoriqueEntry → HistoriqueItemDto ──────────────
        private static HistoriqueItemDto MapToDto(HistoriqueEntry h)
        {
            return new HistoriqueItemDto
            {
                Id = h.Id,
                Date = h.Date.ToString("dd MMM yyyy"),
                Titre = BuildTitre(h),
                CouleurPoint = BuildCouleur(h),
                Chips = BuildChips(h),
            };
        }

        private static string BuildTitre(HistoriqueEntry h) => h.TypeEvenement switch
        {
            "MisePoids" => "Mise à jour poids",
            "MiseTaille" => "Mise à jour taille",
            "ObjectifChange" => "Objectif mis à jour",
            "Inscription" => "Inscription",
            "AvatarGenere" => "Avatar IA généré",
            _ => "Événement"
        };

        private static string BuildCouleur(HistoriqueEntry h) => h.TypeEvenement switch
        {
            "Inscription" => "rgba(225,6,0,0.7)",
            "ObjectifChange" => "#fb923c",
            "AvatarGenere" => "#a78bfa",
            "MisePoids" when h.NouveauPoids < h.AncienPoids => "#4ade80",
            "MisePoids" when h.NouveauPoids > h.AncienPoids => "#f87171",
            _ => "rgba(255,255,255,0.3)"
        };

        private static List<ChipDto> BuildChips(HistoriqueEntry h)
        {
            var chips = new List<ChipDto>();

            if (h.AncienPoids.HasValue && h.NouveauPoids.HasValue)
            {
                float diff = h.NouveauPoids.Value - h.AncienPoids.Value;
                chips.Add(new ChipDto
                {
                    Label = (diff >= 0 ? "+" : "") + diff.ToString("F1") + " kg",
                    Type = diff < 0 ? "down" : "up"
                });
            }
            else if (h.NouveauPoids.HasValue)
            {
                chips.Add(new ChipDto { Label = h.NouveauPoids.Value + " kg", Type = "neutre" });
            }

            if (h.AncienneTaille.HasValue && h.NouvelleTaille.HasValue)
            {
                bool stable = MathF.Abs(h.NouvelleTaille.Value - h.AncienneTaille.Value) < 0.1f;
                chips.Add(new ChipDto
                {
                    Label = stable
                        ? $"{h.NouvelleTaille.Value} cm → stable"
                        : $"{h.AncienneTaille.Value} cm → {h.NouvelleTaille.Value} cm",
                    Type = "neutre"
                });
            }
            else if (h.NouvelleTaille.HasValue)
            {
                chips.Add(new ChipDto { Label = h.NouvelleTaille.Value + " cm", Type = "neutre" });
            }

            if (h.AncienObjectifPoids.HasValue && h.NouvelObjectifPoids.HasValue)
            {
                chips.Add(new ChipDto
                {
                    Label = $"Objectif → {h.NouvelObjectifPoids.Value} kg",
                    Type = "neutre"
                });
            }

            if (h.ImcSnapshot.HasValue)
            {
                chips.Add(new ChipDto { Label = $"IMC {h.ImcSnapshot.Value}", Type = "neutre" });
            }

            return chips;
        }

        // ─── Résumé global ────────────────────────────────────────────
        private static HistoriqueResumeDto BuildResume(List<HistoriqueEntry> items)
        {
            var poidsEntries = items
                .Where(h => h.NouveauPoids.HasValue)
                .OrderBy(h => h.Date)
                .ToList();

            float kgPerdus = 0;
            if (poidsEntries.Count >= 2)
                kgPerdus = MathF.Round(
                    poidsEntries.Last().NouveauPoids!.Value - poidsEntries.First().NouveauPoids!.Value, 1);

            int moisActif = 0;
            if (items.Count >= 2)
            {
                var debut = items.Min(h => h.Date);
                var fin = items.Max(h => h.Date);
                moisActif = (int)Math.Round((fin - debut).TotalDays / 30.0);
                if (moisActif < 1) moisActif = 1;
            }

            var imcEntries = items.Where(h => h.ImcSnapshot.HasValue).OrderBy(h => h.Date).ToList();
            float variationImc = 0;
            if (imcEntries.Count >= 2)
                variationImc = MathF.Round(
                    imcEntries.Last().ImcSnapshot!.Value - imcEntries.First().ImcSnapshot!.Value, 1);

            return new HistoriqueResumeDto
            {
                KgPerdus = kgPerdus,
                MoisActif = moisActif,
                VariationImc = variationImc,
            };
        }

        // ─── Helpers ──────────────────────────────────────────────────
        private async Task Ajouter(HistoriqueEntry h)
        {
            _db.Historiques.Add(h);
            await _db.SaveChangesAsync();
        }

        private static float CalculerImc(float poids, float taille)
        {
            if (taille <= 0) return 0;
            float tailleM = taille / 100f;
            return poids / (tailleM * tailleM);
        }
    }
}