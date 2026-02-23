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


// ?? 1. Déclarer la politique CORS ??????????????????????
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200") // URL Angular
            .AllowAnyMethod()   // GET, POST, PUT, DELETE
            .AllowAnyHeader()   // Content-Type, Authorization...
            .AllowCredentials(); // Si vous utilisez des cookies
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
    opt.AddPolicy("SuperAdministrateur", p => p.RequireRole("SuperAdministrateur"));
    opt.AddPolicy("Administrateur", p => p.RequireRole("Administrateur", "SuperAdministrateur"));
    opt.AddPolicy("Membre", p => p.RequireRole("Membre"));
});

builder.Services.AddCors(opt => opt.AddPolicy("PFA_CORS", p =>
    p.WithOrigins("http://localhost:4200", "http://localhost:3000")
     .AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PFA API", Version = "v1" });
    
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
        Console.WriteLine("SuperAdmin : superadmin@pfa.com / Admin123!");
    }
    
    var uploadsPath = Path.Combine(
        app.Environment.WebRootPath ?? "wwwroot", "uploads");
    if (!Directory.Exists(uploadsPath))
    {
        Directory.CreateDirectory(uploadsPath);
        Console.WriteLine("Dossier uploads créé");
    }
}


app.UseCors("AllowAngular");
//app.UseSwagger();
//app.UseSwaggerUI();
app.UseCors("PFA_CORS");
app.UseAuthentication();  
app.UseAuthorization();
app.MapControllers();
app.Run();
