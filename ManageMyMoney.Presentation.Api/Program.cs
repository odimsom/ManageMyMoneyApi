using System.Text;
using System.Threading.RateLimiting;
using ManageMyMoney.Core.Application;
using ManageMyMoney.Infrastructure.Persistence;
using ManageMyMoney.Infrastructure.Shared;
using ManageMyMoney.Presentation.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;

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
var swaggerSettings = builder.Configuration.GetSection("SwaggerSettings");
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = swaggerSettings["Title"] ?? "ManageMyMoney API",
        Version = swaggerSettings["Version"] ?? "v1",
        Description = swaggerSettings["Description"] ?? "API para gestión de gastos personales"
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

// JWT Authentication - Usar variables de entorno en Railway
var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured"));
retKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;  options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})engeScheme = JwtBearerDefaults.AuthenticationScheme;
.AddJwtBearer(options =>)
{
    options.TokenValidationParameters = new TokenValidationParameters
    {ameters = new TokenValidationParameters
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],SSUER") ?? builder.Configuration["JwtSettings:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),nt.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["JwtSettings:Audience"],
        ClockSkew = TimeSpan.Zero  IssuerSigningKey = new SymmetricSecurityKey(secretKey),
    };     ClockSkew = TimeSpan.Zero
});    };

builder.Services.AddAuthorization();
.Services.AddAuthorization();
// CORS
var corsSettings = builder.Configuration.GetSection("CorsSettings");
var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();builder.Services.AddCors(options =>

builder.Services.AddCors(options =>   options.AddPolicy("AllowAll", policy =>
{
    options.AddPolicy("DefaultPolicy", policy =>   policy.AllowAnyOrigin()
    {
        if (allowedOrigins.Length > 0)     .AllowAnyHeader();
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();ervices.AddHealthChecks();
        }
        elseervices.AddHttpContextAccessor();
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()ces();
                  .AllowAnyHeader();ervices.AddPersistenceServices(builder.Configuration);
        }.Services.AddSharedInfrastructure(builder.Configuration);
    });
});var app = builder.Build();

// Rate Limiting
var rateLimitSettings = builder.Configuration.GetSection("RateLimitSettings");
if (rateLimitSettings.GetValue<bool>("EnableRateLimiting"))
{uiredService<ManageMyMoney.Infrastructure.Persistence.Context.ManageMyMoneyContext>();
    builder.Services.AddRateLimiter(options =>ry
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                factory: _ => new FixedWindowRateLimiterOptions
                {;
                    PermitLimit = rateLimitSettings.GetValue<int>("PermitLimit"),
                    Window = TimeSpan.FromSeconds(rateLimitSettings.GetValue<int>("WindowSeconds")),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = rateLimitSettings.GetValue<int>("QueueLimit")
                }));// Swagger siempre habilitado

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;SwaggerUI(options =>
    });
}    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ManageMyMoney API v1");
s.RoutePrefix = string.Empty;
// Caching
var cacheSettings = builder.Configuration.GetSection("CacheSettings");
if (cacheSettings.GetValue<bool>("EnableCaching"))/ Health check endpoint
{
    builder.Services.AddDistributedMemoryCache();
}// Middleware
are>();
builder.Services.AddHttpContextAccessor();
;
// Application Services
builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddSharedInfrastructure(builder.Configuration);

var app = builder.Build();

// Swagger (Development only or always if needed)if (app.Environment.IsDevelopment()){    app.UseSwagger();    app.UseSwaggerUI(options =>    {        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ManageMyMoney API v1");        options.RoutePrefix = string.Empty;    });}// Middlewareapp.UseMiddleware<ExceptionHandlingMiddleware>();app.UseCors("DefaultPolicy");if (rateLimitSettings.GetValue<bool>("EnableRateLimiting")){    app.UseRateLimiter();}app.UseAuthentication();app.UseAuthorization();app.MapControllers();app.Run();
