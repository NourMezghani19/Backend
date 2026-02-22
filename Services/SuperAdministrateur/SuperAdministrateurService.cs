using backend.Data;
using backend.DTOs.SuperAdministrateur;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.SuperAdminstrateur
{
    public class SuperAdministrateurService
    {
        private readonly AppDbContext db;
        public SuperAdministrateurService(AppDbContext db)
        {
            this.db = db;
        }
        public async Task<AdminReponseDto> CreateCompte(CreateCompteDto dto)
        {
            var existe = await db.Utilisateurs
                .AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (existe)
                throw new InvalidOperationException(
                    $"L'email '{dto.Email}' est déjà utilisé");

            var role = dto.Role.Trim();

            Utilisateur user;

            if (role == "Administrateur")
            {
                user = new Administrateur();
            }
            else if (role == "SuperAdministrateur")
            {
                user = new SuperAdministrateur();
            }
            else
            {
                throw new ArgumentException("Rôle invalide");
            }

            user.Nom = dto.Nom.Trim();
            user.Prenom = dto.Prenom.Trim();
            user.Email = dto.Email.ToLower().Trim();
            user.MotDePasse = BCrypt.Net.BCrypt.HashPassword(dto.MotDePasse);
            user.Telephone = dto.Telephone?.Trim();
            user.Role = role;
            user.DateCreation = DateTime.UtcNow;

            db.Add(user);
            await db.SaveChangesAsync();

            return MapToDto(user);
        }
        private static AdminReponseDto MapToDto(Utilisateur user) => new()
        {
            Id = user.Id,
            Nom = user.Nom,
            Prenom = user.Prenom,
            Email = user.Email,
            Telephone = user.Telephone,
            DateCreation = user.DateCreation,
        
        };
        public async Task<bool> SupprimerAdministrateur(int adminId)
        {
            var admin = await db.Administrateurs.FindAsync(adminId);

            if (admin == null)
                return false; 

            db.Administrateurs.Remove(admin);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<AdminReponseDto?> GetAdminById(int id)
        {
            var admin = await db.Administrateurs.FindAsync(id);
            return admin == null ? null : MapToDto(admin);
        }

      
        public async Task<List<AdminReponseDto>> GetAllAdmins()
        {
            return await db.Administrateurs
                .OrderByDescending(a => a.DateCreation)
                .Select(a => new AdminReponseDto
                {
                    Id = a.Id,
                    Nom = a.Nom,
                    Prenom = a.Prenom,
                    Email = a.Email,
                    Telephone = a.Telephone,
                    DateCreation = a.DateCreation,
                 
                })
                .ToListAsync();
        }
        

        public async Task<AdminReponseDto?> GetSuperAdminById(int id)
        {
            var admin = await db.SuperAdministrateurs.FindAsync(id);
            return admin == null ? null : MapToDto(admin);
        }

        public async Task<List<AdminReponseDto>> GetAllSuperAdmins()
        {
            return await db.SuperAdministrateurs
                .OrderByDescending(a => a.DateCreation)
                .Select(a => new AdminReponseDto
                {
                    Id = a.Id,
                    Nom = a.Nom,
                    Prenom = a.Prenom,
                    Email = a.Email,
                    Telephone = a.Telephone,
                    DateCreation = a.DateCreation,
                    
                })
                .ToListAsync();
        }
    }
}
