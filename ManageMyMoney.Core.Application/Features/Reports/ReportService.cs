using ManageMyMoney.Core.Application.DTOs.Reports;
using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace ManageMyMoney.Core.Application.Features.Reports;

public class ReportService : IReportService
{
    private readonly ILogger<ReportService> _logger;
    private readonly IExpenseRepository _expenseRepository;
    private readonly IIncomeRepository _incomeRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBudgetRepository _budgetRepository;

    public ReportService(
        ILogger<ReportService> logger,
        IExpenseRepository expenseRepository,
        IIncomeRepository incomeRepository, 
        IAccountRepository accountRepository,
        ICategoryRepository categoryRepository,
        IBudgetRepository budgetRepository)
    {
        _logger = logger;
        _expenseRepository = expenseRepository;
        _incomeRepository = incomeRepository;
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
        _budgetRepository = budgetRepository;
    }

    public async Task<OperationResult<FinancialSummaryResponse>> GetFinancialSummaryAsync(Guid userId, DateTime fromDate, DateTime toDate)
    {
        try
        {
            _logger.LogInformation("Getting financial summary for user {UserId} from {FromDate} to {ToDate}", 
                userId, fromDate, toDate);

            var dateRangeResult = ManageMyMoney.Core.Domain.ValueObjects.DateRange.Create(fromDate, toDate);
            if (dateRangeResult.IsFailure)
                return OperationResult.Failure<FinancialSummaryResponse>(dateRangeResult.Error);
            
            var dateRange = dateRangeResult.Value!;
            
            var totalIncomeResult = await _incomeRepository.GetTotalByUserAndDateRangeAsync(userId, dateRange);
            var totalExpensesResult = await _expenseRepository.GetTotalByUserAndDateRangeAsync(userId, dateRange);

            var totalIncome = totalIncomeResult.Value;
            var totalExpenses = totalExpensesResult.Value;
            var netBalance = totalIncome - totalExpenses;
            var savingsRate = totalIncome > 0 ? (netBalance / totalIncome) * 100 : 0;

            var response = new FinancialSummaryResponse
            {
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                NetBalance = netBalance,
                SavingsRate = savingsRate,
                Currency = "DOP", // Default for now
                FromDate = fromDate,
                ToDate = toDate
            };

            return OperationResult.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting financial summary for user {UserId}", userId);
            return OperationResult.Failure<FinancialSummaryResponse>("An error occurred while retrieving the financial summary");
        }
    }

