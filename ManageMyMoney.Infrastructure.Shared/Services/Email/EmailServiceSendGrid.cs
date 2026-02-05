using SendGrid;
using SendGrid.Helpers.Mail;
using ManageMyMoney.Core.Application.Common.Interfaces;
using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Infrastructure.Shared.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ManageMyMoney.Infrastructure.Shared.Services.Email;

public class EmailServiceSendGrid : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailServiceSendGrid> _logger;
    private readonly string _appUrl;
    private readonly int _currentYear;
    private readonly string _templatesBasePath;
    private readonly bool _isConfigured;
    private readonly ISendGridClient? _sendGridClient;

    public EmailServiceSendGrid(IOptions<EmailSettings> settings, ILogger<EmailServiceSendGrid> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _appUrl = "https://app.managemymoney.com";
        _currentYear = DateTime.UtcNow.Year;
        
        // Resolve templates path relative to application base directory
        var baseDir = AppContext.BaseDirectory;
        _templatesBasePath = Path.Combine(baseDir, _settings.TemplatesPath);
        
        // Validate email configuration
        _isConfigured = ValidateConfiguration();
        
        // Initialize SendGrid client
        if (_isConfigured)
        {
            _sendGridClient = new SendGridClient(_settings.Password); // Password contains the API Key
            _logger.LogInformation("✅ SendGrid API client initialized successfully. From: {Sender}", _settings.SenderEmail);
        }
        else
        {
            _logger.LogWarning("⚠️ Email service is NOT configured. Set SENDGRID_API_KEY environment variable.");
        }
        
        _logger.LogInformation("Email templates base path: {Path}", _templatesBasePath);
    }

    #region Base Methods

    public async Task<OperationResult> SendEmailAsync(string to, string subject, string body)
    {
        if (!_isConfigured || _sendGridClient == null)
        {
            _logger.LogWarning("📧 Email not sent to {Recipient} - SendGrid not configured", to);
            return OperationResult.Success();
        }

        try
        {
            _logger.LogInformation("📤 Sending email via SendGrid API to {Recipient}: {Subject}", to, subject);
            
            var from = new EmailAddress(_settings.SenderEmail, _settings.SenderName);
            var toAddress = new EmailAddress(to);
            var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, null, body);
            
            var response = await _sendGridClient.SendEmailAsync(msg);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("✅ Email sent successfully to {Recipient} via SendGrid", to);
                return OperationResult.Success();
            }
            else
            {
                var responseBody = await response.Body.ReadAsStringAsync();
                _logger.LogError("❌ SendGrid API error: StatusCode={StatusCode}, Body={Body}", 
                    response.StatusCode, responseBody);
                return OperationResult.Success(); // Still return success to not block user flow
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Exception sending email via SendGrid to {Recipient}", to);
            return OperationResult.Success();
        }
    }

    public async Task<OperationResult> SendTemplateEmailAsync(string to, string templateName, object model)
    {
        try
        {
            var templatePath = Path.Combine(_templatesBasePath, $"{templateName}.html");
            if (!File.Exists(templatePath))
            {
                _logger.LogWarning("Email template not found: {Template} at {Path}", templateName, templatePath);
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

    #endregion

    #region Casual Templates

    public Task<OperationResult> SendWelcomeCasualEmailAsync(string to, string userName) =>
        SendTemplateEmailAsync(to, "Casual/WelcomeCasual", new { UserName = userName, AppUrl = _appUrl, Year = _currentYear });

    public Task<OperationResult> SendFirstExpenseRegisteredEmailAsync(string to, string userName, decimal amount, string description, string category) =>
        SendTemplateEmailAsync(to, "Casual/FirstExpenseRegistered", new { UserName = userName, Amount = amount, Description = description, Category = category, AppUrl = _appUrl, Year = _currentYear });

    public Task<OperationResult> SendWeeklyAlertSimpleEmailAsync(string to, string userName, decimal weeklyTotal, int expenseCount, decimal dailyAverage, decimal highestExpense) =>
        SendTemplateEmailAsync(to, "Casual/WeeklyAlertSimple", new { UserName = userName, WeeklyTotal = weeklyTotal, ExpenseCount = expenseCount, DailyAverage = dailyAverage, HighestExpense = highestExpense, AppUrl = _appUrl, Year = _currentYear });

    public Task<OperationResult> SendRegistrationReminderEmailAsync(string to, string userName, int daysSinceLastExpense) =>
        SendTemplateEmailAsync(to, "Casual/RegistrationReminder", new { UserName = userName, DaysSinceLastExpense = daysSinceLastExpense, AppUrl = _appUrl, Year = _currentYear });

    public Task<OperationResult> SendWeeklySummarySimpleEmailAsync(string to, string userName, DateTime weekStart, DateTime weekEnd, decimal weeklyTotal, int expenseCount, string weeklyMessage) =>
        SendTemplateEmailAsync(to, "Casual/WeeklySummarySimple", new { UserName = userName, WeekStart = weekStart.ToString("MMM dd"), WeekEnd = weekEnd.ToString("MMM dd, yyyy"), WeeklyTotal = weeklyTotal, ExpenseCount = expenseCount, WeeklyMessage = weeklyMessage, AppUrl = _appUrl, Year = _currentYear });

    #endregion

    #region Organized Templates

    public Task<OperationResult> SendWelcomeOrganizedEmailAsync(string to, string userName) =>
        SendTemplateEmailAsync(to, "Organized/WelcomeOrganized", new { UserName = userName, Year = _currentYear });

    public Task<OperationResult> SendBudgetCreatedEmailAsync(string to, string userName, string budgetName, decimal limit, string period, Guid budgetId) =>
        SendTemplateEmailAsync(to, "Organized/BudgetCreated", new { UserName = userName, BudgetName = budgetName, Limit = limit, Period = period, BudgetId = budgetId, AppUrl = _appUrl, Year = _currentYear });

    public Task<OperationResult> SendBudgetAlert80EmailAsync(string to, string userName, string budgetName, decimal limit, decimal current, decimal remaining, decimal percentageUsed, int daysRemaining, Guid budgetId) =>
        SendTemplateEmailAsync(to, "Organized/BudgetAlert80", new { UserName = userName, BudgetName = budgetName, Limit = limit, Current = current, Remaining = remaining, PercentageUsed = percentageUsed, DaysRemaining = daysRemaining, BudgetId = budgetId, AppUrl = _appUrl, Year = _currentYear });

    public Task<OperationResult> SendBudgetExceededEmailAsync(string to, string userName, string budgetName, decimal limit, decimal current, decimal excess, int daysRemaining, Guid budgetId) =>
        SendTemplateEmailAsync(to, "Organized/BudgetExceeded", new { UserName = userName, BudgetName = budgetName, Limit = limit, Current = current, Excess = excess, DaysRemaining = daysRemaining, BudgetId = budgetId, AppUrl = _appUrl, Year = _currentYear });

    public Task<OperationResult> SendMonthlySummaryEmailAsync(string to, string userName, MonthlySummaryModel model) =>
        SendTemplateEmailAsync(to, "Organized/MonthlySummary", new
        {
            UserName = userName,
            Year = model.Year,
            Month = model.Month,
            MonthName = model.MonthName,
            TotalIncome = model.TotalIncome,
            TotalExpenses = model.TotalExpenses,
            NetBalance = model.NetBalance,
            SavingsRate = model.SavingsRate,
            Currency = model.Currency,
            TopCategoriesHtml = model.TopCategoriesHtml,
            ComparisonColor = model.ComparisonColor,
            ComparisonIcon = model.ComparisonIcon,
            ComparisonText = model.ComparisonText,
            AppUrl = _appUrl,
            CurrentYear = _currentYear
        });

    public Task<OperationResult> SendGoalAchievedEmailAsync(string to, string userName, string goalName, decimal targetAmount, int daysToComplete, int contributionsCount, decimal averageContribution) =>
        SendTemplateEmailAsync(to, "Organized/GoalAchieved", new { UserName = userName, GoalName = goalName, TargetAmount = targetAmount, DaysToComplete = daysToComplete, ContributionsCount = contributionsCount, AverageContribution = averageContribution, AppUrl = _appUrl, Year = _currentYear });

    public Task<OperationResult> SendGoalAtRiskEmailAsync(string to, string userName, string goalName, decimal targetAmount, decimal currentAmount, decimal progressPercentage, DateTime targetDate, int daysRemaining, decimal requiredMonthly, Guid goalId) =>
        SendTemplateEmailAsync(to, "Organized/GoalAtRisk", new { UserName = userName, GoalName = goalName, TargetAmount = targetAmount, CurrentAmount = currentAmount, ProgressPercentage = progressPercentage, TargetDate = targetDate.ToString("MMM dd, yyyy"), DaysRemaining = daysRemaining, RequiredMonthly = requiredMonthly, GoalId = goalId, AppUrl = _appUrl, Year = _currentYear });

    #endregion

    #region Power User Templates

    public Task<OperationResult> SendWelcomePowerUserEmailAsync(string to, string userName) =>
        SendTemplateEmailAsync(to, "PowerUser/WelcomePowerUser", new { UserName = userName, Year = _currentYear });

    public Task<OperationResult> SendCustomReportEmailAsync(string to, string userName, string reportName, string dateRange, string reportSummaryHtml, string downloadUrl) =>
        SendTemplateEmailAsync(to, "PowerUser/CustomReport", new { UserName = userName, ReportName = reportName, DateRange = dateRange, ReportSummaryHtml = reportSummaryHtml, DownloadUrl = downloadUrl, Year = _currentYear });

    public Task<OperationResult> SendMultiAccountAlertEmailAsync(string to, string userName, int alertCount, string accountAlertsHtml, decimal totalBalance) =>
        SendTemplateEmailAsync(to, "PowerUser/MultiAccountAlert", new { UserName = userName, AlertCount = alertCount, AccountAlertsHtml = accountAlertsHtml, TotalBalance = totalBalance, AppUrl = _appUrl, Year = _currentYear });

    public Task<OperationResult> SendFinancialProjectionEmailAsync(string to, string userName, FinancialProjectionModel model) =>
        SendTemplateEmailAsync(to, "PowerUser/FinancialProjection", new
        {
            UserName = userName,
            ProjectionMonths = model.ProjectionMonths,
            CurrentMonthlyAverage = model.CurrentMonthlyAverage,
            TrendColor = model.TrendColor,
            TrendDirection = model.TrendDirection,
            TrendPercentage = model.TrendPercentage,
            ProjectedNextMonth = model.ProjectedNextMonth,
            ProjectedSavings = model.ProjectedSavings,
            Recommendation = model.Recommendation,
            AppUrl = _appUrl,
            Year = _currentYear
        });

    public Task<OperationResult> SendPeriodComparisonEmailAsync(string to, string userName, PeriodComparisonModel model) =>
        SendTemplateEmailAsync(to, "PowerUser/PeriodComparison", new
        {
            UserName = userName,
            Period1Name = model.Period1Name,
            Period2Name = model.Period2Name,
            Period1Income = model.Period1Income,
            Period2Income = model.Period2Income,
            Period1Expenses = model.Period1Expenses,
            Period2Expenses = model.Period2Expenses,
            IncomeChangeColor = model.IncomeChangeColor,
            IncomeChangeIcon = model.IncomeChangeIcon,
            IncomeChangePercentage = model.IncomeChangePercentage,
            ExpenseChangeColor = model.ExpenseChangeColor,
            ExpenseChangeIcon = model.ExpenseChangeIcon,
            ExpenseChangePercentage = model.ExpenseChangePercentage,
            BalanceChangeColor = model.BalanceChangeColor,
            BalanceChangeIcon = model.BalanceChangeIcon,
            BalanceChange = model.BalanceChange,
            AppUrl = _appUrl,
            Year = _currentYear
        });

    public Task<OperationResult> SendExportCompletedEmailAsync(string to, string userName, string fileName, string fileSize, string exportType, string dateRange, string downloadUrl, int expirationHours) =>
        SendTemplateEmailAsync(to, "PowerUser/ExportCompleted", new { UserName = userName, FileName = fileName, FileSize = fileSize, ExportType = exportType, DateRange = dateRange, DownloadUrl = downloadUrl, ExpirationHours = expirationHours, Year = _currentYear });

    #endregion

    #region System Templates

    public Task<OperationResult> SendEmailVerificationAsync(string to, string userName, string verificationCode, string verificationUrl, int expirationMinutes) =>
        SendTemplateEmailAsync(to, "System/EmailVerification", new { UserName = userName, VerificationCode = verificationCode, VerificationUrl = verificationUrl, ExpirationMinutes = expirationMinutes, Year = _currentYear });

    public Task<OperationResult> SendPasswordResetEmailAsync(string to, string userName, string resetUrl, int expirationMinutes) =>
        SendTemplateEmailAsync(to, "System/PasswordReset", new { UserName = userName, ResetUrl = resetUrl, ExpirationMinutes = expirationMinutes, Year = _currentYear });

    public Task<OperationResult> SendPasswordChangedEmailAsync(string to, string userName, DateTime changeDate, string ipAddress) =>
        SendTemplateEmailAsync(to, "System/PasswordChanged", new { UserName = userName, ChangeDate = changeDate.ToString("MMM dd, yyyy HH:mm"), IpAddress = ipAddress, AppUrl = _appUrl, Year = _currentYear });

    public Task<OperationResult> SendProfileUpdatedEmailAsync(string to, string userName, string changesHtml, DateTime updateDate, string deviceInfo) =>
        SendTemplateEmailAsync(to, "System/ProfileUpdated", new { UserName = userName, ChangesHtml = changesHtml, UpdateDate = updateDate.ToString("MMM dd, yyyy HH:mm"), DeviceInfo = deviceInfo, AppUrl = _appUrl, Year = _currentYear });

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

    #region Additional Methods

    public async Task<OperationResult> SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string body)
    {
        if (!_isConfigured || _sendGridClient == null)
        {
            _logger.LogWarning("📧 Bulk email not sent - SendGrid not configured");
            return OperationResult.Success();
        }

        try
        {
            var tasks = recipients.Select(recipient => SendEmailAsync(recipient, subject, body));
            await Task.WhenAll(tasks);
            
            _logger.LogInformation("✅ Bulk email sent to {Count} recipients", recipients.Count());
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Exception sending bulk email");
            return OperationResult.Success();
        }
    }

    #endregion

    #region Private Helpers

    private bool ValidateConfiguration()
    {
        var isValid = !string.IsNullOrWhiteSpace(_settings.SenderEmail) &&
                     !string.IsNullOrWhiteSpace(_settings.Password);

        if (!isValid)
        {
            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(_settings.SenderEmail)) missing.Add("SENDER_EMAIL");
            if (string.IsNullOrWhiteSpace(_settings.Password)) missing.Add("SENDGRID_API_KEY (or EMAIL_PASSWORD)");

            _logger.LogWarning("Email configuration incomplete. Missing: {MissingVars}", string.Join(", ", missing));
        }
        else
        {
            // Validate SendGrid API Key format
            if (!_settings.Password.StartsWith("SG."))
            {
                _logger.LogWarning("⚠️ API Key doesn't start with 'SG.' - may be incorrect");
            }
            _logger.LogInformation("SendGrid API Key validated (length: {Length})", _settings.Password.Length);
        }

        return isValid;
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
            "WelcomeCasual" => "👋 Bienvenido a ManageMyMoney",
            "FirstExpenseRegistered" => "🎉 ¡Primer gasto registrado!",
            "WeeklyAlertSimple" => "📊 Tu resumen semanal",
            "RegistrationReminder" => "⏰ ¿Olvidaste registrar tus gastos?",
            "WeeklySummarySimple" => "📅 Resumen de tu semana",
            "WelcomeOrganized" => "🎯 Bienvenido a ManageMyMoney",
            "BudgetCreated" => "✅ Presupuesto creado",
            "BudgetAlert80" => "⚠️ Alerta: 80% del presupuesto usado",
            "BudgetExceeded" => "🚨 Presupuesto excedido",
            "MonthlySummary" => "📊 Tu resumen mensual",
            "GoalAchieved" => "🎉 ¡Meta alcanzada!",
            "GoalAtRisk" => "⚡ Tu meta necesita atención",
            "WelcomePowerUser" => "⚡ Bienvenido, Power User",
            "CustomReport" => "📊 Tu reporte personalizado",
            "MultiAccountAlert" => "🏦 Resumen de alertas",
            "FinancialProjection" => "📈 Tu proyección financiera",
            "PeriodComparison" => "📊 Comparativa de períodos",
            "ExportCompleted" => "✅ Exportación completada",
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
