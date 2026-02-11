using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ManageMyMoney.Core.Application;
using ManageMyMoney.Infrastructure.Persistence;
using ManageMyMoney.Infrastructure.Persistence.Context;
using ManageMyMoney.Infrastructure.Persistence.Seeds;
using ManageMyMoney.Infrastructure.Persistence.Services;
using ManageMyMoney.Infrastructure.Shared;
using ManageMyMoney.Presentation.Api.Middleware;
using Microsoft.Extensions.FileProviders;

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ManageMyMoney API", Version = "v1" });

    // JWT Bearer authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// Health checks
builder.Services.AddHealthChecks();

// JWT Configuration
var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? builder.Configuration["JwtSettings:SecretKey"]
    ?? throw new ArgumentNullException("JWT_SECRET_KEY", "JWT Secret Key is required");

var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
    ?? builder.Configuration["JwtSettings:Issuer"]
    ?? throw new ArgumentNullException("JWT_ISSUER", "JWT Issuer is required");

var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
    ?? builder.Configuration["JwtSettings:Audience"]
    ?? throw new ArgumentNullException("JWT_AUDIENCE", "JWT Audience is required");

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Configure forward headers for Railway
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Database configuration
string connectionString = "";

if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT")))
{
    // Railway environment
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrWhiteSpace(databaseUrl))
    {
        var uri = new Uri(databaseUrl);
        var host = uri.Host;
        var port_db = uri.Port;
        var database = uri.AbsolutePath.TrimStart('/');
        var username = uri.UserInfo.Split(':')[0];
        var password = uri.UserInfo.Split(':')[1];

        connectionString = $"Host={host};Port={port_db};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true;";
        builder.Services.AddDbContext<ManageMyMoneyContext>(options =>
            options.UseNpgsql(connectionString)
                   .EnableSensitiveDataLogging(false)
                   .EnableDetailedErrors(false));
    }
}
else
{
    // Local development
    connectionString = builder.Configuration.GetConnectionString("ManageMyMoneyConnection")!;
    builder.Services.AddDbContext<ManageMyMoneyContext>(options =>
        options.UseNpgsql(connectionString)
               .EnableSensitiveDataLogging()
               .EnableDetailedErrors());
}

// Configure Kestrel to use the PORT environment variable
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(connectionString);
builder.Services.AddSharedInfrastructure(builder.Configuration);

var app = builder.Build();

// Verificar si se deben ejecutar las migraciones automáticamente
var runMigrations = Environment.GetEnvironmentVariable("RUN_MIGRATIONS")?.ToLowerInvariant() == "true";
var recreateDatabase = Environment.GetEnvironmentVariable("RECREATE_DATABASE")?.ToLowerInvariant() == "true";
var forceSeedMigrationHistory = Environment.GetEnvironmentVariable("FORCE_SEED_MIGRATION_HISTORY")?.ToLowerInvariant() == "true";

