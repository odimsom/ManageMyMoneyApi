using System.Reflection;
using Microsoft.EntityFrameworkCore;
using ManageMyMoney.Core.Domain.Entities.Accounts;
using ManageMyMoney.Core.Domain.Entities.Auth;
using ManageMyMoney.Core.Domain.Entities.Budgets;
using ManageMyMoney.Core.Domain.Entities.Categories;
using ManageMyMoney.Core.Domain.Entities.Expenses;
using ManageMyMoney.Core.Domain.Entities.Income;
using ManageMyMoney.Core.Domain.Entities.Notifications;
using ManageMyMoney.Core.Domain.Entities.System;

namespace ManageMyMoney.Infrastructure.Persistence.Context;

public class ManageMyMoneyContext : DbContext
{
    public ManageMyMoneyContext(DbContextOptions<ManageMyMoneyContext> options)
        : base(options)
    {
    }

    #region DbSets - Auth
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    #endregion

    #region DbSets - Accounts
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<AccountTransaction> AccountTransactions => Set<AccountTransaction>();
    public DbSet<CreditCard> CreditCards => Set<CreditCard>();
    #endregion

    #region DbSets - Expenses
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<RecurringExpense> RecurringExpenses => Set<RecurringExpense>();
    public DbSet<ExpenseAttachment> ExpenseAttachments => Set<ExpenseAttachment>();
    public DbSet<ExpenseTag> ExpenseTags => Set<ExpenseTag>();
    public DbSet<ExpenseSplit> ExpenseSplits => Set<ExpenseSplit>();
    #endregion

    #region DbSets - Income
    public DbSet<Income> Incomes => Set<Income>();
    public DbSet<RecurringIncome> RecurringIncomes => Set<RecurringIncome>();
    public DbSet<IncomeSource> IncomeSources => Set<IncomeSource>();
    #endregion

    #region DbSets - Categories
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Subcategory> Subcategories => Set<Subcategory>();
    public DbSet<CategoryBudget> CategoryBudgets => Set<CategoryBudget>();
    #endregion

    #region DbSets - Budgets
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<SavingsGoal> SavingsGoals => Set<SavingsGoal>();
    public DbSet<GoalContribution> GoalContributions => Set<GoalContribution>();
    #endregion

    #region DbSets - Notifications
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
    public DbSet<Alert> Alerts => Set<Alert>();
    #endregion

    #region DbSets - System
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public DbSet<TaxRate> TaxRates => Set<TaxRate>();
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ManageMyMoneyContext).Assembly);

        ApplySnakeCaseNamingConvention(modelBuilder);
    }

    private static void ApplySnakeCaseNamingConvention(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Table names - only for non-owned entities
            if (!entity.IsOwned())
            {
                entity.SetTableName(ToSnakeCase(entity.GetTableName() ?? entity.ClrType.Name));
            }

            // Column names - only for owned entities if explicitly configured
            foreach (var property in entity.GetProperties())
            {
                if (!entity.IsOwned() || property.GetColumnName() != property.Name)
                {
                    property.SetColumnName(ToSnakeCase(property.GetColumnName() ?? property.Name));
                }
            }

            // Foreign keys - skip owned entities to avoid conflicts
            if (!entity.IsOwned())
            {
                foreach (var foreignKey in entity.GetForeignKeys())
                {
                    foreignKey.SetConstraintName(ToSnakeCase(foreignKey.GetConstraintName() ?? ""));
                }
            }

            // Indexes
            foreach (var index in entity.GetIndexes())
            {
                index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName() ?? ""));
            }
        }
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = new System.Text.StringBuilder();
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (char.IsUpper(c))
            {
                if (i > 0)
                    result.Append('_');
                result.Append(char.ToLowerInvariant(c));
            }
            else
            {
                result.Append(c);
            }
        }
        return result.ToString();
    }
}
