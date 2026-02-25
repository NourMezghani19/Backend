using backend.Data;
using backend.Models;
using backend.Services;
using backend.Services.Admin;
using backend.Services.MembreServices;
using backend.Services.SuperAdminstrateur;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// ── 1. Déclarer la politique CORS ──────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration
        .GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<SuperAdministrateurService>();
builder.Services.AddScoped<MembreService>();

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
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("SuperAdministrateur",
        p => p.RequireRole("SuperAdministrateur"));

    opt.AddPolicy("Administrateur",
        p => p.RequireRole("Administrateur", "SuperAdministrateur"));

    opt.AddPolicy("Membre",
        p => p.RequireRole("Membre"));
});

builder.Services.AddCors(opt => opt.AddPolicy("PFA_CORS", p =>
    p.WithOrigins("http://localhost:4200", "http://localhost:3000")
     .AllowAnyHeader()
     .AllowAnyMethod()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// ─────────────────────────────────────────────
// SEED DATABASE
// ─────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // 👑 Super Administrateur
    if (!db.Utilisateurs.Any(u => u.Role == "SuperAdministrateur"))
    {
        db.Utilisateurs.Add(new SuperAdministrateur
        {
            Nom = "Admin",
            Prenom = "Super",
            Email = "superadmin@pfa.com",
            MotDePasse = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = "SuperAdministrateur",
            DateCreation = DateTime.UtcNow
        });

        db.SaveChanges();
        Console.WriteLine("SuperAdmin : superadmin@pfa.com / Admin123!");
    }

    // 👤 Membre
    if (!db.Utilisateurs.Any(u => u.Role == "Membre"))
    {
        db.Utilisateurs.Add(new Membre
        {
            Nom = "Membre",
            Prenom = "Test",
            Email = "membre@pfa.com",
            MotDePasse = BCrypt.Net.BCrypt.HashPassword("Membre123!"),
            Role = "Membre",
            Telephone = "12345678",
            Taille = 170,
            Poids = 65,
            PhotoProfile = null,
            DateInscription = DateTime.UtcNow
        });

        db.SaveChanges();
        Console.WriteLine("Membre : membre@pfa.com / Membre123!");
    }

    // 📁 uploads
    var uploadsPath = Path.Combine(
        app.Environment.WebRootPath ?? "wwwroot",
        "uploads");

    if (!Directory.Exists(uploadsPath))
    {
        Directory.CreateDirectory(uploadsPath);
        Console.WriteLine("Dossier uploads créé");
    }
}


// ─────────────────────────────────────────────
// MIDDLEWARE
// ─────────────────────────────────────────────
app.UseStaticFiles();
app.UseCors("AllowAngular");

app.UseCors("PFA_CORS");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();