using ManageMyMoney.Core.Domain.Interfaces;
using ManageMyMoney.Infrastructure.Persistence.Context;
using ManageMyMoney.Infrastructure.Persistence.Repositories.Accounts;
using ManageMyMoney.Infrastructure.Persistence.Repositories.Auth;
using ManageMyMoney.Infrastructure.Persistence.Repositories.Budgets;
using ManageMyMoney.Infrastructure.Persistence.Repositories.Categories;
using ManageMyMoney.Infrastructure.Persistence.Repositories.Expenses;
using ManageMyMoney.Infrastructure.Persistence.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ManageMyMoney.Infrastructure.Persistence;

public static class ServicesRegistration
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
        }

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

        // Services
        services.AddScoped<DatabaseInitializationService>();

        return services;
    }
}
