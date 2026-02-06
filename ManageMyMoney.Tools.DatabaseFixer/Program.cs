using System;
using System.CommandLine;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ManageMyMoney.Infrastructure.Persistence;
using ManageMyMoney.Infrastructure.Persistence.Context;

namespace ManageMyMoney.Tools.DatabaseFixer;

/// <summary>
/// Utility to fix database migration state issues in production
/// </summary>
public class DatabaseFixerProgram
{
    public static async Task<int> Main(string[] args)
    {
        var connectionStringOption = new Option<string>(
            name: "--connection-string",
            description: "Database connection string") { IsRequired = true };

        var actionOption = new Option<string>(
            name: "--action", 
            description: "Action to perform: check|fix|reset") { IsRequired = true };

        var rootCommand = new RootCommand("Database Migration State Fixer")
        {
            connectionStringOption,
            actionOption
        };

        rootCommand.SetHandler(async (connectionString, action) =>
        {
            await ExecuteActionAsync(connectionString, action);
        }, connectionStringOption, actionOption);

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task ExecuteActionAsync(string connectionString, string action)
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        services.AddPersistenceServices(connectionString);

        var serviceProvider = services.BuildServiceProvider();
        var context = serviceProvider.GetRequiredService<ManageMyMoneyContext>();
        var logger = serviceProvider.GetRequiredService<ILogger<DatabaseFixerProgram>>();

        try
        {
            switch (action.ToLowerInvariant())
            {
                case "check":
                    await CheckDatabaseStateAsync(context, logger);
                    break;
                case "fix":
                    await FixMigrationStateAsync(context, logger);
                    break;
                case "reset":
                    await ResetMigrationStateAsync(context, logger);
                    break;
                default:
                    logger.LogError("Invalid action: {Action}. Valid actions: check|fix|reset", action);
                    Environment.Exit(1);
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing action: {Action}", action);
            Environment.Exit(1);
        }
        finally
        {
            await serviceProvider.DisposeAsync();
        }
    }

    private static async Task CheckDatabaseStateAsync(ManageMyMoneyContext context, ILogger logger)
    {
        logger.LogInformation("🔍 Checking database state...");

        // Check connection
        var canConnect = await context.Database.CanConnectAsync();
        logger.LogInformation("Database connection: {CanConnect}", canConnect);

        if (!canConnect)
        {
            logger.LogError("❌ Cannot connect to database");
            return;
        }

        // Check if migration history table exists
        bool historyTableExists;
        try
        {
            await context.Database.ExecuteSqlRawAsync("SELECT 1 FROM \"__EFMigrationsHistory\" LIMIT 1");
            historyTableExists = true;
        }
        catch
        {
            historyTableExists = false;
        }

        logger.LogInformation("Migration history table exists: {Exists}", historyTableExists);

        if (historyTableExists)
        {
            var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
            var allMigrations = context.Database.GetMigrations();
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

            logger.LogInformation("Applied migrations: {Count}", appliedMigrations.Count());
            logger.LogInformation("Total migrations: {Count}", allMigrations.Count());
            logger.LogInformation("Pending migrations: {Count}", pendingMigrations.Count());

            foreach (var migration in appliedMigrations)
            {
                logger.LogInformation("✅ Applied: {Migration}", migration);
            }

            foreach (var migration in pendingMigrations)
            {
                logger.LogInformation("⏳ Pending: {Migration}", migration);
            }
        }

        // Check if critical tables exist
        await CheckTableExistenceAsync(context, logger, "users");
        await CheckTableExistenceAsync(context, logger, "account_transactions");
        await CheckTableExistenceAsync(context, logger, "expenses");
        await CheckTableExistenceAsync(context, logger, "categories");
    }

    private static async Task CheckTableExistenceAsync(ManageMyMoneyContext context, ILogger logger, string tableName)
    {
        try
        {
            var sql = $"SELECT 1 FROM \"{tableName}\" LIMIT 1";
            await context.Database.ExecuteSqlRawAsync(sql);
            logger.LogInformation("✅ Table exists: {TableName}", tableName);
        }
        catch
        {
            logger.LogInformation("❌ Table missing: {TableName}", tableName);
        }
    }

    private static async Task FixMigrationStateAsync(ManageMyMoneyContext context, ILogger logger)
    {
        logger.LogInformation("🔧 Fixing migration state...");

        try
        {
            // Check if migrations history table exists first
            bool historyTableExists;
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
                logger.LogWarning("No migration history found but checking for existing tables...");

                // Check if core tables exist
                var tablesExist = await CheckCoreTablesExistAsync(context);
                
                if (tablesExist)
                {
                    logger.LogInformation("Tables exist - marking all migrations as applied...");
                    
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
                    logger.LogInformation("✅ Migration state fixed successfully");
                }
                else
                {
                    logger.LogInformation("Core tables don't exist - running migrations normally...");
                    context.Database.Migrate();
                    logger.LogInformation("✅ Migrations applied successfully");
                }
            }
            else
            {
                // Try to apply remaining migrations
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count());
                    context.Database.Migrate();
                    logger.LogInformation("✅ Pending migrations applied successfully");
                }
                else
                {
                    logger.LogInformation("✅ Database is up to date");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error fixing migration state");
            throw;
        }
    }

    private static async Task<bool> CheckCoreTablesExistAsync(ManageMyMoneyContext context)
    {
        var coreTables = new[] { "users", "account_transactions", "expenses", "categories" };
        var existingTables = 0;

        foreach (var table in coreTables)
        {
            try
            {
                await context.Database.ExecuteSqlRawAsync($"SELECT 1 FROM \"{table}\" LIMIT 1");
                existingTables++;
            }
            catch
            {
                // Table doesn't exist
            }
        }

        return existingTables > coreTables.Length / 2; // More than half of core tables exist
    }

    private static async Task ResetMigrationStateAsync(ManageMyMoneyContext context, ILogger logger)
    {
        logger.LogWarning("🗑️ Resetting migration state (this will clear migration history)...");
        
        try
        {
            // Drop migration history table
            await context.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS \"__EFMigrationsHistory\"");
            logger.LogInformation("Migration history table dropped");

            // Recreate and apply all migrations
            logger.LogInformation("Applying all migrations...");
            context.Database.Migrate();
            
            logger.LogInformation("✅ Migration state reset and migrations applied successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error resetting migration state");
            throw;
        }
    }
}