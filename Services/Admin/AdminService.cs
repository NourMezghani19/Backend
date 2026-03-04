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

        private static readonly HashSet<string> _idsSalleValides = new()
        {
            "SPORT-2024-001", "SPORT-2024-002", "SPORT-2024-003",
            "SPORT-2024-004", "SPORT-2024-005", "SPORT-2024-006",
            "SPORT-2024-007", "SPORT-2024-008", "SPORT-2024-009",
            "SPORT-2024-010"
        };

        public AdminService(AppDbContext db, EmailService email)
        {
            this.db = db;
            this.email = email;
        }

        public VerificationIdResult VerifierIdSalle(string idSalle)
        {
            idSalle = idSalle.Trim().ToUpper();

            if (!_idsSalleValides.Contains(idSalle))
                return new VerificationIdResult
                {
                    Valide = false,
                    Message = "ID non reconnu",
                    IdSalle = idSalle
                };

            var dejaEnBase = db.Membres.Any(m => m.IdSalleSport == idSalle);
            if (dejaEnBase)
                return new VerificationIdResult
                {
                    Valide = false,
                    Message = "ID déjà utilisé",
                    IdSalle = idSalle
                };

            return new VerificationIdResult
            {
                Valide = true,
                Message = "ID valide ✓",
                IdSalle = idSalle
            };
        }

        public async Task<MembreResponseDto> CreerCompteMembre(CreateMembreDto dto)
        {
            var verification = VerifierIdSalle(dto.IdSalleSport);
            if (!verification.Valide)
                throw new InvalidOperationException(verification.Message);

            var existe = await db.Utilisateurs
                .AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower().Trim());
            if (existe)
                throw new InvalidOperationException(
                    $"Un compte avec l'email '{dto.Email}' existe déjà");

            var motDePasseTemp = GenererMotDePasse();

            var membre = new Membre
            {
                IdSalleSport = dto.IdSalleSport.Trim().ToUpper(),
                Nom = dto.Nom.Trim(),
                Prenom = dto.Prenom.Trim(),
                genre = dto.genre?.Trim().ToLower(),
                Email = dto.Email.ToLower().Trim(),
                MotDePasse = BCrypt.Net.BCrypt.HashPassword(motDePasseTemp),
                Role = "Membre",
                Telephone = dto.Telephone?.Trim(),  
                Taille = dto.Taille,             
                Poids = dto.Poids,           
                DateCreation = DateTime.UtcNow,
                DateInscription = DateTime.UtcNow
            };

            db.Membres.Add(membre);
            await db.SaveChangesAsync();

            await email.EnvoyerEmailInscription(
                membre.Email,
                $"{membre.Prenom} {membre.Nom}",
                motDePasseTemp);  

            return MapToDto(membre);
        }

        public async Task<List<MembreResponseDto>> GetAllMembres(string? search = null)
        {
            var query = db.Membres.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower().Trim();
                query = query.Where(m =>
                    m.Nom.ToLower().Contains(s) ||
                    m.Prenom.ToLower().Contains(s) ||
                    m.Email.ToLower().Contains(s) ||
                    m.IdSalleSport.ToLower().Contains(s));
            }

            return await query
                .OrderByDescending(m => m.DateInscription)
                .Select(m => new MembreResponseDto
                {
                    Id = m.Id,
                    IdSalleSport = m.IdSalleSport,
                    Nom = m.Nom,
                    Prenom = m.Prenom,
                    genre = m.genre,
                    Email = m.Email,
                    Telephone = m.Telephone,
                    Taille = m.Taille,
                    Poids = m.Poids,
                    PhotoProfile = m.PhotoProfile,
                    DateInscription = m.DateInscription
                })
                .ToListAsync();
        }

        public async Task<bool> SupprimerMembre(int id)
        {
            var membre = await db.Membres.FindAsync(id);
            if (membre == null) return false;

            db.Membres.Remove(membre);
            await db.SaveChangesAsync();
            return true;
        }

        public object GetStatutIds()
        {
            var idsUtilisesEnBase = db.Membres
                .Select(m => m.IdSalleSport)
                .ToHashSet();

            var idsDisponibles = _idsSalleValides
                .Except(idsUtilisesEnBase)
                .OrderBy(x => x)
                .ToList();

            return new
            {
                totalIds = _idsSalleValides.Count,
                idsUtilises = idsUtilisesEnBase.Count,
                idsDisponibles = idsDisponibles.Count,
                listeDisponibles = idsDisponibles
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
            genre = m.genre,
            Email = m.Email,
            Telephone = m.Telephone,
            Taille = m.Taille,
            Poids = m.Poids,
            PhotoProfile = m.PhotoProfile,
            DateInscription = m.DateInscription
        };
    }

    public class VerificationIdResult
    {
        public bool Valide { get; set; }
        public string Message { get; set; } = string.Empty;
        public string IdSalle { get; set; } = string.Empty;
    }
}
