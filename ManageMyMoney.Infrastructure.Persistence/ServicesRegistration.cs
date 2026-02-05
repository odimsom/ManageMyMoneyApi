using ManageMyMoney.Core.Domain.Interfaces;
using ManageMyMoney.Infrastructure.Persistence.Context;
using ManageMyMoney.Infrastructure.Persistence.Repositories.Accounts;
using ManageMyMoney.Infrastructure.Persistence.Repositories.Auth;
using ManageMyMoney.Infrastructure.Persistence.Repositories.Budgets;
using ManageMyMoney.Infrastructure.Persistence.Repositories.Categories;
using ManageMyMoney.Infrastructure.Persistence.Repositories.Expenses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ManageMyMoney.Infrastructure.Persistence;

public static class ServicesRegistration
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Railway provee DATABASE_URL como variable de entorno
        var connectionString = GetConnectionString(configuration);

        services.AddDbContext<ManageMyMoneyContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(ManageMyMoneyContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            }));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<ISavingsGoalRepository, SavingsGoalRepository>();

        return services;
    }

    private static string GetConnectionString(IConfiguration configuration)
    {
        // Railway provee DATABASE_URL en formato: postgresql://user:password@host:port/database
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        
        if (!string.IsNullOrEmpty(databaseUrl))
        {
            return ConvertDatabaseUrlToConnectionString(databaseUrl);
        }

        // Fallback a configuración local
        return configuration.GetConnectionString("ManageMyMoneyConnection") 
            ?? throw new InvalidOperationException("Database connection string not found");
    }

    private static string ConvertDatabaseUrlToConnectionString(string databaseUrl)
    {
        // Parsear URL de Railway: postgresql://user:password@host:port/database
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');
        var username = userInfo[0];
        var password = userInfo.Length > 1 ? userInfo[1] : "";
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');

        return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
    }
}
