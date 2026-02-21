using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.DTOs.Admin;

namespace backend.Services.Admin
{
    public class AdminService
    {
        private readonly AppDbContext db;
        private readonly EmailService email;

        // ════════════════════════════════════════════════
        // IDs prédéfinis de la salle de sport
        // Dans un vrai projet → vient de la base de données
        // Pour le PFA → liste fixe dans le code
        // ════════════════════════════════════════════════
        private static readonly HashSet<string> _idsSalleValides = new()
    {
        "SPORT-2024-001",
        "SPORT-2024-002",
        "SPORT-2024-003",
        "SPORT-2024-004",
        "SPORT-2024-005",
        "SPORT-2024-006",
        "SPORT-2024-007",
        "SPORT-2024-008",
        "SPORT-2024-009",
        "SPORT-2024-010"
    };

        // IDs déjà utilisés pour créer un compte
        // (évite de créer 2 comptes avec le même ID)
        private static readonly HashSet<string> _idsDejaUtilises = new();

        public AdminService(AppDbContext db, EmailService email)
        {
            this.db = db;
            this.email = email;
        }

        // ════════════════════════════════════════════════
        // VÉRIFIER si un ID salle est valide
        // Appelé par l'Admin avant de créer le compte
        // ════════════════════════════════════════════════
        public VerificationIdResult VerifierIdSalle(string idSalle)
        {
            // 1. ID existe dans la liste de la salle ?
            if (!_idsSalleValides.Contains(idSalle))
                return new VerificationIdResult
                {
                    Valide = false,
                    Message = $"ID '{idSalle}' non reconnu par la salle de sport",
                    IdSalle = idSalle
                };

            // 2. ID déjà utilisé pour créer un compte ?
            if (_idsDejaUtilises.Contains(idSalle))
                return new VerificationIdResult
                {
                    Valide = false,
                    Message = $"ID '{idSalle}' a déjà été utilisé pour créer un compte",
                    IdSalle = idSalle
                };

            // 3. ID valide et disponible ✓
            return new VerificationIdResult
            {
                Valide = true,
                Message = $"ID '{idSalle}' valide — vous pouvez créer le compte",
                IdSalle = idSalle
            };
        }

        // ════════════════════════════════════════════════
        // creerCompteMembre() — avec vérification ID salle
        // ════════════════════════════════════════════════
        public async Task<MembreResponseDto> CreerCompteMembre(CreateMembreDto dto)
        {
            // ── ÉTAPE 1 : Vérifier l'ID salle de sport ──
            var verification = VerifierIdSalle(dto.IdSalleSport);
            if (!verification.Valide)
                throw new InvalidOperationException(verification.Message);

            // ── ÉTAPE 2 : Vérifier email unique ──
            var existe = await db.Utilisateurs
                .AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (existe)
                throw new InvalidOperationException(
                    $"Un compte avec l'email '{dto.Email}' existe déjà");

            // ── ÉTAPE 3 : Générer mot de passe temporaire ──
            var motDePasseTemp = GenererMotDePasse();

            // ── ÉTAPE 4 : Créer le membre ──
            var membre = new Membre
            {
                IdSalleSport = dto.IdSalleSport,
                Nom = dto.Nom.Trim(),
                Prenom = dto.Prenom.Trim(),
                Email = dto.Email.ToLower().Trim(),
                MotDePasse = BCrypt.Net.BCrypt.HashPassword(motDePasseTemp),
                Telephone = dto.Telephone?.Trim(),
                Taille = dto.Taille,
                Poids = dto.Poids,
                Role = "Membre",
                DateCreation = DateTime.UtcNow,
                DateInscription = DateTime.UtcNow
            };

            // ── ÉTAPE 5 : Sauvegarder en DB ──
            db.Membres.Add(membre);
            await db.SaveChangesAsync();

            // ── ÉTAPE 6 : Marquer l'ID comme utilisé ──
            _idsDejaUtilises.Add(dto.IdSalleSport);

            // ── ÉTAPE 7 : Envoyer email avec mot de passe temp ──
            await email.EnvoyerEmailInscription(
                membre.Email,
                $"{membre.Prenom} {membre.Nom}",
                motDePasseTemp);

            return MapToDto(membre);
        }

        // ════════════════════════════════════════════════
        // Liste membres avec recherche
        // ════════════════════════════════════════════════
        public async Task<List<MembreResponseDto>> GetAllMembres(string? search = null)
        {
            var query = db.Membres.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(m =>
                    m.Nom.ToLower().Contains(s) ||
                    m.Prenom.ToLower().Contains(s) ||
                    m.Email.ToLower().Contains(s) ||
                    m.IdSalleSport.ToLower().Contains(s)); // ← recherche par ID aussi
            }

            return await query
                .OrderByDescending(m => m.DateInscription)
                .Select(m => new MembreResponseDto
                {
                    Id = m.Id,
                    IdSalleSport = m.IdSalleSport,   // ← inclure dans la réponse
                    Nom = m.Nom,
                    Prenom = m.Prenom,
                    Email = m.Email,
                    Telephone = m.Telephone,
                    Taille = m.Taille,
                    Poids = m.Poids,
                    PhotoProfile = m.PhotoProfile,
                    DateInscription = m.DateInscription,
                    NomComplet = $"{m.Prenom} {m.Nom}"
                })
                .ToListAsync();
        }

        // ════════════════════════════════════════════════
        // Supprimer un membre
        // ════════════════════════════════════════════════
        public async Task<bool> SupprimerMembre(int id)
        {
            var membre = await db.Membres.FindAsync(id);
            if (membre == null) return false;

            // Libérer l'ID salle (peut être réutilisé)
            _idsDejaUtilises.Remove(membre.IdSalleSport);

            db.Membres.Remove(membre);
            await db.SaveChangesAsync();
            return true;
        }

        // ════════════════════════════════════════════════
        // Voir tous les IDs disponibles (pour debug/admin)
        // ════════════════════════════════════════════════
        public object GetStatutIds()
        {
            return new
            {
                totalIds = _idsSalleValides.Count,
                idsUtilises = _idsDejaUtilises.Count,
                idsDisponibles = _idsSalleValides.Count - _idsDejaUtilises.Count,
                listeDisponibles = _idsSalleValides.Except(_idsDejaUtilises).ToList()
            };
        }

        private static string GenererMotDePasse()
        {
            var rng = Random.Shared;
            var chiffres = rng.Next(1000, 9999);
            var prefixes = new[] { "Pfa", "Mem", "App", "Spt" };
            return $"{prefixes[rng.Next(prefixes.Length)]}{chiffres}!";
        }

        private static MembreResponseDto MapToDto(Membre m) => new()
        {
            Id = m.Id,
            IdSalleSport = m.IdSalleSport,
            Nom = m.Nom,
            Prenom = m.Prenom,
            Email = m.Email,
            Telephone = m.Telephone,
            Taille = m.Taille,
            Poids = m.Poids,
            PhotoProfile = m.PhotoProfile,
            DateInscription = m.DateInscription,
            NomComplet = $"{m.Prenom} {m.Nom}"
        };
    }

    // ────────────────────────────────────────────────────
    // Classe résultat de la vérification ID
    // ────────────────────────────────────────────────────
    public class VerificationIdResult
    {
        public bool Valide { get; set; }
        public string Message { get; set; } = string.Empty;
        public string IdSalle { get; set; } = string.Empty;
    }
}
