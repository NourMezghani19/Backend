using backend.Data;
using backend.Models;
using backend.Services;
using backend.Services.Admin;
using backend.Services.Coach;
using backend.Services.MembreServices;
using backend.Services.Reservation;
using backend.Services.SuperAdminstrateur;
using backend.Services.EmploiDuTemps;
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
builder.Services.AddScoped<ReservationService>();
builder.Services.AddScoped<NotificationService>();
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

    /* var membre = db.Utilisateurs
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
    }*/

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