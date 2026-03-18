using backend.Data;
using backend.Models;
using backend.Services;
using backend.Services.Admin;
using backend.Services.MembreServices;
using backend.Services.ReservationService;
using backend.Services.SuperAdminstrateur;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


// ================= CORS =================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});


// ================= DATABASE =================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);


// ================= SERVICES =================
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<SuperAdministrateurService>();
builder.Services.AddScoped<MembreService>();
builder.Services.AddScoped<CoachService>();
builder.Services.AddScoped<CoursService>();
builder.Services.AddScoped<NotificationService>(); 
builder.Services.AddScoped<ReservationService>();   
builder.Services.AddScoped<EmploiDuTempsService>();

// ================= JWT =================
var jwtKey = builder.Configuration["Jwt:Key"]!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });


// ================= AUTHORIZATION =================
builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("SuperAdministrateur",
        p => p.RequireRole("SuperAdministrateur"));

    opt.AddPolicy("Administrateur",
        p => p.RequireRole("Administrateur", "SuperAdministrateur"));

    opt.AddPolicy("Membre",
        p => p.RequireRole("Membre"));
});


// ================= CONTROLLERS =================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


// ================= SWAGGER + JWT =================
builder.Services.AddSwaggerGen(options =>
{
    // Définition JWT
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Gym API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Entrer le token JWT comme: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


// ================= BUILD =================
var app = builder.Build();


// ================= SWAGGER =================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// ================= SEED DATA =================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // === SUPER ADMIN ===
    if (!db.Utilisateurs.Any(u => u.Role == "SuperAdministrateur"))
    {
        db.Utilisateurs.Add(new SuperAdministrateur
        {
            Nom = "Admin",
            Prenom = "Super",
            Email = "superadmin@gmail.com",
            MotDePasse = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = "SuperAdministrateur",
            DateCreation = DateTime.UtcNow
        });
        db.SaveChanges();
        Console.WriteLine("SuperAdmin : superadmin@gmail.com / Admin123!");
    }

    // === MEMBRE DE TEST ===
    if (!db.Membres.Any())
    {
        db.Membres.Add(new Membre
        {
            Nom = "Test",
            Prenom = "Membre",
            Email = "membre@pfa.com",
            MotDePasse = BCrypt.Net.BCrypt.HashPassword("Membre123!"),
            Role = "Membre",
            Telephone = "12345678",
            genre = "Femme",
            IdSalleSport = "SPORT-2026-001",
            Taille = 165,
            Poids = 60,
            DateInscription = DateTime.UtcNow,
            DateCreation = DateTime.UtcNow
        });
        db.SaveChanges();
        Console.WriteLine("Membre de test créé : membre@pfa.com / Membre123!");
    }

    // === COACH DE TEST ===
    if (!db.Coachs.Any())
    {
        db.Coachs.Add(new Coach
        {
            Nom = "Coach",
            Prenom = "Test",
            Specialite = "Fitness",
            Email = "coach@pfa.com",
            Telephone = "87654321",
            Disponible = true,
            DateCreation = DateTime.UtcNow
        });
        db.SaveChanges();
        Console.WriteLine("Coach de test créé : coach@pfa.com / Coach123!");
    }

    // === COURS ===
    if (!db.Cours.Any())
    {
        var cours1 = new Cours
        {
            Nom = "Yoga",
            Description = "Cours de yoga pour tous",
            CapaciteMax = 10,
            Actif = true,
            Genre = GenreCours.Femme // exemple par défaut
        };

        var cours2 = new Cours
        {
            Nom = "Pilates",
            Description = "Cours de Pilates pour renforcer le corps",
            CapaciteMax = 8,
            Actif = true,
            Genre = GenreCours.Mixte
        };

        db.Cours.AddRange(cours1, cours2);
        db.SaveChanges();
        Console.WriteLine("Cours par défaut créés : Yoga, Pilates");
    }

    // === SESSIONS DE COURS ===
    if (!db.Sessions.Any())
    {
        var yoga = db.Cours.First(c => c.Nom == "Yoga");
        var pilates = db.Cours.First(c => c.Nom == "Pilates");
        var coach = db.Coachs.First();

        var session1 = new Session_Cours
        {
            CoursId = yoga.Id,
            CoachId = coach.Id,
            DateHeure = DateTime.UtcNow.AddDays(1).AddHours(9),
            PlacesDisponibles = yoga.CapaciteMax,
            Statut = "Planifié"
        };

        var session2 = new Session_Cours
        {
            CoursId = pilates.Id,
            CoachId = coach.Id,
            DateHeure = DateTime.UtcNow.AddDays(1).AddHours(11),
            PlacesDisponibles = pilates.CapaciteMax,
            Statut = "Planifié"
        };

        db.Sessions.AddRange(session1, session2);
        db.SaveChanges();
        Console.WriteLine("Sessions de cours par défaut créées");
    }

    // === DOSSIER UPLOADS ===
    var uploadsPath = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "uploads");
    if (!Directory.Exists(uploadsPath))
    {
        Directory.CreateDirectory(uploadsPath);
        Console.WriteLine("Dossier uploads créé");
    }
}

// ================= MIDDLEWARE =================
app.UseStaticFiles();

app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();