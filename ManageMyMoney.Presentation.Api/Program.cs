using System.Text;
using ManageMyMoney.Core.Application;
using ManageMyMoney.Core.Domain.Entities.System;
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

// Railway usa PORT como variable de entorno
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();

// Swagger
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

// JWT Authentication
var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? builder.Configuration["JwtSettings:SecretKey"]
    ?? "DefaultDevSecretKeyThatIsAtLeast32CharactersLong!";

var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
    ?? builder.Configuration["JwtSettings:Issuer"]
    ?? "ManageMyMoney";

var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
    ?? builder.Configuration["JwtSettings:Audience"]
    ?? "ManageMyMoneyUsers";

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

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Health checks
builder.Services.AddHealthChecks();

builder.Services.AddHttpContextAccessor();

// Application Services
builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddSharedInfrastructure(builder.Configuration);

var app = builder.Build();

// Aplicar migraciones automáticamente
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ManageMyMoneyContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Applying database migrations...");
        context.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully");
        
        logger.LogInformation("Seeding currencies...");
        await CurrencySeed.SeedAsync(context);
        logger.LogInformation("Currency seeding completed");
        
        // Validate email configuration
        logger.LogInformation("=== Email Configuration Status ===");
        var smtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER");
        var senderEmail = Environment.GetEnvironmentVariable("SENDER_EMAIL");
        var emailUsername = Environment.GetEnvironmentVariable("EMAIL_USERNAME");
        var emailPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
        
        if (!string.IsNullOrWhiteSpace(smtpServer) &&
            !string.IsNullOrWhiteSpace(senderEmail) &&
            !string.IsNullOrWhiteSpace(emailUsername) &&
            !string.IsNullOrWhiteSpace(emailPassword))
        {
            logger.LogInformation("✅ Email service: CONFIGURED - SMTP: {Server}, From: {Sender}", smtpServer, senderEmail);
        }
        else
        {
            logger.LogWarning("⚠️  Email service: NOT CONFIGURED - Emails will not be sent");
            logger.LogWarning("   Missing variables: {Missing}",
                string.Join(", ", new[]
                {
                    string.IsNullOrWhiteSpace(smtpServer) ? "SMTP_SERVER" : null,
                    string.IsNullOrWhiteSpace(senderEmail) ? "SENDER_EMAIL" : null,
                    string.IsNullOrWhiteSpace(emailUsername) ? "EMAIL_USERNAME" : null,
                    string.IsNullOrWhiteSpace(emailPassword) ? "EMAIL_PASSWORD" : null
                }.Where(x => x != null)));
            logger.LogInformation("   See EMAIL_CONFIGURATION.md for setup instructions");
        }
        logger.LogInformation("==================================");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error applying migrations");
    }
}

// Swagger siempre habilitado
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ManageMyMoney API v1");
    options.RoutePrefix = string.Empty;
});

// Health check endpoint
app.MapHealthChecks("/health");

// Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
