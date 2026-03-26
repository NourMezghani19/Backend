using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using backend.Controllers;
namespace backend.Services { 
    
    public class AuthService { 
        private readonly AppDbContext db; 
        private readonly IConfiguration config;
        public AuthService(AppDbContext db, IConfiguration config)
        { 
            this.db = db; this.config = config;
        } 
        public async Task<AuthResponseDto?> SeConnecter(string email, string password) { 
            var user = await db.Utilisateurs
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.Trim().ToLower());
            if (user == null) 
                return null;
            var motDePasseValide = BCrypt.Net.BCrypt.Verify(password, user.MotDePasse);
            if (!motDePasseValide) 
                return null;
            var expiration = DateTime.UtcNow.AddHours(int.Parse(config["Jwt:ExpiresInHours"] ?? "8"));
            var token = GenererToken(user, expiration); 
            return new AuthResponseDto { 
                Token = token,
                Role = user.Role,
                UserId = user.Id,
                Nom = user.Nom,
                Prenom = user.Prenom,
                Email = user.Email,
                Expiration = expiration,
                Message = $"Bienvenue {user.Prenom} !"
            };
            Console.WriteLine("Email trouvé: " + user?.Email);
            Console.WriteLine("Mot de passe hash DB: " + user?.MotDePasse);
            Console.WriteLine("Password saisi: " + password);
        } 
        public async Task<bool> ChangerMotDePasse(int userId, string ancienMdp, string nouveauMdp) { 
            var user = await db.Utilisateurs.FindAsync(userId);
            if (user == null) return false; 
            if (!BCrypt.Net.BCrypt.Verify(ancienMdp, user.MotDePasse)) 
                return false; 
            user.MotDePasse = BCrypt.Net.BCrypt.HashPassword(nouveauMdp); 
            await db.SaveChangesAsync();
            return true;
        }
        public string GenererToken(Utilisateur user, DateTime expiration)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim("email", user.Email),
        new Claim("nom", user.Nom),
        new Claim("prenom", user.Prenom),
        new Claim(ClaimTypes.Role, user.Role),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Iat,
            new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString())
    };

            // ✅ Ajouter le genre si c'est un Membre
            if (user is Membre membre)
            {
                claims.Add(new Claim("genre", membre.genre ?? ""));
            }

            var keyBytes = Encoding.UTF8.GetBytes(config["Jwt:Key"]!);
            var key = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiration,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    } }