if (runMigrations)
{
    // Aplicar migraciones automáticamente
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ManageMyMoneyContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        try
        {
            logger.LogInformation("Checking database connection...");
            var canConnect = await context.Database.CanConnectAsync();
            logger.LogInformation("Database connection: {CanConnect}", canConnect);

            if (canConnect)
            {
                // Emergency fix for migration state corruption in production
                if (forceSeedMigrationHistory)
                {
                    logger.LogWarning("FORCE_SEED_MIGRATION_HISTORY=true - Attempting emergency migration history fix");
                    try
                    {
                        await EmergencyFixMigrationHistoryAsync(context, logger);
                    }
                    catch (Exception emergencyEx)
                    {
                        logger.LogError(emergencyEx, "Emergency migration history fix failed");
                        // Continue with normal flow
                    }
                }

                if (recreateDatabase)
                {
                    logger.LogWarning("RECREATE_DATABASE=true - Dropping and recreating database...");
                    try
                    {
                        await context.Database.EnsureDeletedAsync();
                        logger.LogInformation("Database dropped successfully");
                        await context.Database.EnsureCreatedAsync();
                        logger.LogInformation("Database recreated successfully");
                    }
                    catch (Exception recreateEx)
                    {
                        logger.LogError(recreateEx, "Error recreating database");
                        throw;
                    }
                }
                else
                {
                    logger.LogInformation("Checking pending migrations...");
                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                    logger.LogInformation("Pending migrations: {Count}", pendingMigrations.Count());

                    if (pendingMigrations.Any())
                    {
                        logger.LogInformation("Applying database migrations...");
                        try
                        {
                            context.Database.Migrate();
                            logger.LogInformation("Database migrations applied successfully");
                        }
                        catch (Exception migrationEx)
                        {
                            logger.LogError(migrationEx, "Error applying migrations - checking for table existence conflicts");
                            
                            // Check if this is a "table already exists" error
                            if (migrationEx.Message.Contains("already exists") || 
                                migrationEx.Message.Contains("42P07") ||
                                migrationEx.InnerException?.Message.Contains("already exists") == true)
                            {
                                logger.LogWarning("Tables already exist - attempting to seed migration history");
                                try
                                {
                                    // Check if migrations history table exists first
                                    var historyTableExists = false;
                                    try
                                    {
                                        await context.Database.ExecuteSqlRawAsync("SELECT 1 FROM \"__EFMigrationsHistory\" LIMIT 1");
                                        historyTableExists = true;
                                    }
                                    catch
                                    {
                                        historyTableExists = false;
                                    }
                                    
                                    if (!historyTableExists)
                                    {
                                        logger.LogInformation("Migration history table does not exist, creating it...");
                                        var createHistoryTableSql = @"
                                            CREATE TABLE ""__EFMigrationsHistory"" (
                                                ""MigrationId"" character varying(150) NOT NULL,
                                                ""ProductVersion"" character varying(32) NOT NULL,
                                                CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId"")
                                            )";
                                        await context.Database.ExecuteSqlRawAsync(createHistoryTableSql);
                                        logger.LogInformation("Migration history table created");
                                    }
                                    
                                    // Get all applied migrations in the database
                                    var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
                                    var allMigrations = context.Database.GetMigrations();
                                    
                                    logger.LogInformation("Applied migrations count: {Count}", appliedMigrations.Count());
                                    logger.LogInformation("Total migrations count: {Count}", allMigrations.Count());
                                    
                                    if (appliedMigrations.Count() == 0 && allMigrations.Any())
                                    {
                                        logger.LogWarning("No migration history found but tables exist - marking migrations as applied");
                                        
                                        // Execute raw SQL to insert migration history
                                        foreach (var migration in allMigrations)
                                        {
                                            try
                                            {
                                                var sql = $"INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('{migration}', '8.0.0') ON CONFLICT DO NOTHING";
                                                await context.Database.ExecuteSqlRawAsync(sql);
                                                logger.LogInformation("Marked migration as applied: {Migration}", migration);
                                            }
                                            catch (Exception markEx)
                                            {
                                                logger.LogWarning(markEx, "Could not mark migration as applied: {Migration}", migration);
                                            }
                                        }
                                        
                                        logger.LogInformation("Migration history seeded successfully");
                                    }
                                    else if (appliedMigrations.Any())
                                    {
                                        logger.LogInformation("Some migrations already applied - attempting to apply remaining");
                                        try
                                        {
                                            context.Database.Migrate();
                                            logger.LogInformation("Remaining migrations applied successfully");
                                        }
                                        catch (Exception retryEx)
                                        {
                                            logger.LogWarning(retryEx, "Could not apply remaining migrations - this may be expected if tables already exist");
                                        }
                                    }
                                }
                                catch (Exception historyEx)
                                {
                                    logger.LogError(historyEx, "Error handling migration history");
                                    
                                    // Fallback: try to ensure database is created (for completely fresh setup)
                                    try
                                    {
                                        await context.Database.EnsureCreatedAsync();
                                        logger.LogInformation("Database ensured created as fallback");
                                    }
                                    catch (Exception createEx)
                                    {
                                        logger.LogError(createEx, "Error with fallback database creation");
                                        logger.LogWarning("Database appears to exist but with incompatible schema. Manual intervention may be required.");
                                        logger.LogWarning("Consider dropping and recreating the database in Railway dashboard.");
                                    }
                                }
                            }
                            else
                            {
                                // For other migration errors, try the original fallback
                                try
                                {
                                    await context.Database.EnsureCreatedAsync();
                                    logger.LogInformation("Database ensured created");
                                }
                                catch (Exception createEx)
                                {
                                    logger.LogError(createEx, "Error creating database - checking if it already exists with different schema");
                                    logger.LogWarning("Database appears to exist but with incompatible schema. Manual intervention may be required.");
                                    logger.LogWarning("Consider dropping and recreating the database in Railway dashboard.");
                                }
                            }
                        }
                    }
                    else
                    {
                        logger.LogInformation("No pending migrations found");
                    }
                }

                logger.LogInformation("Seeding currencies...");
                try
                {
                    await CurrencySeed.SeedAsync(context);
                    logger.LogInformation("Currency seeding completed");
                }
                catch (Exception seedEx)
                {
                    logger.LogError(seedEx, "Error seeding currencies - continuing startup");
                }
            }
            else
            {
                logger.LogError("Cannot connect to database");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in database initialization - continuing startup");
        }
    }
}
else
{
    using (var scope = app.Services.CreateScope())
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Skipping database migrations (RUN_MIGRATIONS not set to 'true')");
    }
}

