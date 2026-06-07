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
        public DbSet<Exercice> Exercices { get; set; }
        public DbSet<Programme> Programmes { get; set; }
        public DbSet<ProgrammeExercice> ProgrammeExercices { get; set; }

        public DbSet<HistoriqueEntry> Historiques { get; set; }
        public DbSet<ConversationSession> ConversationSessions { get; set; }
        public DbSet<ConversationMessage> ConversationMessages { get; set; }
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
              .HasDatabaseName("IX_Utilisateurs_Telephone");
            modelBuilder.Entity<SalleInformation>()
                .OwnsMany(s => s.Horaires, h =>
                {
                    h.WithOwner().HasForeignKey("SalleInformationId");
                    h.Property<int>("Id"); // clé technique obligatoire
                    h.HasKey("Id");
                });
            modelBuilder.Entity<Exercice>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.Nom).IsRequired().HasMaxLength(100);
                e.Property(x => x.Mode).HasConversion<string>(); // stocke l'enum en string

                e.HasOne(x => x.Membre)
                 .WithMany()               // ou .WithMany(u => u.Exercices) si navigation inversée
                 .HasForeignKey(x => x.MembreId)
                 .OnDelete(DeleteBehavior.Cascade);

                // Index pour accélérer les requêtes par membre
                e.HasIndex(x => x.MembreId);
                e.HasIndex(x => x.ExpiresAt); // pour la purge automatique
            });
            modelBuilder.Entity<Programme>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Nom).IsRequired().HasMaxLength(150);
                e.HasOne(x => x.Membre)
                 .WithMany()
                 .HasForeignKey(x => x.MembreId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(x => x.MembreId);
                e.HasIndex(x => x.ExpiresAt);
            });

            modelBuilder.Entity<ProgrammeExercice>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasOne(x => x.Programme)
                 .WithMany(p => p.ProgrammeExercices)
                 .HasForeignKey(x => x.ProgrammeId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(x => x.Exercice)
                 .WithMany()
                 .HasForeignKey(x => x.ExerciceId)
                 .OnDelete(DeleteBehavior.Restrict); // exercice géré manuellement
            });
            modelBuilder.Entity<ConversationSession>(e =>
             {
                e.HasKey(s => s.Id);
                e.HasOne(s => s.Membre)
                 .WithMany()
                .HasForeignKey(s => s.MembreId)
                .OnDelete(DeleteBehavior.Cascade);
               e.HasMany(s => s.Messages)
                  .WithOne(m => m.Session)
                  .HasForeignKey(m => m.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);
             });

             modelBuilder.Entity<ConversationMessage>(e =>
             {
                 e.HasKey(m => m.Id);
                 e.Property(m => m.Role).HasMaxLength(20);
                 e.Property(m => m.Content).HasColumnType("TEXT");
             });
        }
    }
}