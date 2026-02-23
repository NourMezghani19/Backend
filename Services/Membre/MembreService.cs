using backend.Data;
using backend.DTOs;
using backend.DTOs.Membre;
using backend.Models;

namespace backend.Services.MembreServices
{
    public class MembreService
    {
        private readonly AppDbContext db;
        private readonly IWebHostEnvironment env;

        private static readonly string[] _allowedExts =
            { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private const long MaxFileSize = 5 * 1024 * 1024; 

        public MembreService(AppDbContext db, IWebHostEnvironment env)
        {
            this.db = db;
            this.env = env;
        }


        public async Task<MembreProfilDto?> GetProfil(int id)
        {
            var m = await this.db.Membres.FindAsync(id);
            return m == null ? null : MapToDto(m);
        }

        public async Task<MembreProfilDto?> ModifierProfil(int id, UpdateMembreDto dto)
        {
            var m = await this.db.Membres.FindAsync(id);
            if (m == null) return null;

            m.Nom = dto.Nom.Trim();
            m.Prenom = dto.Prenom.Trim();
            m.Telephone = dto.Telephone?.Trim();
            m.Taille = dto.Taille;
            m.Poids = dto.Poids;

            await this.db.SaveChangesAsync();
            return MapToDto(m);
        }

        public async Task<(bool success, string message)> ModifierMotDePasse(int id, ChangePasswordDto dto)
        {
            var m = await db.Membres.FindAsync(id);
            if (m == null) return (false, "Membre non trouvé");

            bool isOldPasswordValid = BCrypt.Net.BCrypt.Verify(dto.AncienMotDePasse, m.MotDePasse);
            if (!isOldPasswordValid)
                return (false, "L'ancien mot de passe est incorrect");

            m.MotDePasse = BCrypt.Net.BCrypt.HashPassword(dto.NouveauMotDePasse);
            await db.SaveChangesAsync();

            return (true, "Mot de passe modifié avec succès");
        }
  
        public async Task<(bool success, string message, string? url)>
            UploadPhotoProfile(int id, IFormFile photo)
        {
            var m = await db.Membres.FindAsync(id);
            if (m == null)
                return (false, $"Membre #{id} non trouvé", null);

            if (photo.Length == 0)
                return (false, "Fichier vide", null);

            if (photo.Length > MaxFileSize)
                return (false, "Fichier trop grand (max 5 MB)", null);

            var ext = Path.GetExtension(photo.FileName).ToLowerInvariant();
            if (!_allowedExts.Contains(ext))
                return (false, $"Extension non autorisée. Autorisées : {string.Join(", ", _allowedExts)}", null);

            if (!string.IsNullOrEmpty(m.PhotoProfile))
            {
                var oldPath = Path.Combine(env.WebRootPath, m.PhotoProfile.TrimStart('/'));
                if (File.Exists(oldPath))
                    File.Delete(oldPath);
            }

            var uploadsDir = Path.Combine(env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var fileName = $"membre_{id}_{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            m.PhotoProfile = $"/uploads/{fileName}";
            await db.SaveChangesAsync();

            return (true, "Photo uploadée avec succès ✓", m.PhotoProfile);
        }

       
        private static string CategoriserIMC(float imc) => imc switch
        {
            < 18.5f => "Insuffisance pondérale",
            < 25f => "Normal",
            < 30f => "Surpoids",
            _ => "Obésité"
        };

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
