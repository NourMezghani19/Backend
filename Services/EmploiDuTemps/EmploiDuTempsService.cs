using backend.Data;
using backend.DTOs.EmploiDuTemps;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.EmploiDuTemps
{
    public class EmploiDuTempsService
    {
        private readonly AppDbContext db;

        public EmploiDuTempsService(AppDbContext db)
        {
            this.db = db;
        }

        private static readonly string[] Couleurs =
        [
            "#4f8ef7","#f7934f","#3ecf8e","#c084fc",
            "#f75f5f","#f7d44f","#38bdf8","#fb7185"
        ];

        private static readonly string[] JoursNoms =
        [
            "Dimanche","Lundi","Mardi","Mercredi",
            "Jeudi","Vendredi","Samedi"
        ];

        // ── 1. CRÉER ──────────────────────────────────────────────
        public async Task<CreneauResponseDto> Creer(CreerCreneauDto dto)
        {
            if (dto.HeureDebut >= dto.HeureFin)
                throw new ArgumentException(
                    "L'heure de début doit être avant l'heure de fin.");

            // Conflit : même coach, même jour, chevauchement horaire
            var conflit = await this.db.EmploisDuTemps.AnyAsync(e =>
                e.CoachId == dto.CoachId &&
                e.Jour == dto.Jour &&
                e.HeureDebut < dto.HeureFin &&
                e.HeureFin > dto.HeureDebut);

            if (conflit)
                throw new InvalidOperationException(
                    "Conflit : ce coach a déjà un créneau qui chevauche cet horaire ce jour.");

            var entite = new Models.EmploiDuTemps
            {
                CoachId = dto.CoachId,
                Jour = dto.Jour,
                HeureDebut = dto.HeureDebut,
                HeureFin = dto.HeureFin,
                Note = dto.Note,
                CreeLe = DateTime.UtcNow
            };

            this.db.EmploisDuTemps.Add(entite);
            await this.db.SaveChangesAsync();
            await this.db.Entry(entite).Reference(e => e.Coach).LoadAsync();
            return MapToResponse(entite);
        }

        // ── 2. CALENDRIER SEMAINE ─────────────────────────────────
        // Retourne tous les créneaux hebdomadaires
        // Le frontend calcule les dates réelles selon la semaine affichée
        public async Task<List<CalendrierEventDto>> GetCalendrier(int? coachId = null)
        {
            var query = this.db.EmploisDuTemps
                .Include(e => e.Coach)
                .AsQueryable();

            if (coachId.HasValue)
                query = query.Where(e => e.CoachId == coachId.Value);

            var liste = await query
                .OrderBy(e => e.Jour)
                .ThenBy(e => e.HeureDebut)
                .ToListAsync();

            // Palette couleur stable par coach
            var coachIds = liste.Select(e => e.CoachId).Distinct().ToList();
            var couleurMap = coachIds
                .Select((id, i) => new { id, col = Couleurs[i % Couleurs.Length] })
                .ToDictionary(x => x.id, x => x.col);

            return liste.Select(e => new CalendrierEventDto
            {
                Id = $"edt-{e.Id}",
                EdtId = e.Id,
                Title = $"{e.Coach!.Prenom} {e.Coach.Nom}",
                CoachId = e.CoachId,
                Jour = JoursNoms[(int)e.Jour],
                HeureDebut = e.HeureDebut.ToString(@"hh\:mm"),
                HeureFin = e.HeureFin.ToString(@"hh\:mm"),
                Note = e.Note,
                Color = couleurMap.GetValueOrDefault(e.CoachId, Couleurs[0]),
                Photo = e.Coach.PhotoUrl
            }).ToList();
        }

        // ── 3. GET PAR ID ─────────────────────────────────────────
        public async Task<CreneauResponseDto> GetById(int id)
        {
            var e = await this.db.EmploisDuTemps
                .Include(e => e.Coach)
                .FirstOrDefaultAsync(e => e.Id == id)
                ?? throw new KeyNotFoundException($"Créneau #{id} introuvable.");

            return MapToResponse(e);
        }

        // ── 4. CRÉNEAUX D'UN COACH ────────────────────────────────
        public async Task<List<CreneauResponseDto>> GetByCoach(int coachId)
        {
            return await this.db.EmploisDuTemps
                .Include(e => e.Coach)
                .Where(e => e.CoachId == coachId)
                .OrderBy(e => e.Jour)
                .ThenBy(e => e.HeureDebut)
                .Select(e => MapToResponse(e))
                .ToListAsync();
        }

        // ── 5. SUPPRIMER ──────────────────────────────────────────
        public async Task Supprimer(int id)
        {
            var entite = await this.db.EmploisDuTemps.FindAsync(id)
                ?? throw new KeyNotFoundException($"Créneau #{id} introuvable.");

            this.db.EmploisDuTemps.Remove(entite);
            await this.db.SaveChangesAsync();
        }

        public async Task<List<CoachDisponibleDto>> GetDisponiblesAujourdhui()
        {
            var maintenant = DateTime.Now;
            var jourAujourd = maintenant.DayOfWeek;

            // Récupérer tous les créneaux du jour
            var creneaux = await this.db.EmploisDuTemps
                .Include(e => e.Coach)
                .Where(e => e.Jour == jourAujourd)
                .OrderBy(e => e.HeureDebut)
                .ToListAsync();

            var result = new List<CoachDisponibleDto>();

            foreach (var group in creneaux.GroupBy(e => e.CoachId))
            {
                var coach = group.First().Coach!;

                var creneauxDto = group.Select(e => new PlageDto
                {
                    HeureDebut = e.HeureDebut.ToString(@"hh\:mm"),
                    HeureFin = e.HeureFin.ToString(@"hh\:mm"),
                    Note = e.Note
                }).ToList();

                // Vérifier si le coach est EN SÉANCE MAINTENANT
                bool estEnSessionMaintenant = group.Any(e =>
                    maintenant.TimeOfDay >= e.HeureDebut &&
                    maintenant.TimeOfDay <= e.HeureFin);

                result.Add(new CoachDisponibleDto
                {
                    CoachId = coach.Id,
                    Prenom = coach.Prenom,
                    Nom = coach.Nom,
                    Photo = coach.PhotoUrl,
                    Specialite = coach.Specialite,
                    Creneaux = creneauxDto,
                    EstEnSession = estEnSessionMaintenant   // ← NOUVEAU champ
                });
            }

            return result
                .OrderBy(c => c.Prenom)
                .ToList();
        }
        // ── 6. MODIFIER ───────────────────────────────────────────────
        public async Task<CreneauResponseDto> Modifier(int id, CreerCreneauDto dto)
        {
            var entite = await this.db.EmploisDuTemps.FindAsync(id)
                ?? throw new KeyNotFoundException($"Créneau #{id} introuvable.");

            if (dto.HeureDebut >= dto.HeureFin)
                throw new ArgumentException("L'heure de début doit être avant l'heure de fin.");

            // Conflit : même coach, même jour, chevauchement — en excluant le créneau lui-même
            var conflit = await this.db.EmploisDuTemps.AnyAsync(e =>
                e.Id != id &&
                e.CoachId == entite.CoachId &&
                e.Jour == entite.Jour &&
                e.HeureDebut < dto.HeureFin &&
                e.HeureFin > dto.HeureDebut);

            if (conflit)
                throw new InvalidOperationException(
                    "Conflit : ce coach a déjà un créneau qui chevauche cet horaire ce jour.");

            entite.HeureDebut = dto.HeureDebut;
            entite.HeureFin = dto.HeureFin;
            entite.Note = dto.Note;

            await this.db.SaveChangesAsync();
            await this.db.Entry(entite).Reference(e => e.Coach).LoadAsync();
            return MapToResponse(entite);
        }
        // ── 7. STATS ──────────────────────────────────────────────
        public async Task<List<StatsCoachDto>> GetStats()
        {
            var data = await this.db.EmploisDuTemps
                .Include(e => e.Coach)
                .ToListAsync();

            return data
                .GroupBy(e => e.CoachId)
                .Select(g => new StatsCoachDto
                {
                    CoachId = g.Key,
                    CoachNom = $"{g.First().Coach!.Prenom} {g.First().Coach.Nom}",
                    NbCreneaux = g.Count(),
                    HeuresParSemaine = Math.Round(
                        g.Sum(e => (e.HeureFin - e.HeureDebut).TotalHours), 1)
                })
                .OrderBy(x => x.CoachNom)
                .ToList();
        }

        // ── HELPER ────────────────────────────────────────────────
        private static CreneauResponseDto MapToResponse(Models.EmploiDuTemps e) => new()
        {
            Id = e.Id,
            CoachId = e.CoachId,
            CoachNom = e.Coach is null ? "" : $"{e.Coach.Prenom} {e.Coach.Nom}",
            CoachPhoto = e.Coach?.PhotoUrl,
            Jour = JoursNoms[(int)e.Jour],
            HeureDebut = e.HeureDebut.ToString(@"hh\:mm"),
            HeureFin = e.HeureFin.ToString(@"hh\:mm"),
            Note = e.Note,
            CreeLe = e.CreeLe
        };
    }
}