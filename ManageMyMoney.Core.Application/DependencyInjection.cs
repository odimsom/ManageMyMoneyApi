using Microsoft.Extensions.DependencyInjection;
using ManageMyMoney.Core.Application.Features.Auth;
using ManageMyMoney.Core.Application.Features.Expenses;
using ManageMyMoney.Core.Application.Features.Income;
using ManageMyMoney.Core.Application.Features.Categories;
using ManageMyMoney.Core.Application.Features.Accounts;
using ManageMyMoney.Core.Application.Features.Budgets;
using ManageMyMoney.Core.Application.Features.Reports;
using ManageMyMoney.Core.Application.Features.Notifications;

namespace ManageMyMoney.Core.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Auth
        services.AddScoped<IAuthService, AuthService>();

        // Expenses
        services.AddScoped<IExpenseService, ExpenseService>();

        // Income - Interface defined, implementation pending
        // services.AddScoped<IIncomeService, IncomeService>();

        // Categories - Interface defined, implementation pending
        // services.AddScoped<ICategoryService, CategoryService>();

        // Accounts - Interface defined, implementation pending
        // services.AddScoped<IAccountService, AccountService>();

        // Budgets - Interface defined, implementation pending
        // services.AddScoped<IBudgetService, BudgetService>();

        // Reports - Interface defined, implementation pending
        // services.AddScoped<IReportService, ReportService>();

        // Notifications - Interface defined, implementation pending
        // services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}
