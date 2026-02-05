using System.Net;
using System.Net.Mail;
using ManageMyMoney.Core.Application.Common.Interfaces;
using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Infrastructure.Shared.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ManageMyMoney.Infrastructure.Shared.Services.Email;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;
    private readonly string _appUrl;
    private readonly int _currentYear;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _appUrl = "https://app.managemymoney.com";
        _currentYear = DateTime.UtcNow.Year;
    }

    #region Base Methods

    public async Task<OperationResult> SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            using var client = CreateSmtpClient();
            using var message = CreateMailMessage(to, subject, body);
            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent successfully to {Recipient}", to);
            return OperationResult.Success();
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP error sending email to {Recipient}", to);
            return OperationResult.Failure($"SMTP error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipient}", to);
            return OperationResult.Failure($"Failed to send email: {ex.Message}");
        }
    }

    public async Task<OperationResult> SendTemplateEmailAsync(string to, string templateName, object model)
    {
        try
        {
            var templatePath = Path.Combine(_settings.TemplatesPath, $"{templateName}.html");
            if (!File.Exists(templatePath))
            {
                _logger.LogWarning("Email template not found: {Template}", templateName);
                return OperationResult.Failure($"Template not found: {templateName}");
            }

            var template = await File.ReadAllTextAsync(templatePath);
            var body = ReplaceTokens(template, model);
            var subject = GetSubjectFromTemplate(templateName);

            return await SendEmailAsync(to, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending template email {Template} to {Recipient}", templateName, to);
            return OperationResult.Failure($"Error sending template email: {ex.Message}");
        }
    }

    public async Task<OperationResult> SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string body)
    {
        var recipientList = recipients.ToList();
        var failedCount = 0;

        foreach (var recipient in recipientList)
        {
            var result = await SendEmailAsync(recipient, subject, body);
            if (result.IsFailure) failedCount++;
            await Task.Delay(100);
        }

        if (failedCount == 0) return OperationResult.Success();
        if (failedCount == recipientList.Count) return OperationResult.Failure("All emails failed to send");
        return OperationResult.Failure($"{failedCount} of {recipientList.Count} emails failed");
    }

    #endregion

    #region Casual User Emails

    public Task<OperationResult> SendWelcomeCasualEmailAsync(string to, string userName) =>
        SendTemplateEmailAsync(to, "Casual/WelcomeCasual", new { UserName = userName, AppUrl = _appUrl, Year = _currentYear });

    public Task<OperationResult> SendFirstExpenseRegisteredEmailAsync(string to, string userName, decimal amount, string description, string category) =>
        SendTemplateEmailAsync(to, "Casual/FirstExpenseRegistered", new { UserName = userName, Amount = amount.ToString("C"), Description = description, Category = category, AppUrl = _appUrl, Year = _currentYear });

    public Task<OperationResult> SendWeeklyAlertSimpleEmailAsync(string to, string userName, decimal weeklyTotal, int expenseCount, decimal dailyAverage, decimal highestExpense) =>
        SendTemplateEmailAsync(to, "Casual/WeeklyAlertSimple", new { UserName = userName, WeeklyTotal = weeklyTotal.ToString("C"), ExpenseCount = expenseCount, DailyAverage = dailyAverage.ToString("C"), HighestExpense = highestExpense.ToString("C"), AppUrl = _appUrl, Year = _currentYear });

    public Task<OperationResult> SendRegistrationReminderEmailAsync(string to, string userName, int daysSinceLastExpense) =>
        SendTemplateEmailAsync(to, "Casual/RegistrationReminder", new { UserName = userName, DaysSinceLastExpense = daysSinceLastExpense, AppUrl = _appUrl, Year = _currentYear });

    public Task<OperationResult> SendWeeklySummarySimpleEmailAsync(string to, string userName, DateTime weekStart, DateTime weekEnd, decimal weeklyTotal, int expenseCount, string weeklyMessage) =>
        SendTemplateEmailAsync(to, "Casual/WeeklySummarySimple", new { UserName = userName, WeekStart = weekStart.ToString("MMM dd"), WeekEnd = weekEnd.ToString("MMM dd"), WeeklyTotal = weeklyTotal.ToString("C"), ExpenseCount = expenseCount, WeeklyMessage = weeklyMessage, AppUrl = _appUrl, Year = _currentYear });

    #endregion

    #region Organized User Emails

    public Task<OperationResult> SendWelcomeOrganizedEmailAsync(string to, string userName) =>
        SendTemplateEmailAsync(to, "Organized/WelcomeOrganized", new { UserName = userName, AppUrl = _appUrl, Year = _currentYear });

    public Task<OperationResult> SendBudgetCreatedEmailAsync(string to, string userName, string budgetName, decimal limit, string period, Guid budgetId) =>
        SendTemplateEmailAsync(to, "Organized/BudgetCreated", new { UserName = userName, BudgetName = budgetName, Limit = limit.ToString("C"), Period = period, BudgetId = budgetId, AppUrl = _appUrl, Year = _currentYear });

    public Task<OperationResult> SendBudgetAlert80EmailAsync(string to, string userName, string budgetName, decimal limit, decimal current, decimal remaining, decimal percentageUsed, int daysRemaining, Guid budgetId) =>
        SendTemplateEmailAsync(to, "Organized/BudgetAlert80", new { UserName = userName, BudgetName = budgetName, Limit = limit.ToString("C"), Current = current.ToString("C"), Remaining = remaining.ToString("C"), PercentageUsed = percentageUsed.ToString("F0"), DaysRemaining = daysRemaining, BudgetId = budgetId, AppUrl = _appUrl, Year = _currentYear });

    public Task<OperationResult> SendBudgetExceededEmailAsync(string to, string userName, string budgetName, decimal limit, decimal current, decimal excess, int daysRemaining, Guid budgetId) =>
        SendTemplateEmailAsync(to, "Organized/BudgetExceeded", new { UserName = userName, BudgetName = budgetName, Limit = limit.ToString("C"), Current = current.ToString("C"), Excess = excess.ToString("C"), DaysRemaining = daysRemaining, BudgetId = budgetId, AppUrl = _appUrl, Year = _currentYear });

    public Task<OperationResult> SendMonthlySummaryEmailAsync(string to, string userName, MonthlySummaryModel model) =>
        SendTemplateEmailAsync(to, "Organized/MonthlySummary", new
        {
            UserName = userName,
            model.MonthName,
            model.Year,
            model.Month,
            TotalIncome = model.TotalIncome.ToString("C"),
            TotalExpenses = model.TotalExpenses.ToString("C"),
            NetBalance = model.NetBalance.ToString("C"),
            SavingsRate = model.SavingsRate.ToString("F1"),
            model.Currency,
            model.TopCategoriesHtml,
            model.ComparisonColor,
            model.ComparisonIcon,
            model.ComparisonText,
            AppUrl = _appUrl
        });

    public Task<OperationResult> SendGoalAchievedEmailAsync(string to, string userName, string goalName, decimal targetAmount, int daysToComplete, int contributionsCount, decimal averageContribution) =>
        SendTemplateEmailAsync(to, "Organized/GoalAchieved", new { UserName = userName, GoalName = goalName, TargetAmount = targetAmount.ToString("C"), DaysToComplete = daysToComplete, ContributionsCount = contributionsCount, AverageContribution = averageContribution.ToString("C"), AppUrl = _appUrl, Year = _currentYear });

    public Task<OperationResult> SendGoalAtRiskEmailAsync(string to, string userName, string goalName, decimal targetAmount, decimal currentAmount, decimal progressPercentage, DateTime targetDate, int daysRemaining, decimal requiredMonthly, Guid goalId) =>
        SendTemplateEmailAsync(to, "Organized/GoalAtRisk", new { UserName = userName, GoalName = goalName, TargetAmount = targetAmount.ToString("C"), CurrentAmount = currentAmount.ToString("C"), ProgressPercentage = progressPercentage.ToString("F1"), TargetDate = targetDate.ToString("MMM dd, yyyy"), DaysRemaining = daysRemaining, RequiredMonthly = requiredMonthly.ToString("C"), GoalId = goalId, AppUrl = _appUrl, Year = _currentYear });

    #endregion

    #region Power User Emails

    public Task<OperationResult> SendWelcomePowerUserEmailAsync(string to, string userName) =>
        SendTemplateEmailAsync(to, "PowerUser/WelcomePowerUser", new { UserName = userName, AppUrl = _appUrl, Year = _currentYear });

    public Task<OperationResult> SendCustomReportEmailAsync(string to, string userName, string reportName, string dateRange, string reportSummaryHtml, string downloadUrl) =>
        SendTemplateEmailAsync(to, "PowerUser/CustomReport", new { UserName = userName, ReportName = reportName, DateRange = dateRange, ReportSummaryHtml = reportSummaryHtml, DownloadUrl = downloadUrl, AppUrl = _appUrl, Year = _currentYear });

    public Task<OperationResult> SendMultiAccountAlertEmailAsync(string to, string userName, int alertCount, string accountAlertsHtml, decimal totalBalance) =>
        SendTemplateEmailAsync(to, "PowerUser/MultiAccountAlert", new { UserName = userName, AlertCount = alertCount, AccountAlertsHtml = accountAlertsHtml, TotalBalance = totalBalance.ToString("C"), AppUrl = _appUrl, Year = _currentYear });

    public Task<OperationResult> SendFinancialProjectionEmailAsync(string to, string userName, FinancialProjectionModel model) =>
        SendTemplateEmailAsync(to, "PowerUser/FinancialProjection", new
        {
            UserName = userName,
            model.ProjectionMonths,
            CurrentMonthlyAverage = model.CurrentMonthlyAverage.ToString("C"),
            model.TrendColor,
            model.TrendDirection,
            TrendPercentage = model.TrendPercentage.ToString("F1"),
            ProjectedNextMonth = model.ProjectedNextMonth.ToString("C"),
            ProjectedSavings = model.ProjectedSavings.ToString("C"),
            model.Recommendation,
            AppUrl = _appUrl,
            Year = _currentYear
        });

    public Task<OperationResult> SendPeriodComparisonEmailAsync(string to, string userName, PeriodComparisonModel model) =>
        SendTemplateEmailAsync(to, "PowerUser/PeriodComparison", new
        {
            UserName = userName,
            model.Period1Name,
            model.Period2Name,
            Period1Income = model.Period1Income.ToString("C"),
            Period2Income = model.Period2Income.ToString("C"),
            Period1Expenses = model.Period1Expenses.ToString("C"),
            Period2Expenses = model.Period2Expenses.ToString("C"),
            model.IncomeChangeColor,
            model.IncomeChangeIcon,
            IncomeChangePercentage = model.IncomeChangePercentage.ToString("F1"),
            model.ExpenseChangeColor,
            model.ExpenseChangeIcon,
            ExpenseChangePercentage = model.ExpenseChangePercentage.ToString("F1"),
            model.BalanceChangeColor,
            model.BalanceChangeIcon,
            BalanceChange = model.BalanceChange.ToString("C"),
            AppUrl = _appUrl,
            Year = _currentYear
        });

    public Task<OperationResult> SendExportCompletedEmailAsync(string to, string userName, string fileName, string fileSize, string exportType, string dateRange, string downloadUrl, int expirationHours)
    {
        var fileIcon = exportType.ToLower() switch
        {
            "excel" => "📊",
            "pdf" => "📄",
            "csv" => "📑",
            _ => "📁"
        };
        return SendTemplateEmailAsync(to, "PowerUser/ExportCompleted", new
        {
            UserName = userName,
            FileName = fileName,
            FileSize = fileSize,
            ExportType = exportType,
            DateRange = dateRange,
            DownloadUrl = downloadUrl,
            ExpirationHours = expirationHours,
            FileIcon = fileIcon,
            GeneratedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"),
            AppUrl = _appUrl,
            Year = _currentYear
        });
    }

    #endregion

    #region System Emails

    public Task<OperationResult> SendEmailVerificationAsync(string to, string userName, string verificationUrl) =>
        SendTemplateEmailAsync(to, "System/EmailVerification", new { UserName = userName, VerificationUrl = verificationUrl, AppUrl = _appUrl, Year = _currentYear });

    public Task<OperationResult> SendPasswordResetEmailAsync(string to, string userName, string resetUrl, int expirationMinutes) =>
        SendTemplateEmailAsync(to, "System/PasswordReset", new { UserName = userName, ResetUrl = resetUrl, ExpirationMinutes = expirationMinutes, Year = _currentYear });

    public Task<OperationResult> SendPasswordChangedEmailAsync(string to, string userName, DateTime changeDate, string ipAddress) =>
        SendTemplateEmailAsync(to, "System/PasswordChanged", new { UserName = userName, ChangeDate = changeDate.ToString("yyyy-MM-dd HH:mm:ss UTC"), IpAddress = ipAddress, SupportUrl = $"{_appUrl}/support", Year = _currentYear });

    public Task<OperationResult> SendProfileUpdatedEmailAsync(string to, string userName, string changesHtml, DateTime updateDate, string deviceInfo) =>
        SendTemplateEmailAsync(to, "System/ProfileUpdated", new { UserName = userName, ChangesHtml = changesHtml, UpdateDate = updateDate.ToString("yyyy-MM-dd HH:mm:ss UTC"), DeviceInfo = deviceInfo, AppUrl = _appUrl, Year = _currentYear });

    public Task<OperationResult> SendAccountDeletionEmailAsync(string to, string userName, int daysUntilDeletion, string cancelDeletionUrl) =>
        SendTemplateEmailAsync(to, "System/AccountDeletion", new { UserName = userName, DaysUntilDeletion = daysUntilDeletion, CancelDeletionUrl = cancelDeletionUrl, Year = _currentYear });

    public Task<OperationResult> SendSyncErrorEmailAsync(string to, string userName, string bankName, string errorMessage) =>
        SendTemplateEmailAsync(to, "System/SyncError", new { UserName = userName, BankName = bankName, ErrorMessage = errorMessage, AppUrl = _appUrl, Year = _currentYear });

    public Task<OperationResult> SendMaintenanceNoticeEmailAsync(string to, string userName, DateTime maintenanceStart, int durationMinutes, string timeZone, string improvementsSummary) =>
        SendTemplateEmailAsync(to, "System/MaintenanceNotice", new
        {
            UserName = userName,
            MaintenanceDate = maintenanceStart.ToString("dddd, MMMM dd, yyyy"),
            MaintenanceTime = maintenanceStart.ToString("HH:mm"),
            DurationMinutes = durationMinutes,
            TimeZone = timeZone,
            ImprovementsSummary = improvementsSummary,
            Year = _currentYear
        });

    #endregion

    #region Private Helpers

    private SmtpClient CreateSmtpClient()
    {
        return new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
        {
            Credentials = new NetworkCredential(_settings.Username, _settings.Password),
            EnableSsl = _settings.EnableSsl,
            Timeout = 30000
        };
    }

    private MailMessage CreateMailMessage(string to, string subject, string body)
    {
        return new MailMessage
        {
            From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
            To = { to },
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
    }

    private static string ReplaceTokens(string template, object model)
    {
        var result = template;
        var properties = model.GetType().GetProperties();

        foreach (var prop in properties)
        {
            var token = $"{{{{{prop.Name}}}}}";
            var value = prop.GetValue(model)?.ToString() ?? string.Empty;
            result = result.Replace(token, value);
        }

        return result;
    }

    private static string GetSubjectFromTemplate(string templateName)
    {
        var name = Path.GetFileNameWithoutExtension(templateName);
        return name switch
        {
            // Casual
            "WelcomeCasual" => "👋 Bienvenido a ManageMyMoney",
            "FirstExpenseRegistered" => "🎉 ¡Primer gasto registrado!",
            "WeeklyAlertSimple" => "📊 Tu resumen semanal",
            "RegistrationReminder" => "⏰ ¿Olvidaste registrar tus gastos?",
            "WeeklySummarySimple" => "📅 Resumen de tu semana",
            
            // Organized
            "WelcomeOrganized" => "🎯 Bienvenido a ManageMyMoney",
            "BudgetCreated" => "✅ Presupuesto creado",
            "BudgetAlert80" => "⚠️ Alerta: 80% del presupuesto usado",
            "BudgetExceeded" => "🚨 Presupuesto excedido",
            "MonthlySummary" => "📊 Tu resumen mensual",
            "GoalAchieved" => "🎉 ¡Meta alcanzada!",
            "GoalAtRisk" => "⚡ Tu meta necesita atención",
            
            // Power User
            "WelcomePowerUser" => "⚡ Bienvenido, Power User",
            "CustomReport" => "📊 Tu reporte personalizado",
            "MultiAccountAlert" => "🏦 Resumen de alertas",
            "FinancialProjection" => "📈 Tu proyección financiera",
            "PeriodComparison" => "📊 Comparativa de períodos",
            "ExportCompleted" => "✅ Exportación completada",
            
            // System
            "EmailVerification" => "✉️ Verifica tu email",
            "PasswordReset" => "🔐 Restablecer contraseña",
            "PasswordChanged" => "✅ Contraseña actualizada",
            "ProfileUpdated" => "👤 Perfil actualizado",
            "AccountDeletion" => "⚠️ Eliminación de cuenta programada",
            "SyncError" => "⚡ Error de sincronización",
            "MaintenanceNotice" => "🔧 Mantenimiento programado",
            
            _ => "Notificación de ManageMyMoney"
        };
    }

    #endregion
}
