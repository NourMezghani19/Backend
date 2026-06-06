using backend.Models;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Macs;


namespace backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<SuperAdministrateur> SuperAdministrateurs { get; set; }
        public DbSet<Administrateur> Administrateurs { get; set; }
        public DbSet<Membre> Membres { get; set; }
        public DbSet<Coach> Coachs { get; set; }

        public DbSet<Cours> Cours { get; set; }

        public DbSet<Session_Cours> Sessions { get; set; }

        public DbSet<Reservation> Reservations { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<EmploiDuTemps> EmploisDuTemps { get; set; }
        public DbSet<SalleInformation> SalleInformations { get; set; }

        public DbSet<Salle> Salles { get; set; }
        public DbSet<PlanAbonnement> PlansAbonnement { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Utilisateur>()
                .HasDiscriminator<string>("Role")
                .HasValue<SuperAdministrateur>("SuperAdministrateur")
                .HasValue<Administrateur>("Administrateur")
                .HasValue<Membre>("Membre");


            modelBuilder.Entity<Utilisateur>()
              .HasIndex(u => u.Email)
              .IsUnique()
              .HasDatabaseName("IX_Utilisateurs_Email");

            modelBuilder.Entity<Utilisateur>()
              .HasIndex(u => u.Telephone)
              .IsUnique()
              .HasFilter("[Telephone] IS NOT NULL")  
              .HasFilter("[Telephone] IS NOT NULL")

              .HasDatabaseName("IX_Utilisateurs_Telephone");
            modelBuilder.Entity<SalleInformation>()
                .OwnsMany(s => s.Horaires, h =>
                {
                    h.WithOwner().HasForeignKey("SalleInformationId");
                    h.Property<int>("Id"); // clé technique obligatoire
                    h.HasKey("Id");

                }); 

                

        }
    }
}