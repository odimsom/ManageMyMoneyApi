using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Application.Common.Interfaces;

public interface IEmailService
{
    // Base methods
    Task<OperationResult> SendEmailAsync(string to, string subject, string body);
    Task<OperationResult> SendTemplateEmailAsync(string to, string templateName, object model);
    Task<OperationResult> SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string body);

    // Casual User Templates
    Task<OperationResult> SendWelcomeCasualEmailAsync(string to, string userName);
    Task<OperationResult> SendFirstExpenseRegisteredEmailAsync(string to, string userName, decimal amount, string description, string category);
    Task<OperationResult> SendWeeklyAlertSimpleEmailAsync(string to, string userName, decimal weeklyTotal, int expenseCount, decimal dailyAverage, decimal highestExpense);
    Task<OperationResult> SendRegistrationReminderEmailAsync(string to, string userName, int daysSinceLastExpense);
    Task<OperationResult> SendWeeklySummarySimpleEmailAsync(string to, string userName, DateTime weekStart, DateTime weekEnd, decimal weeklyTotal, int expenseCount, string weeklyMessage);

    // Organized User Templates
    Task<OperationResult> SendWelcomeOrganizedEmailAsync(string to, string userName);
    Task<OperationResult> SendBudgetCreatedEmailAsync(string to, string userName, string budgetName, decimal limit, string period, Guid budgetId);
    Task<OperationResult> SendBudgetAlert80EmailAsync(string to, string userName, string budgetName, decimal limit, decimal current, decimal remaining, decimal percentageUsed, int daysRemaining, Guid budgetId);
    Task<OperationResult> SendBudgetExceededEmailAsync(string to, string userName, string budgetName, decimal limit, decimal current, decimal excess, int daysRemaining, Guid budgetId);
    Task<OperationResult> SendMonthlySummaryEmailAsync(string to, string userName, MonthlySummaryModel model);
    Task<OperationResult> SendGoalAchievedEmailAsync(string to, string userName, string goalName, decimal targetAmount, int daysToComplete, int contributionsCount, decimal averageContribution);
    Task<OperationResult> SendGoalAtRiskEmailAsync(string to, string userName, string goalName, decimal targetAmount, decimal currentAmount, decimal progressPercentage, DateTime targetDate, int daysRemaining, decimal requiredMonthly, Guid goalId);

    // Power User Templates
    Task<OperationResult> SendWelcomePowerUserEmailAsync(string to, string userName);
    Task<OperationResult> SendCustomReportEmailAsync(string to, string userName, string reportName, string dateRange, string reportSummaryHtml, string downloadUrl);
    Task<OperationResult> SendMultiAccountAlertEmailAsync(string to, string userName, int alertCount, string accountAlertsHtml, decimal totalBalance);
    Task<OperationResult> SendFinancialProjectionEmailAsync(string to, string userName, FinancialProjectionModel model);
    Task<OperationResult> SendPeriodComparisonEmailAsync(string to, string userName, PeriodComparisonModel model);
    Task<OperationResult> SendExportCompletedEmailAsync(string to, string userName, string fileName, string fileSize, string exportType, string dateRange, string downloadUrl, int expirationHours);

    // System Templates
    Task<OperationResult> SendEmailVerificationAsync(string to, string userName, string verificationCode, string verificationUrl, int expirationMinutes);
    Task<OperationResult> SendPasswordResetEmailAsync(string to, string userName, string resetUrl, int expirationMinutes);
    Task<OperationResult> SendPasswordChangedEmailAsync(string to, string userName, DateTime changeDate, string ipAddress);
    Task<OperationResult> SendProfileUpdatedEmailAsync(string to, string userName, string changesHtml, DateTime updateDate, string deviceInfo);
    Task<OperationResult> SendAccountDeletionEmailAsync(string to, string userName, int daysUntilDeletion, string cancelDeletionUrl);
    Task<OperationResult> SendSyncErrorEmailAsync(string to, string userName, string bankName, string errorMessage);
    Task<OperationResult> SendMaintenanceNoticeEmailAsync(string to, string userName, DateTime maintenanceStart, int durationMinutes, string timeZone, string improvementsSummary);
}

public record MonthlySummaryModel
{
    public int Year { get; init; }
    public int Month { get; init; }
    public required string MonthName { get; init; }
    public decimal TotalIncome { get; init; }
    public decimal TotalExpenses { get; init; }
    public decimal NetBalance { get; init; }
    public decimal SavingsRate { get; init; }
    public required string Currency { get; init; }
    public required string TopCategoriesHtml { get; init; }
    public required string ComparisonColor { get; init; }
    public required string ComparisonIcon { get; init; }
    public required string ComparisonText { get; init; }
}

public record FinancialProjectionModel
{
    public int ProjectionMonths { get; init; }
    public decimal CurrentMonthlyAverage { get; init; }
    public required string TrendColor { get; init; }
    public required string TrendDirection { get; init; }
    public decimal TrendPercentage { get; init; }
    public decimal ProjectedNextMonth { get; init; }
    public decimal ProjectedSavings { get; init; }
    public required string Recommendation { get; init; }
}

public record PeriodComparisonModel
{
    public required string Period1Name { get; init; }
    public required string Period2Name { get; init; }
    public decimal Period1Income { get; init; }
    public decimal Period2Income { get; init; }
    public decimal Period1Expenses { get; init; }
    public decimal Period2Expenses { get; init; }
    public required string IncomeChangeColor { get; init; }
    public required string IncomeChangeIcon { get; init; }
    public decimal IncomeChangePercentage { get; init; }
    public required string ExpenseChangeColor { get; init; }
    public required string ExpenseChangeIcon { get; init; }
    public decimal ExpenseChangePercentage { get; init; }
    public required string BalanceChangeColor { get; init; }
    public required string BalanceChangeIcon { get; init; }
    public decimal BalanceChange { get; init; }
}