// Validar configuración de email independientemente de las migraciones
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        // Validate email configuration
        logger.LogInformation("=== Email Configuration Status ===");
        var sendGridApiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY") 
                           ?? Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
        var senderEmail = Environment.GetEnvironmentVariable("SENDER_EMAIL");
        
        if (!string.IsNullOrWhiteSpace(sendGridApiKey) &&
            !string.IsNullOrWhiteSpace(senderEmail))
        {
            var keyPreview = sendGridApiKey.Length >= 10 ? sendGridApiKey.Substring(0, 10) + "..." : "???";
            logger.LogInformation("✅ SendGrid API: CONFIGURED - API Key: {Key}, From: {Sender}", 
                keyPreview, senderEmail);
        }
        else
        {
            logger.LogWarning("⚠️  SendGrid API: NOT CONFIGURED - Emails will not be sent");
            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(sendGridApiKey)) missing.Add("SENDGRID_API_KEY");
            if (string.IsNullOrWhiteSpace(senderEmail)) missing.Add("SENDER_EMAIL");
            logger.LogWarning("   Missing variables: {Missing}", string.Join(", ", missing));
            logger.LogInformation("   Set: SENDGRID_API_KEY=your-api-key SENDER_EMAIL=your-verified-email@gmail.com");
        }
        logger.LogInformation("==================================");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error validating email configuration");
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

// Serve static files from Storage/Files
var storagePath = Path.Combine(builder.Environment.ContentRootPath, "Storage", "Files");
if (!Directory.Exists(storagePath))
{
    Directory.CreateDirectory(storagePath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storagePath),
    RequestPath = "/files"
});

// Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Emergency method to fix migration history corruption
static async Task EmergencyFixMigrationHistoryAsync(ManageMyMoneyContext context, ILogger logger)
{
    logger.LogInformation("🚑 Starting emergency migration history fix...");

    // Check if migrations history table exists
    bool historyTableExists;
    try
    {
        await context.Database.ExecuteSqlRawAsync("SELECT 1 FROM \"__EFMigrationsHistory\" LIMIT 1");
        historyTableExists = true;
        logger.LogInformation("Migration history table exists");
    }
    catch
    {
        historyTableExists = false;
        logger.LogInformation("Migration history table does not exist");
    }

    if (!historyTableExists)
    {
        logger.LogInformation("Creating migration history table...");
        var createHistoryTableSql = @"
            CREATE TABLE ""__EFMigrationsHistory"" (
                ""MigrationId"" character varying(150) NOT NULL,
                ""ProductVersion"" character varying(32) NOT NULL,
                CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId"")
            )";
        await context.Database.ExecuteSqlRawAsync(createHistoryTableSql);
        logger.LogInformation("✅ Migration history table created");
    }

    var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
    var allMigrations = context.Database.GetMigrations();

    logger.LogInformation("Applied migrations: {Count}", appliedMigrations.Count());
    logger.LogInformation("Total migrations: {Count}", allMigrations.Count());

    if (appliedMigrations.Count() == 0 && allMigrations.Any())
    {
        // Check if critical tables exist before marking migrations as applied
        var criticalTablesExist = await CheckCriticalTablesExistAsync(context, logger);
        
        if (criticalTablesExist)
        {
            logger.LogInformation("Critical tables exist - marking all migrations as applied...");
            
            foreach (var migration in allMigrations)
            {
                try
                {
                    var sql = $"INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('{migration}', '8.0.0') ON CONFLICT DO NOTHING";
                    await context.Database.ExecuteSqlRawAsync(sql);
                    logger.LogInformation("✅ Marked migration as applied: {Migration}", migration);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Could not mark migration as applied: {Migration}", migration);
                }
            }
            
            logger.LogInformation("✅ Emergency migration history fix completed successfully");
        }
        else
        {
            logger.LogInformation("Critical tables don't exist - normal migration flow will handle this");
        }
    }
    else
    {
        logger.LogInformation("Migration history already exists with {Count} applied migrations", appliedMigrations.Count());
    }
}

static async Task<bool> CheckCriticalTablesExistAsync(ManageMyMoneyContext context, ILogger logger)
{
    var criticalTables = new[] { "users", "account_transactions", "expenses", "categories" };
    var existingTables = 0;

    foreach (var table in criticalTables)
    {
        try
        {
            await context.Database.ExecuteSqlRawAsync($"SELECT 1 FROM \"{table}\" LIMIT 1");
            existingTables++;
            logger.LogInformation("✅ Table exists: {TableName}", table);
        }
        catch
        {
            logger.LogInformation("❌ Table missing: {TableName}", table);
        }
    }

    var tablesExist = existingTables > criticalTables.Length / 2;
    logger.LogInformation("Critical tables check: {ExistingCount}/{TotalCount} exist = {Result}", 
        existingTables, criticalTables.Length, tablesExist ? "PASS" : "FAIL");
    
    return tablesExist;
}
