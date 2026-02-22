using backend.Models;
using Microsoft.EntityFrameworkCore;


namespace backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Tables dans la base de données
        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<SuperAdministrateur> SuperAdministrateurs { get; set; }
        public DbSet<Administrateur> Administrateurs { get; set; }
        public DbSet<Membre> Membres { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Table Par Hierarchy (TPH) — une seule table "Users"
            modelBuilder.Entity<Utilisateur>()
                .HasDiscriminator<string>("Role")
                .HasValue<SuperAdministrateur>("SuperAdministrateur")
                .HasValue<Administrateur>("Administrateur")
                .HasValue<Membre>("Membre");

         
            // Email unique
            modelBuilder.Entity<Utilisateur>()
              .HasIndex(u => u.Email)
              .IsUnique()
              .HasDatabaseName("IX_Utilisateurs_Email");

            // Téléphone unique (ignore les nulls — plusieurs peuvent avoir null)
            modelBuilder.Entity<Utilisateur>()
              .HasIndex(u => u.Telephone)
              .IsUnique()
              .HasFilter("[Telephone] IS NOT NULL")   // ← null n'est pas considéré doublon
              .HasDatabaseName("IX_Utilisateurs_Telephone"); ;
        }
    }
}