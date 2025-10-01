using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System.Security.Claims;  // <— needed for ClaimTypes
using MediatR;

using PetCare.Infrastructure.Auth;          // ApplicationUser, RoleSeeder
using PetCare.Infrastructure.Jwt;           // JwtOptions, JwtTokenGenerator
using PetCare.Infrastructure.Persistence;   // PetCareDbContext
using PetCare.Infrastructure.Persistence.Repositories;  // PetRepository
using PetCare.Application.Common.Interfaces; // IPetRepository
using PetCare.Infrastructure.Services; // UserService

using FluentValidation;
using FluentValidation.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

// ---------- DB: MySQL ----------
// In production, MySQL connection string comes from environment variable
// In development, it comes from appsettings.Development.json
var connectionString = builder.Configuration.GetConnectionString("MySql")
    ?? Environment.GetEnvironmentVariable("MySql")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:MySql");

var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));
builder.Services.AddDbContext<PetCareDbContext>(opts =>
    opts.UseMySql(connectionString, serverVersion));

// ---------- Identity ----------
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Final password policy enforced later (FluentValidation).
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;

        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<PetCareDbContext>()
    .AddDefaultTokenProviders();

// ---------- CORS ----------
// Environment-specific CORS configuration
if (builder.Environment.IsProduction())
{
    builder.Services.AddCors(o =>
    {
        o.AddPolicy("ui", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
    });
}
else
{
    builder.Services.AddCors(o =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        o.AddPolicy("ui", p => p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials());
    });
}

// ---------- Controllers & Swagger ----------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PetCare.Api", Version = "v1" });

    // Bearer security in Swagger
    var scheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT}"
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new() { { scheme, Array.Empty<string>() } });
});

// ---------- JWT ----------
// Support environment variables for production deployment
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddScoped<PetCare.Infrastructure.Jwt.IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<PetCare.Application.Common.Interfaces.IJwtTokenGenerator>(provider => 
    provider.GetRequiredService<PetCare.Infrastructure.Jwt.IJwtTokenGenerator>());

// ---------- Repositories ----------
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<IMedicalRecordRepository, MedicalRecordRepository>();
builder.Services.AddScoped<IVaccinationRepository, VaccinationRepository>();
builder.Services.AddScoped<ITreatmentRepository, TreatmentRepository>();
builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();

// ---------- Services ----------
builder.Services.AddScoped<IUserService, PetCare.Infrastructure.Services.UserService>();

// ---------- MediatR ----------
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(PetCare.Application.Pets.Commands.CreatePet.CreatePetCommand).Assembly));

// Handlers
builder.Services.AddScoped<PetCare.Application.Users.Profile.UpdateProfileCommand>();
builder.Services.AddScoped<PetCare.Application.Admin.Users.CreateVet.CreateVetCommand>();

// JWT Configuration with environment variable support
var jwt = builder.Configuration.GetSection("Jwt");
var issuer = jwt["Issuer"] 
    ?? Environment.GetEnvironmentVariable("JWT_ISSUER") 
    ?? throw new InvalidOperationException("Jwt:Issuer is missing or empty");
var audience = jwt["Audience"] 
    ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
    ?? throw new InvalidOperationException("Jwt:Audience is missing or empty");
var secret = jwt["Secret"] 
    ?? Environment.GetEnvironmentVariable("JWT_SECRET") 
    ?? throw new InvalidOperationException("Jwt:Secret is missing or empty");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(2),

        // make role checks work
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.NameIdentifier
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<PetCare.Application.Auth.RegisterOwner.RegisterOwnerCommand>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<
    PetCare.Application.Auth.RegisterOwner.RegisterOwnerValidator
>();

builder.Services.AddScoped<PetCare.Application.Auth.Login.LoginQuery>();

var app = builder.Build();

// ---------- One-time role seeding & Database Migration ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PetCareDbContext>();

    // Run migrations automatically in Development and Production
    if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
    {
        db.Database.Migrate();
    }

    // Seed roles, admin user, etc. if needed
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    // await RoleSeeder.SeedAsync(roleManager);
}

// ---------- Pipeline ----------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Serve static files from wwwroot (for frontend in production only)
if (app.Environment.IsProduction())
{
    app.UseStaticFiles();
}

app.UseCors("ui");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Fallback to serve frontend SPA (for client-side routing in production only)
if (app.Environment.IsProduction())
{
    app.MapFallbackToFile("index.html");
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }