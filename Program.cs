using backend.Data;
using backend.Models;
using backend.Services;
using backend.Services.MembreServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// ?? 1. Base de donn�es Entity Framework Core ??
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration
        .GetConnectionString("DefaultConnection")));

// ?? 2. Services m�tier (injection de d�pendance) ??
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<MembreService>();
// ?? 3. JWT Authentication ??
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
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
    // Swagger avec support JWT
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
        db.Utilisateurs.Add(new Membre
        {
            Nom = "Ben Ali",
            Prenom = "Ahmed",
            Email = "ahmed.benali@gmail.com",
            MotDePasse = BCrypt.Net.BCrypt.HashPassword("Test1234!"),
            Telephone = "22334455",
            Taille = 175f,
            Poids = 70f,
            Role = "Membre",
            PhotoProfile = "gggg",

            DateInscription= DateTime.UtcNow,
        }

        );
        await db.SaveChangesAsync();
    }
}
/*using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!context.Utilisateurs.Any(u => u.Role == "SuperAdministrateur"))
    {
        context.Utilisateurs.Add(new SuperAdministrateur
        {
            Nom = "administrateur",
            Prenom = "Super",
            Email = "superadmin@pfa.com",
            MotDePasse = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = "SuperAdministrateur",
            DateCreation = DateTime.UtcNow
        });

        context.SaveChanges();
    }
}*/
// ?? Middleware pipeline ??
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("PFA_CORS");
app.UseAuthentication();  // JWT
app.UseAuthorization();
app.MapControllers();
app.Run();