    public async Task<OperationResult<MonthlyReportResponse>> GetMonthlyReportAsync(Guid userId, int year, int month)
    {
        try
        {
            _logger.LogInformation("Getting monthly report for user {UserId}, year {Year}, month {Month}", 
                userId, year, month);

            var monthName = new DateTime(year, month, 1).ToString("MMMM", CultureInfo.InvariantCulture);
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var dateRangeResult = ManageMyMoney.Core.Domain.ValueObjects.DateRange.Create(startDate, endDate);
            if (dateRangeResult.IsFailure)
                return OperationResult.Failure<MonthlyReportResponse>(dateRangeResult.Error);
            
            var dateRange = dateRangeResult.Value!;

            var expensesResult = await _expenseRepository.GetByUserAndDateRangeAsync(userId, dateRange);
            var incomeResult = await _incomeRepository.GetByUserAndDateRangeAsync(userId, dateRange);

            var expenses = expensesResult.Value ?? Enumerable.Empty<ManageMyMoney.Core.Domain.Entities.Expenses.Expense>();
            var incomes = incomeResult.Value ?? Enumerable.Empty<ManageMyMoney.Core.Domain.Entities.Income.Income>();

            var totalExpenses = expenses.Sum(e => e.Amount.Amount);
            var totalIncome = incomes.Sum(i => i.Amount.Amount);
            var netBalance = totalIncome - totalExpenses;
            var savingsRate = totalIncome > 0 ? (netBalance / totalIncome) * 100 : 0;

            var expensesByCategory = expenses
                .GroupBy(e => e.CategoryId)
                .Select(g => new CategoryBreakdownItem
                {
                    CategoryId = g.Key,
                    CategoryName = g.First().Category?.Name ?? "Unknown",
                    Amount = g.Sum(e => e.Amount.Amount),
                    Percentage = totalExpenses > 0 ? (g.Sum(e => e.Amount.Amount) / totalExpenses) * 100 : 0,
                    TransactionCount = g.Count()
                })
                .OrderByDescending(c => c.Amount)
                .ToList();

            var incomeBySource = incomes
                .GroupBy(i => i.IncomeSourceId)
                .Select(g => new IncomeSourceBreakdownItem
                {
                    SourceId = g.Key,
                    SourceName = "Income Source", // Source navigation property missing in entity
                    Amount = g.Sum(i => i.Amount.Amount),
                    Percentage = totalIncome > 0 ? (g.Sum(i => i.Amount.Amount) / totalIncome) * 100 : 0
                })
                .OrderByDescending(i => i.Amount)
                .ToList();

            var response = new MonthlyReportResponse
            {
                Year = year,
                Month = month,
                MonthName = monthName,
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                NetBalance = netBalance,
                SavingsRate = savingsRate,
                Currency = "DOP",
                ExpensesByCategory = expensesByCategory,
                IncomeBySource = incomeBySource,
                DailyBalance = new List<DailyBalanceItem>(), // Simplified
                ComparedToPreviousMonth = 0
            };

            return OperationResult.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting monthly report for user {UserId}, year {Year}, month {Month}", 
                userId, year, month);
            return OperationResult.Failure<MonthlyReportResponse>("An error occurred while retrieving the monthly report");
        }
    }

    public async Task<OperationResult<YearlyReportResponse>> GetYearlyReportAsync(Guid userId, int year)
    {
        try
        {
            _logger.LogInformation("Getting yearly report for user {UserId}, year {Year}", userId, year);

            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31);
            var dateRangeResult = ManageMyMoney.Core.Domain.ValueObjects.DateRange.Create(startDate, endDate);
            if (dateRangeResult.IsFailure)
                return OperationResult.Failure<YearlyReportResponse>(dateRangeResult.Error);
            
            var dateRange = dateRangeResult.Value!;

            var expensesResult = await _expenseRepository.GetByUserAndDateRangeAsync(userId, dateRange);
            var incomeResult = await _incomeRepository.GetByUserAndDateRangeAsync(userId, dateRange);

            var expenses = expensesResult.Value ?? Enumerable.Empty<ManageMyMoney.Core.Domain.Entities.Expenses.Expense>();
            var incomes = incomeResult.Value ?? Enumerable.Empty<ManageMyMoney.Core.Domain.Entities.Income.Income>();

            var totalExpenses = expenses.Sum(e => e.Amount.Amount);
            var totalIncome = incomes.Sum(i => i.Amount.Amount);
            var netBalance = totalIncome - totalExpenses;

            var monthlyTrends = new List<MonthlyTrendItem>();
            for (int m = 1; m <= 12; m++)
            {
                var monthExpenses = expenses.Where(e => e.Date.Month == m).Sum(e => e.Amount.Amount);
                var monthIncome = incomes.Where(i => i.Date.Month == m).Sum(i => i.Amount.Amount);
                monthlyTrends.Add(new MonthlyTrendItem
                {
                    Month = m,
                    MonthName = new DateTime(year, m, 1).ToString("MMMM", CultureInfo.InvariantCulture),
                    Income = monthIncome,
                    Expenses = monthExpenses,
                    Balance = monthIncome - monthExpenses
                });
            }

            var response = new YearlyReportResponse
            {
                Year = year,
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                NetBalance = netBalance,
                AverageMonthlySavings = netBalance / 12,
                Currency = "DOP",
                MonthlyTrends = monthlyTrends,
                ExpensesByCategory = new List<CategoryBreakdownItem>() // Simplified
            };

            return OperationResult.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting yearly report for user {UserId}, year {Year}", userId, year);
            return OperationResult.Failure<YearlyReportResponse>("An error occurred while retrieving the yearly report");
        }
    }

    public async Task<OperationResult<ComparisonReportResponse>> GetComparisonReportAsync(Guid userId, ComparisonReportRequest request)
    {
        try
        {
            _logger.LogInformation("Getting comparison report for user {UserId}", userId);

            var range1Result = ManageMyMoney.Core.Domain.ValueObjects.DateRange.Create(request.Period1Start, request.Period1End);
            var range2Result = ManageMyMoney.Core.Domain.ValueObjects.DateRange.Create(request.Period2Start, request.Period2End);
            
            if (range1Result.IsFailure) return OperationResult.Failure<ComparisonReportResponse>(range1Result.Error);
            if (range2Result.IsFailure) return OperationResult.Failure<ComparisonReportResponse>(range2Result.Error);

            var range1 = range1Result.Value!;
            var range2 = range2Result.Value!;

            var income1Result = await _incomeRepository.GetByUserAndDateRangeAsync(userId, range1);
            var expense1Result = await _expenseRepository.GetByUserAndDateRangeAsync(userId, range1);
            
            var income2Result = await _incomeRepository.GetByUserAndDateRangeAsync(userId, range2);
            var expense2Result = await _expenseRepository.GetByUserAndDateRangeAsync(userId, range2);

            var income1 = income1Result.Value?.Sum(i => i.Amount.Amount) ?? 0;
            var expense1 = expense1Result.Value?.Sum(e => e.Amount.Amount) ?? 0;
            var income2 = income2Result.Value?.Sum(i => i.Amount.Amount) ?? 0;
            var expense2 = expense2Result.Value?.Sum(e => e.Amount.Amount) ?? 0;

            var incomeChange = income2 - income1;
            var expenseChange = expense2 - expense1;

            var response = new ComparisonReportResponse
            {
                Period1 = new PeriodSummary
                {
                    StartDate = request.Period1Start,
                    EndDate = request.Period1End,
                    TotalIncome = income1,
                    TotalExpenses = expense1,
                    Balance = income1 - expense1
                },
                Period2 = new PeriodSummary
                {
                    StartDate = request.Period2Start,
                    EndDate = request.Period2End,
                    TotalIncome = income2,
                    TotalExpenses = expense2,
                    Balance = income2 - expense2
                },
                IncomeChange = incomeChange,
                IncomeChangePercentage = income1 > 0 ? (incomeChange / income1) * 100 : 0,
                ExpenseChange = expenseChange,
                ExpenseChangePercentage = expense1 > 0 ? (expenseChange / expense1) * 100 : 0,
                BalanceChange = (income2 - expense2) - (income1 - expense1),
                CategoryComparison = new List<CategoryComparisonItem>()
            };

            return OperationResult.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comparison report for user {UserId}", userId);
            return OperationResult.Failure<ComparisonReportResponse>("An error occurred while retrieving the comparison report");
        }
    }

    public async Task<OperationResult<ComparisonReportResponse>> GetMonthOverMonthComparisonAsync(Guid userId, int year, int month)
    {
        try
        {
            _logger.LogInformation("Getting month over month comparison for user {UserId}, year {Year}, month {Month}", 
                userId, year, month);

            var currentMonthStart = new DateTime(year, month, 1);
            var currentMonthEnd = currentMonthStart.AddMonths(1).AddDays(-1);
            var previousMonthStart = currentMonthStart.AddMonths(-1);
            var previousMonthEnd = previousMonthStart.AddMonths(1).AddDays(-1);

            var request = new ComparisonReportRequest
            {
                Period1Start = previousMonthStart,
                Period1End = previousMonthEnd,
                Period2Start = currentMonthStart,
                Period2End = currentMonthEnd
            };

            return await GetComparisonReportAsync(userId, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting month over month comparison for user {UserId}", userId);
            return OperationResult.Failure<ComparisonReportResponse>("An error occurred while retrieving the month over month comparison");
        }
    }

    public async Task<OperationResult<ComparisonReportResponse>> GetYearOverYearComparisonAsync(Guid userId, int year)
    {
        try
        {
            _logger.LogInformation("Getting year over year comparison for user {UserId}, year {Year}", userId, year);

            var currentYearStart = new DateTime(year, 1, 1);
            var currentYearEnd = new DateTime(year, 12, 31);
            var previousYearStart = new DateTime(year - 1, 1, 1);
            var previousYearEnd = new DateTime(year - 1, 12, 31);

            var request = new ComparisonReportRequest
            {
                Period1Start = previousYearStart,
                Period1End = previousYearEnd,
                Period2Start = currentYearStart,
                Period2End = currentYearEnd
            };

            return await GetComparisonReportAsync(userId, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting year over year comparison for user {UserId}", userId);
            return OperationResult.Failure<ComparisonReportResponse>("An error occurred while retrieving the year over year comparison");
        }
    }

    public async Task<OperationResult<IEnumerable<BudgetPerformanceResponse>>> GetBudgetPerformanceAsync(Guid userId)
    {
        try
        {
            var budgetsResult = await _budgetRepository.GetActiveByUserAsync(userId);
            if (budgetsResult.IsFailure)
                return OperationResult.Failure<IEnumerable<BudgetPerformanceResponse>>(budgetsResult.Error);

            var performanceList = new List<BudgetPerformanceResponse>();
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            var dateRangeResult = ManageMyMoney.Core.Domain.ValueObjects.DateRange.Create(startOfMonth, endOfMonth);
            if (dateRangeResult.IsFailure)
                return OperationResult.Failure<IEnumerable<BudgetPerformanceResponse>>(dateRangeResult.Error);
            
            var dateRange = dateRangeResult.Value!;

            foreach (var budget in budgetsResult.Value!)
            {
                // Budget uses CategoryIds collection
                decimal spent = 0;
                foreach (var categoryId in budget.CategoryIds)
                {
                    var spentResult = await _expenseRepository.GetTotalByCategoryAndDateRangeAsync(categoryId, dateRange);
                    spent += spentResult.Value;
                }
                
                var performance = budget.Limit.Amount > 0 ? (spent / budget.Limit.Amount) * 100 : 0;
                
                string status = "UnderBudget";
                if (performance >= 100) status = "OverBudget";
                else if (performance >= 90) status = "NearLimit";

                performanceList.Add(new BudgetPerformanceResponse
                {
                    BudgetName = budget.Name,
                    AllocatedAmount = budget.Limit.Amount,
                    SpentAmount = spent,
                    RemainingAmount = Math.Max(0, budget.Limit.Amount - spent),
                    PerformancePercentage = performance,
                    Status = status
                });
            }

            return OperationResult.Success<IEnumerable<BudgetPerformanceResponse>>(performanceList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting budget performance for user {UserId}", userId);
            return OperationResult.Failure<IEnumerable<BudgetPerformanceResponse>>("An error occurred while retrieving budget performance");
        }
    }

    public async Task<OperationResult<IEnumerable<MonthlyTrendItem>>> GetExpenseTrendsAsync(Guid userId, int months = 12)
    {
        try
        {
            _logger.LogInformation("Getting expense trends for user {UserId}, months: {Months}", userId, months);

            var today = DateTime.Today;
            var startDate = new DateTime(today.Year, today.Month, 1).AddMonths(-months + 1);
            var endDate = new DateTime(today.Year, today.Month, 1).AddMonths(1).AddDays(-1);
            var dateRangeResult = ManageMyMoney.Core.Domain.ValueObjects.DateRange.Create(startDate, endDate);
            if (dateRangeResult.IsFailure)
                return OperationResult.Failure<IEnumerable<MonthlyTrendItem>>(dateRangeResult.Error);
            
            var dateRange = dateRangeResult.Value!;

            var expensesResult = await _expenseRepository.GetByUserAndDateRangeAsync(userId, dateRange);
            var incomesResult = await _incomeRepository.GetByUserAndDateRangeAsync(userId, dateRange);

            var expenses = expensesResult.Value ?? Enumerable.Empty<ManageMyMoney.Core.Domain.Entities.Expenses.Expense>();
            var incomes = incomesResult.Value ?? Enumerable.Empty<ManageMyMoney.Core.Domain.Entities.Income.Income>();

            var trends = new List<MonthlyTrendItem>();

            for (int i = 0; i < months; i++)
            {
                var date = startDate.AddMonths(i);
                var monthExpenses = expenses.Where(e => e.Date.Month == date.Month && e.Date.Year == date.Year).Sum(e => e.Amount.Amount);
                var monthIncome = incomes.Where(i => i.Date.Month == date.Month && i.Date.Year == date.Year).Sum(i => i.Amount.Amount);
                
                trends.Add(new MonthlyTrendItem
                {
                    Month = date.Month,
                    MonthName = date.ToString("MMMM", CultureInfo.InvariantCulture),
                    Income = monthIncome,
                    Expenses = monthExpenses,
                    Balance = monthIncome - monthExpenses
                });
            }

            return OperationResult.Success<IEnumerable<MonthlyTrendItem>>(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expense trends for user {UserId}", userId);
            return OperationResult.Failure<IEnumerable<MonthlyTrendItem>>("An error occurred while retrieving expense trends");
        }
    }

    public async Task<OperationResult<IEnumerable<MonthlyTrendItem>>> GetIncomeTrendsAsync(Guid userId, int months = 12)
    {
        return await GetExpenseTrendsAsync(userId, months); // Both use the same logic for trends
    }

    public async Task<OperationResult<IEnumerable<CategoryBreakdownItem>>> GetTopExpenseCategoriesAsync(Guid userId, DateTime fromDate, DateTime toDate, int top = 5)
    {
        try
        {
            _logger.LogInformation("Getting top {Top} expense categories for user {UserId} from {FromDate} to {ToDate}", 
                top, userId, fromDate, toDate);

            var dateRangeResult = ManageMyMoney.Core.Domain.ValueObjects.DateRange.Create(fromDate, toDate);
            if (dateRangeResult.IsFailure)
                return OperationResult.Failure<IEnumerable<CategoryBreakdownItem>>(dateRangeResult.Error);
            
            var dateRange = dateRangeResult.Value!;
            var expensesResult = await _expenseRepository.GetByUserAndDateRangeAsync(userId, dateRange);
            
            if (expensesResult.IsFailure)
                return OperationResult.Failure<IEnumerable<CategoryBreakdownItem>>(expensesResult.Error);

            var totalExpenses = expensesResult.Value?.Sum(e => e.Amount.Amount) ?? 0;

            var topCategories = expensesResult.Value?
                .GroupBy(e => e.CategoryId)
                .Select(g => new CategoryBreakdownItem
                {
                    CategoryId = g.Key,
                    CategoryName = g.First().Category?.Name ?? "Unknown",
                    Amount = g.Sum(e => e.Amount.Amount),
                    Percentage = totalExpenses > 0 ? (g.Sum(e => e.Amount.Amount) / totalExpenses) * 100 : 0,
                    TransactionCount = g.Count()
                })
                .OrderByDescending(c => c.Amount)
                .Take(top)
                .ToList() ?? new List<CategoryBreakdownItem>();

            return OperationResult.Success<IEnumerable<CategoryBreakdownItem>>(topCategories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top expense categories for user {UserId}", userId);
            return OperationResult.Failure<IEnumerable<CategoryBreakdownItem>>("An error occurred while retrieving top expense categories");
        }
    }

    public async Task<OperationResult<byte[]>> ExportReportAsync(Guid userId, ExportReportRequest request)
    {
        try
        {
            _logger.LogInformation("Exporting report for user {UserId}, type: {ReportType}, format: {ExportFormat}", 
                userId, request.ReportType, request.ExportFormat);

            // TODO: Implementar exportación real (CSV, PDF, Excel)
            return OperationResult.Failure<byte[]>("Report export functionality not yet available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting report for user {UserId}", userId);
            return OperationResult.Failure<byte[]>("An error occurred while exporting the report");
        }
    }

    public async Task<OperationResult> SendMonthlyReportEmailAsync(Guid userId, int year, int month)
    {
        try
        {
            _logger.LogInformation("Sending monthly report email for user {UserId}, year {Year}, month {Month}", 
                userId, year, month);

            // TODO: Implementar envío de email con el reporte mensual
            return OperationResult.Failure("Email report functionality not yet available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending monthly report email for user {UserId}", userId);
            return OperationResult.Failure("An error occurred while sending the monthly report email");
        }
    }
}
