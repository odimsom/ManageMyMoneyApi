using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ManageMyMoney.Infrastructure.Persistence.Context;
using ManageMyMoney.Infrastructure.Persistence.Seeds;

namespace ManageMyMoney.Infrastructure.Persistence.Services;

public class DatabaseInitializationService
{
    private readonly ManageMyMoneyContext _context;
    private readonly ILogger<DatabaseInitializationService> _logger;

    public DatabaseInitializationService(
        ManageMyMoneyContext context, 
        ILogger<DatabaseInitializationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DatabaseInitializationResult> InitializeDatabaseAsync(
        bool runMigrations = true,
        bool recreateDatabase = false)
    {
        var result = new DatabaseInitializationResult();

        try
        {
            _logger.LogInformation("Starting database initialization...");

            // Check database connection
            _logger.LogInformation("Checking database connection...");
            var canConnect = await _context.Database.CanConnectAsync();
            _logger.LogInformation("Database connection: {CanConnect}", canConnect);
            
            if (!canConnect)
            {
                _logger.LogError("Cannot connect to database");
                result.IsSuccessful = false;
                result.ErrorMessage = "Cannot connect to database";
                return result;
            }

            // Handle database recreation if requested
            if (recreateDatabase)
            {
                _logger.LogWarning("RECREATE_DATABASE=true - Dropping and recreating database...");
                await _context.Database.EnsureDeletedAsync();
                _logger.LogInformation("Database dropped successfully");
                await _context.Database.EnsureCreatedAsync();
                _logger.LogInformation("Database recreated successfully");
                result.DatabaseRecreated = true;
            }
            else if (runMigrations)
            {
                await HandleMigrationsAsync(result);
            }

            // Seed data
            await SeedDataAsync();

            result.IsSuccessful = true;
            _logger.LogInformation("Database initialization completed successfully");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database initialization");
            result.IsSuccessful = false;
            result.ErrorMessage = ex.Message;
            result.Exception = ex;
            return result;
        }
    }

    private async Task HandleMigrationsAsync(DatabaseInitializationResult result)
    {
        _logger.LogInformation("Checking pending migrations...");
        var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
        _logger.LogInformation("Pending migrations: {Count}", pendingMigrations.Count());

        if (!pendingMigrations.Any())
        {
            _logger.LogInformation("No pending migrations found");
            return;
        }

        _logger.LogInformation("Applying database migrations...");
        try
        {
            _context.Database.Migrate();
            _logger.LogInformation("Database migrations applied successfully");
            result.MigrationsApplied = true;
        }
        catch (Exception migrationEx)
        {
            await HandleMigrationErrorAsync(migrationEx, result);
        }
    }

    private async Task HandleMigrationErrorAsync(Exception migrationEx, DatabaseInitializationResult result)
    {
        _logger.LogError(migrationEx, "Error applying migrations");

        // Check for table existence conflicts
        var isTableConflict = IsTableExistsError(migrationEx);
        
        if (isTableConflict)
        {
            _logger.LogWarning("Detected table existence conflict - attempting to fix migration history");
            var historyFixed = await TryFixMigrationHistoryAsync();
            
            if (historyFixed)
            {
                result.MigrationHistoryFixed = true;
                _logger.LogInformation("Migration history fixed successfully");
            }
            else
            {
                await FallbackDatabaseCreation(result);
            }
        }
        else
        {
            // For other errors, try fallback
            await FallbackDatabaseCreation(result);
        }
    }

    private static bool IsTableExistsError(Exception ex)
    {
        var message = ex.Message;
        var innerMessage = ex.InnerException?.Message ?? "";
        
        return message.Contains("already exists") ||
               message.Contains("42P07") ||
               innerMessage.Contains("already exists") ||
               innerMessage.Contains("42P07");
    }

    private async Task<bool> TryFixMigrationHistoryAsync()
    {
        try
        {
            // Check if migrations history table exists
            var historyTableExists = await CheckMigrationHistoryTableExistsAsync();
            
            if (!historyTableExists)
            {
                _logger.LogInformation("Creating migration history table...");
                await CreateMigrationHistoryTableAsync();
            }

            // Check applied migrations
            var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();
            var allMigrations = _context.Database.GetMigrations();
            
            _logger.LogInformation("Applied migrations count: {Count}", appliedMigrations.Count());
            _logger.LogInformation("Total migrations count: {Count}", allMigrations.Count());
            
            // If no migrations are recorded but tables exist, mark all as applied
            if (appliedMigrations.Count() == 0 && allMigrations.Any())
            {
                _logger.LogWarning("No migration history found but tables exist - marking migrations as applied");
                await MarkMigrationsAsAppliedAsync(allMigrations);
                return true;
            }

            // Try to apply remaining migrations
            if (appliedMigrations.Any())
            {
                _logger.LogInformation("Some migrations applied - attempting to apply remaining");
                try
                {
                    _context.Database.Migrate();
                    _logger.LogInformation("Remaining migrations applied successfully");
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not apply remaining migrations");
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fixing migration history");
            return false;
        }
    }

    private async Task<bool> CheckMigrationHistoryTableExistsAsync()
    {
        try
        {
            await _context.Database.ExecuteSqlRawAsync("SELECT 1 FROM \"__EFMigrationsHistory\" LIMIT 1");
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task CreateMigrationHistoryTableAsync()
    {
        var sql = @"
            CREATE TABLE ""__EFMigrationsHistory"" (
                ""MigrationId"" character varying(150) NOT NULL,
                ""ProductVersion"" character varying(32) NOT NULL,
                CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY (""MigrationId"")
            )";
        
        await _context.Database.ExecuteSqlRawAsync(sql);
        _logger.LogInformation("Migration history table created");
    }

    private async Task MarkMigrationsAsAppliedAsync(IEnumerable<string> migrations)
    {
        foreach (var migration in migrations)
        {
            try
            {
                var sql = $"INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('{migration}', '8.0.0') ON CONFLICT DO NOTHING";
                await _context.Database.ExecuteSqlRawAsync(sql);
                _logger.LogInformation("Marked migration as applied: {Migration}", migration);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not mark migration as applied: {Migration}", migration);
            }
        }
    }

    private async Task FallbackDatabaseCreation(DatabaseInitializationResult result)
    {
        _logger.LogInformation("Attempting fallback database creation...");
        try
        {
            await _context.Database.EnsureCreatedAsync();
            _logger.LogInformation("Database ensured created via fallback method");
            result.FallbackCreationUsed = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallback database creation failed");
            _logger.LogWarning("Manual intervention may be required. Consider:");
            _logger.LogWarning("1. Dropping and recreating the database in Railway dashboard");
            _logger.LogWarning("2. Setting RECREATE_DATABASE=true environment variable");
            
            throw;
        }
    }

    private async Task SeedDataAsync()
    {
        _logger.LogInformation("Seeding currencies...");
        try
        {
            await CurrencySeed.SeedAsync(_context);
            _logger.LogInformation("Currency seeding completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding currencies");
            // Don't throw - continue with startup even if seeding fails
        }
    }
}

public class DatabaseInitializationResult
{
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }
    public bool DatabaseRecreated { get; set; }
    public bool MigrationsApplied { get; set; }
    public bool MigrationHistoryFixed { get; set; }
    public bool FallbackCreationUsed { get; set; }
}