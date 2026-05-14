using backend.Data;
using backend.DTOs.SalleInformation;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.SalleInformation
{
    public class SalleInformationService
    {
        private readonly AppDbContext db;

        public SalleInformationService(AppDbContext db)
        {
            this.db = db;
        }

        // ───────────── Helpers ─────────────
        private static int ToMinutes(string heure)
        {
            var parts = heure.Split(':');
            return int.Parse(parts[0]) * 60 + int.Parse(parts[1]);
        }

        private static SalleInfoResponseDto MapToDto(backend.Models.SalleInformation s)
        {
            return new SalleInfoResponseDto
            {
                Id = s.Id,
                NomSalle = s.NomSalle,
                Adresse = s.Adresse,
                Telephone = s.Telephone,
                LienFacebook = s.LienFacebook,
                LienInstagram = s.LienInstagram,
                Horaires = s.Horaires.Select(h => new HoraireJourDto
                {
                    Jour = h.Jour,
                    HeureOuverture = h.HeureOuverture,
                    HeureFermeture = h.HeureFermeture,
                    EstOuvert = h.EstOuvert
                }).ToList(),
                Actif = s.Actif,
                UpdatedAt = s.UpdatedAt
            };
        }

        // ───────────── GET ─────────────
        public async Task<SalleInfoResponseDto?> GetInfoSalle()
        {
            var salle = await db.SalleInformations
                .Include(s => s.Horaires)
                .FirstOrDefaultAsync();

            if (salle == null) return null;

            // Dédoublonner par sécurité
            var dto = MapToDto(salle);
            dto.Horaires = dto.Horaires
                .GroupBy(h => h.Jour)
                .Select(g => g.First())
                .ToList();

            return dto;
        }

        // ───────────── INIT ─────────────
        public async Task<SalleInfoResponseDto> InitSalle()
        {
            var existe = await db.SalleInformations.AnyAsync();

            if (existe)
                throw new InvalidOperationException("La salle est déjà initialisée");

            var salle = new backend.Models.SalleInformation
            {
                NomSalle = "Salle QLF Qym",
                Adresse = "Route afrane, Sfax",
                Telephone = "+216 XX XXX XXX",
                LienFacebook = "https://facebook.com/glfgym",
                LienInstagram = "https://instagram.com/glfgym",
                Actif = true,
                Horaires = new List<HoraireJour>
        {
            new() { Jour = "lundi",    HeureOuverture = "07:00", HeureFermeture = "22:00", EstOuvert = true  },
            new() { Jour = "mardi",    HeureOuverture = "07:00", HeureFermeture = "22:00", EstOuvert = true  },
            new() { Jour = "mercredi", HeureOuverture = "07:00", HeureFermeture = "22:00", EstOuvert = true  },
            new() { Jour = "jeudi",    HeureOuverture = "07:00", HeureFermeture = "22:00", EstOuvert = true  },
            new() { Jour = "vendredi", HeureOuverture = "07:00", HeureFermeture = "22:00", EstOuvert = true  },
            new() { Jour = "samedi",   HeureOuverture = "08:00", HeureFermeture = "20:00", EstOuvert = true  },
            new() { Jour = "dimanche", HeureOuverture = "08:00", HeureFermeture = "14:00", EstOuvert = false },
        }
            };

            db.SalleInformations.Add(salle);
            await db.SaveChangesAsync();

            return MapToDto(salle);
        }


        // ───────────── UPDATE INFO GLOBALE ─────────────
        public async Task<SalleInfoResponseDto?> UpdateInfoGlobale(UpdateInfoGlobaleRequestDto req)
        {
            var salle = await db.SalleInformations
                .Include(s => s.Horaires)
                .FirstOrDefaultAsync();

            if (salle == null) return null;

            if (req.NomSalle != null) salle.NomSalle = req.NomSalle;
            if (req.Adresse != null) salle.Adresse = req.Adresse;
            if (req.Telephone != null) salle.Telephone = req.Telephone;
            if (req.LienFacebook != null) salle.LienFacebook = req.LienFacebook;
            if (req.LienInstagram != null) salle.LienInstagram = req.LienInstagram;
            if (req.Actif != null) salle.Actif = req.Actif.Value;

            salle.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return MapToDto(salle);
        }

        // ───────────── UPDATE HORAIRE D'UN JOUR ─────────────
        public async Task<SalleInfoResponseDto?> UpdateHoraireJour(string jour, UpdateHoraireJourRequestDto req)
        {
            var salle = await db.SalleInformations
                .Include(s => s.Horaires)
                .FirstOrDefaultAsync();

            if (salle == null) return null;

            var horaire = salle.Horaires.FirstOrDefault(h => h.Jour == jour.ToLower());
            if (horaire == null) return null;

            if (req.HeureOuverture != null) horaire.HeureOuverture = req.HeureOuverture;
            if (req.HeureFermeture != null) horaire.HeureFermeture = req.HeureFermeture;
            if (req.EstOuvert != null) horaire.EstOuvert = req.EstOuvert.Value;

            salle.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return MapToDto(salle);
        }
        // ───────────── DELETE (optionnel) ─────────────
        public async Task<bool> DeleteSalle()
        {
            var salle = await db.SalleInformations.FirstOrDefaultAsync();

            if (salle == null)
                return false;

            db.SalleInformations.Remove(salle);
            await db.SaveChangesAsync();

            return true;
        }

        // ───────────── STATUT ACTUEL ─────────────
        public async Task<StatutSalleResponseDto> GetStatutSalle()
        {
            var salle = await db.SalleInformations.FirstOrDefaultAsync();

            if (salle == null)
                return new StatutSalleResponseDto
                {
                    EstOuverte = false,
                    Message = "Salle non configurée"
                };

            var now = DateTime.Now;
            var jours = new[] { "dimanche", "lundi", "mardi", "mercredi", "jeudi", "vendredi", "samedi" };
            var jourActuel = jours[(int)now.DayOfWeek];
            var heureActuelle = now.ToString("HH:mm");

            var horaire = salle.Horaires.FirstOrDefault(h => h.Jour == jourActuel);

            if (horaire == null || !horaire.EstOuvert)
            {
                return new StatutSalleResponseDto
                {
                    EstOuverte = false,
                    JourActuel = jourActuel,
                    HeureActuelle = heureActuelle,
                    Message = "Salle fermée"
                };
            }

            var nowMin = ToMinutes(heureActuelle);
            var ouverture = ToMinutes(horaire.HeureOuverture);
            var fermeture = ToMinutes(horaire.HeureFermeture);

            var ouverte = nowMin >= ouverture && nowMin <= fermeture;

            return new StatutSalleResponseDto
            {
                EstOuverte = ouverte,
                JourActuel = jourActuel,
                HeureActuelle = heureActuelle,
                Message = ouverte ? "Salle ouverte" : "Hors horaires"
            };
        }

        // ───────────── VALIDER CRÉNEAU ─────────────
        public async Task<ValiderCreneauResponseDto> ValiderCreneau(ValiderCreneauRequestDto req)
        {
            var jours = new[] { "lundi", "mardi", "mercredi", "jeudi", "vendredi", "samedi", "dimanche" };
            var jour = req.Jour.ToLower();

            if (!jours.Contains(jour))
                return new ValiderCreneauResponseDto
                {
                    Valide = false,
                    Message = "Jour invalide"
                };

            var salle = await db.SalleInformations.FirstOrDefaultAsync();

            if (salle == null)
                return new ValiderCreneauResponseDto
                {
                    Valide = false,
                    Message = "Salle non configurée"
                };

            var horaire = salle.Horaires.FirstOrDefault(h => h.Jour == jour);

            if (horaire == null || !horaire.EstOuvert)
                return new ValiderCreneauResponseDto
                {
                    Valide = false,
                    Message = $"Salle fermée le {jour}"
                };

            var debut = ToMinutes(req.HeureDebut);
            var fin = ToMinutes(req.HeureFin);
            var ouverture = ToMinutes(horaire.HeureOuverture);
            var fermeture = ToMinutes(horaire.HeureFermeture);

            if (debut >= fin)
                return new ValiderCreneauResponseDto
                {
                    Valide = false,
                    Message = "Heure début doit être avant heure fin"
                };

            if (debut < ouverture)
                return new ValiderCreneauResponseDto
                {
                    Valide = false,
                    Message = "Avant ouverture"
                };

            if (fin > fermeture)
                return new ValiderCreneauResponseDto
                {
                    Valide = false,
                    Message = "Après fermeture"
                };

            return new ValiderCreneauResponseDto
            {
                Valide = true,
                Message = "Créneau valide",
                HoraireJour = new HoraireJourDto
                {
                    Jour = horaire.Jour,
                    HeureOuverture = horaire.HeureOuverture,
                    HeureFermeture = horaire.HeureFermeture,
                    EstOuvert = horaire.EstOuvert
                }
            };
        }
    }
}
