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
        public DbSet<MessageContact> MessagesContact { get; set; }
        public DbSet<Avis> Avis { get; set; }
        public DbSet<ReglementInterne> ReglementsInternes { get; set; }
        public DbSet<Abonnement> Abonnements { get; set; }
        public DbSet<Paiement> Paiements { get; set; }




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
            modelBuilder.Entity<ReglementInterne>().HasData(new ReglementInterne
            {
                Id = 1,
                ModifieParId = 1,
                DateModification = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Titre = "Règlement Intérieur — PLF GYM",
                Contenu = @"
🏋️ RÈGLEMENT INTÉRIEUR — PLF GYM

Chez PLF GYM, nous voulons que tout le monde fasse du sport en sécurité dans un cadre agréable.
Tout entrant s'engage à respecter ces règles.
En cas de non-respect ou violation des conditions générales, l'équipe peut vous expulser et résilier votre contrat.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🧼 HYGIÈNE ET PROPRETÉ

- Portez des chaussures de sport propres et vêtements adaptés.
- Utilisez une serviette à l'entraînement.
- Claquettes et ""Crocs"" sont interdits.
- Déposez tous vos effets personnels dans les casiers prévus et libérez-les après votre départ.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🔒 SÉCURITÉ ET USAGE

- Utilisez les appareils à leur usage prévu et rangez-les après.
- Nourriture et récipients non refermables interdits en zone d'entraînement.
- Connaissez l'équipement et vos limites, vous êtes responsable de votre bien-être.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🤝 RESPECT DES AUTRES

- Pas de chutes bruyantes d'équipements, évitez les bruits forts.
- Tolérance zéro : violence verbale, physique...
- Respectez tout interdit : gestes, harcèlement...
- Laissez les appareils libres si non utilisés activement.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

🗄️ RÈGLES GÉNÉRALES ET CASIERS

- Respectez les instructions du personnel.
- Interdit de laisser vos effets personnels.
- Les casiers sont pour la séance seulement. En cas de non-libération, la direction a le droit d'ouvrir les casiers.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

⚖️ DROITS DES CLIENTS

- La salle doit respecter les normes de sécurité et d'hygiène.
- La salle est tenue de couvrir la responsabilité civile de tous les usagers, ce qui protège les clients en cas d'accident lié aux installations ou au personnel.
- Droit de demander le contrat avant signature.
- Droit de faire une séance d'essai.
- La salle met à votre disposition des coachs durant toutes les heures de travail qui pourraient vous guider vers vos objectifs.
- Chaque adhérent a le droit de demander son programme par son Coach.
"
            });




        }
    }
}