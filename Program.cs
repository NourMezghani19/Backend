using backend.Data;
using backend.Models;
using backend.Services;
using backend.Services.Admin;
using backend.Services.Coach;
using backend.Services.MembreServices;
using backend.Services.ReservationService;
using backend.Services.SuperAdminstrateur;
using backend.Services.EmploiDuTemps;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using backend.Hubs;
using backend.Services.SalleInformation; // 1. Assure-toi d'ajouter ce namespace pour ton Hub

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
              .AllowCredentials(); // Obligatoire pour SignalR
    });
});

// ================= DATABASE =================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

// ================= SIGNALR =================
builder.Services.AddSignalR(); // 2. Ajout du service SignalR

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
builder.Services.AddScoped<SalleInformationService>();

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };

        // 3. CONFIGURATION CRUCIALE POUR SIGNALR + JWT
        opt.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                // Si la requête va vers notre Hub, on lit le token dans la query string
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// ... (Le reste de tes politiques d'autorisation et controllers reste inchangé) ...

// ================= AUTHORIZATION =================
builder.Services.AddAuthorization(opt => { /* ... ton code ... */ });

// ================= CONTROLLERS =================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ================= SWAGGER + JWT =================
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "API PFA", Version = "v1" });

    // Configuration JWT
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Entrez 'Bearer {token}'"
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
            new string[]{ }
        }
    });
});

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

// 4. MAPPER LE HUB SIGNALR
app.MapHub<NotificationHub>("/notificationHub"); // Route utilisée par Angular

app.Run();