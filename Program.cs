using backend.Data;
using backend.Models;
using backend.Services;
<<<<<<< HEAD
using backend.Services.Admin;
using backend.Services.MembreServices;
=======
using backend.Services.SuperAdminstrateur;
>>>>>>> aa16d282b976b3099d7ca1477c2594c3b95700f0
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// ?? 1. Base de donn�es Entity Framework Core ??
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration
        .GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<AuthService>();
<<<<<<< HEAD
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<EmailService>();
=======
builder.Services.AddScoped<SuperAdministrateurService>();

>>>>>>> aa16d282b976b3099d7ca1477c2594c3b95700f0


builder.Services.AddScoped<MembreService>();
// ?? 3. JWT Authentication ??
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

// ?? 4. Autorisation par r�le ??
builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("SuperAdministrateur", p => p.RequireRole("SuperAdministrateur"));
    opt.AddPolicy("Administrateur", p => p.RequireRole("Administrateur", "SuperAdministrateur"));
    opt.AddPolicy("Membre", p => p.RequireRole("Membre"));
});

// ?? 5. CORS pour Angular (port 4200) et Flutter Web ??
builder.Services.AddCors(opt => opt.AddPolicy("PFA_CORS", p =>
    p.WithOrigins("http://localhost:4200", "http://localhost:3000")
     .AllowAnyHeader().AllowAnyMethod()));

// ?? 6. Controllers + Swagger ??
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PFA API", Version = "v1" });
    // Swagger avec support
    // JWT
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    /*
    // ── SuperAdmin ──
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
    // ── Membre Prédéfini pour Test ──
    if (!db.Utilisateurs.Any(u => u.Email == "membre@pfa.com"))
    {
        db.Membres.Add(new Membre
        {
            Nom = "Dupont",
            Prenom = "Jean",
            Email = "membre@pfa.com",
            MotDePasse = BCrypt.Net.BCrypt.HashPassword("Membre123!"),
            Telephone = "11111111",
            Role = "Membre",
            IdSalleSport = "SALLE-001",
            Taille = 180,
            Poids = 75,
            DateInscription = DateTime.UtcNow,
            DateCreation = DateTime.UtcNow
        });
        db.SaveChanges();
        Console.WriteLine("Membre Test : membre@pfa.com / Membre123!");
    }
    // ── Administrateur ──
    if (!db.Utilisateurs.Any(u => u.Role == "Administrateur"))
    {
        db.Utilisateurs.Add(new Administrateur
        {
            Nom = "Benali",
            Prenom = "Mohamed",
            Email = "admin@pfa.com",
            MotDePasse = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = "Administrateur",
            DateCreation = DateTime.UtcNow
        });
        db.SaveChanges();
        Console.WriteLine("Admin : admin@pfa.com / Admin123!");
    }

   */
    
    // ── Dossier uploads pour Emna ──
    var uploadsPath = Path.Combine(
        app.Environment.WebRootPath ?? "wwwroot", "uploads");
    if (!Directory.Exists(uploadsPath))
    {
        Directory.CreateDirectory(uploadsPath);
        Console.WriteLine("Dossier uploads créé");
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("PFA_CORS");
app.UseAuthentication();  // JWT
app.UseAuthorization();
app.MapControllers();
app.Run();
