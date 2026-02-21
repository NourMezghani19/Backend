using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using backend.Controllers;

namespace backend.Services
{
    public class AuthService
    {
        private readonly AppDbContext db;
        private readonly IConfiguration config;

        public AuthService(AppDbContext db, IConfiguration config)
        {
            this.db = db;
            this.config = config;
        }

        // ════════════════════════════════════════════════
        // seConnecter() — Vérifier identifiants → JWT
        // ════════════════════════════════════════════════
        public async Task<AuthResponseDto?> SeConnecter(string email, string motDePasse)
        {
            // 1. Chercher l'utilisateur par email (insensible à la casse)
            var user = await db.Utilisateurs
           .FirstOrDefaultAsync(u =>
         u.Email.ToLower() == email.Trim().ToLower());

            if (user == null)
                return null; // utilisateur non trouvé

            // 2. Vérifier le mot de passe avec BCrypt
            var motDePasseValide = BCrypt.Net.BCrypt.Verify(motDePasse, user.MotDePasse);
            if (!motDePasseValide)
                return null; // mot de passe incorrect

            // 3. Générer le token JWT
            var expiration = DateTime.UtcNow.AddHours(
                int.Parse(config["Jwt:ExpiresInHours"] ?? "8"));

            var token = GenererToken(user, expiration);

            return new AuthResponseDto
            {
                Token = token,
                Role = user.Role,
                UserId = user.Id,
                Nom = user.Nom,
                Prenom = user.Prenom,
                Email = user.Email,
                Expiration = expiration,
                Message = $"Bienvenue {user.Prenom} !"
            };
        }

        // ════════════════════════════════════════════════
        // Génération du token JWT
        // ════════════════════════════════════════════════
       

        // ════════════════════════════════════════════════
        // Changer le mot de passe
        // ════════════════════════════════════════════════
        public async Task<bool> ChangerMotDePasse(
            int userId, string ancienMdp, string nouveauMdp)
        {
            var user = await db.Utilisateurs.FindAsync(userId);
            if (user == null) return false;

            if (!BCrypt.Net.BCrypt.Verify(ancienMdp, user.MotDePasse))
                return false; // ancien mot de passe incorrect

            user.MotDePasse = BCrypt.Net.BCrypt.HashPassword(nouveauMdp);
            await db.SaveChangesAsync();
            return true;
        }
        public string GenererToken(Utilisateur user, DateTime expiration)
        {
            // Claims = informations encodées dans le token
            var claims = new[]
            {
                 new Claim("id",              user.Id.ToString()),
                 new Claim("email",           user.Email),
                 new Claim("nom",             user.Nom),
                 new Claim("prenom",          user.Prenom),
                 new Claim(ClaimTypes.Role,    user.Role),
                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                 new Claim(JwtRegisteredClaimNames.Iat,
                 new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString())
            };

            // Clé secrète (depuis appsettings.json)
            var keyBytes = Encoding.UTF8.GetBytes(config["Jwt:Key"]!);
            var key = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Créer le token
            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiration,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
