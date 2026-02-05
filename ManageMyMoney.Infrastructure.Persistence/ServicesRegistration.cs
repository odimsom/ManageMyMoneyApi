using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ManageMyMoney.Core.Domain.Interfaces;
using ManageMyMoney.Infrastructure.Persistence.Context;
using ManageMyMoney.Infrastructure.Persistence.Repositories.Auth;
using ManageMyMoney.Infrastructure.Persistence.Repositories.Expenses;
using ManageMyMoney.Infrastructure.Persistence.Repositories.Categories;
using ManageMyMoney.Infrastructure.Persistence.Repositories.Accounts;
using ManageMyMoney.Infrastructure.Persistence.Repositories.Budgets;

namespace ManageMyMoney.Infrastructure.Persistence;

public static class ServicesRegistration
{
    public static IServiceCollection AddPersistenceServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ManageMyMoneyConnection");

        services.AddDbContext<ManageMyMoneyContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(ManageMyMoneyContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            }));

        // Auth Repositories
        services.AddScoped<IUserRepository, UserRepository>();

        // Expense Repositories
        services.AddScoped<IExpenseRepository, ExpenseRepository>();

        // Category Repositories
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        // Account Repositories
        services.AddScoped<IAccountRepository, AccountRepository>();

        // Budget Repositories
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<ISavingsGoalRepository, SavingsGoalRepository>();

        return services;
    }
}
