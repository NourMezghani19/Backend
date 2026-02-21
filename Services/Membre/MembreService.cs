using backend.Data;
using backend.DTOs;
using backend.DTOs.Membre;
using backend.Models;

namespace backend.Services.MembreServices
{
    public class MembreService
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        // Extensions autorisées pour la photo
        private static readonly string[] _allowedExts =
            { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB max

        public MembreService(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // ════════════════════════════════════════════════
        // GetProfil() — Voir le profil d'un membre
        // ════════════════════════════════════════════════
        public async Task<MembreProfilDto?> GetProfil(int id)
        {
            var m = await _db.Membres.FindAsync(id);
            return m == null ? null : MapToDto(m);
        }

        // ════════════════════════════════════════════════
        // ModifierProfil() → modifierProfil() du diagramme UML
        // Modifie : nom, prenom, telephone, taille, poids
        // ════════════════════════════════════════════════
        public async Task<MembreProfilDto?> ModifierProfil(int id, UpdateMembreDto dto)
        {
            var m = await _db.Membres.FindAsync(id);
            if (m == null) return null;

            m.Nom = dto.Nom.Trim();
            m.Prenom = dto.Prenom.Trim();
            m.Telephone = dto.Telephone?.Trim();
            m.Taille = dto.Taille;
            m.Poids = dto.Poids;

            await _db.SaveChangesAsync();
            return MapToDto(m);
        }

        // ════════════════════════════════════════════════
        // UploadPhotoProfile() → upload de la photo de profil
        // Sauvegarde dans wwwroot/uploads/ + met à jour PhotoProfile en DB
        // ════════════════════════════════════════════════
        public async Task<(bool success, string message, string? url)>
            UploadPhotoProfile(int id, IFormFile photo)
        {
            // 1. Trouver le membre
            var m = await _db.Membres.FindAsync(id);
            if (m == null)
                return (false, $"Membre #{id} non trouvé", null);

            // 2. Vérifier la taille du fichier
            if (photo.Length == 0)
                return (false, "Fichier vide", null);

            if (photo.Length > MaxFileSize)
                return (false, "Fichier trop grand (max 5 MB)", null);

            // 3. Vérifier l'extension
            var ext = Path.GetExtension(photo.FileName).ToLowerInvariant();
            if (!_allowedExts.Contains(ext))
                return (false, $"Extension non autorisée. Autorisées : {string.Join(", ", _allowedExts)}", null);

            // 4. Supprimer l'ancienne photo (si elle existe)
            if (!string.IsNullOrEmpty(m.PhotoProfile))
            {
                var oldPath = Path.Combine(_env.WebRootPath, m.PhotoProfile.TrimStart('/'));
                if (File.Exists(oldPath))
                    File.Delete(oldPath);
            }

            // 5. Créer le dossier uploads s'il n'existe pas
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            // 6. Nom de fichier unique : membre_42_guid.jpg
            var fileName = $"membre_{id}_{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            // 7. Sauvegarder le fichier
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            // 8. Mettre à jour photoProfile en DB
            m.PhotoProfile = $"/uploads/{fileName}";
            await _db.SaveChangesAsync();

            return (true, "Photo uploadée avec succès ✓", m.PhotoProfile);
        }

        // ════════════════════════════════════════════════
        // Calculer la catégorie IMC
        // ════════════════════════════════════════════════
        private static string CategoriserIMC(float imc) => imc switch
        {
            < 18.5f => "Insuffisance pondérale",
            < 25f => "Normal",
            < 30f => "Surpoids",
            _ => "Obésité"
        };

        // Helper : Membre → MembreProfilDto
        private static MembreProfilDto MapToDto(Membre m)
        {
            var imc = m.Taille > 0
                ? m.Poids / MathF.Pow(m.Taille / 100f, 2)
                : 0f;

            return new MembreProfilDto
            {
                Id = m.Id,
                Nom = m.Nom,
                Prenom = m.Prenom,
                NomComplet = $"{m.Prenom} {m.Nom}",
                Email = m.Email,
                Telephone = m.Telephone,
                Taille = m.Taille,
                Poids = m.Poids,
                PhotoProfile = m.PhotoProfile,
                DateInscription = m.DateInscription,
                IMC = MathF.Round(imc, 2),
                CategorieIMC = CategoriserIMC(imc)
            };
        }
    }
}
