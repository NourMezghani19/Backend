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

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

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

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

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

    var membre = db.Utilisateurs
     .FirstOrDefault(u => u.Email == "membre@pfa.com");

    if (membre == null)
    {
        db.Utilisateurs.Add(new Membre
        {
            Nom = "Membre",
            Prenom = "Test",
            Email = "membre@pfa.com",
            MotDePasse = BCrypt.Net.BCrypt.HashPassword("Membre123!"),
            Role = "Membre",
            Telephone = "75315984",
            genre = "Homme",
            IdSalleSport = "SPORT-2024-008",
            Taille = 170,
            Poids = 65,
            PhotoProfile = null,
            DateInscription = DateTime.UtcNow,
            DateCreation = DateTime.UtcNow
        });

        db.SaveChanges();
        Console.WriteLine("Membre créé");
    }

    var uploadsPath = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "uploads");
    if (!Directory.Exists(uploadsPath))
    {
        Directory.CreateDirectory(uploadsPath);
        Console.WriteLine("Dossier uploads créé");
    }
}

app.UseStaticFiles();
app.UseCors("AllowAngular");      
app.UseAuthentication();          
app.UseAuthorization();

app.MapControllers();

app.Run();