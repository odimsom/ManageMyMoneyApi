using System.Text;
using ManageMyMoney.Core.Application;
using ManageMyMoney.Infrastructure.Persistence;
using ManageMyMoney.Infrastructure.Persistence.Context;
using ManageMyMoney.Infrastructure.Persistence.Seeds;
using ManageMyMoney.Infrastructure.Shared;
using ManageMyMoney.Presentation.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ================================
// Railway PORT
// ================================
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// ================================
// Controllers & JSON
// ================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();

// ================================
// Swagger (SIEMPRE ACTIVO)
// ================================
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ManageMyMoney API",
        Version = "v1",
        Description = "API para gestión de gastos personales"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token JWT"
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
            Array.Empty<string>()
        }
    });
});

// ================================
// JWT
// ================================
var jwtSecretKey =
    Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ??
    builder.Configuration["JwtSettings:SecretKey"] ??
    "DefaultDevSecretKeyThatIsAtLeast32CharactersLong!";

var jwtIssuer =
    Environment.GetEnvironmentVariable("JWT_ISSUER") ??
    builder.Configuration["JwtSettings:Issuer"] ??
    "ManageMyMoney";

var jwtAudience =
    Environment.GetEnvironmentVariable("JWT_AUDIENCE") ??
    builder.Configuration["JwtSettings:Audience"] ??
    "ManageMyMoneyUsers";

var secretKey = Encoding.UTF8.GetBytes(jwtSecretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ================================
// CORS (temporal, no ideal)
// ================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ================================
// Health checks
// ================================
var connectionString = builder.Configuration.GetConnectionString("ManageMyMoneyConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'ManageMyMoneyConnection' not found.");
}

builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString,
        name: "postgres"
    );

builder.Services.AddHttpContextAccessor();

// ================================
// Application / Infrastructure
// ================================
builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddSharedInfrastructure(builder.Configuration);

// ================================
// Build
// ================================
var app = builder.Build();

// ================================
// Controlled DB init (SAFE)
// ================================
var runMigrations =
    Environment.GetEnvironmentVariable("RUN_DB_MIGRATIONS") == "true";

if (runMigrations)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ManageMyMoneyContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Running database migrations...");
        context.Database.Migrate();

        logger.LogInformation("Running currency seed...");
        await CurrencySeed.SeedAsync(context);

        logger.LogInformation("Database initialization completed");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Database initialization failed");
        throw;
    }
}

// ================================
// Middleware pipeline
// ================================
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ManageMyMoney API v1");
    options.RoutePrefix = "swagger";
});

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// ================================
// Endpoints
// ================================
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
