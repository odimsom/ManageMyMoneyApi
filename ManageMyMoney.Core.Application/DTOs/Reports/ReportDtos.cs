namespace ManageMyMoney.Core.Application.DTOs.Reports;

public record FinancialSummaryResponse
{
    public decimal TotalIncome { get; init; }
    public decimal TotalExpenses { get; init; }
    public decimal NetBalance { get; init; }
    public decimal SavingsRate { get; init; }
    public required string Currency { get; init; }
    public DateTime FromDate { get; init; }
    public DateTime ToDate { get; init; }
}

public record MonthlyReportResponse
{
    public int Year { get; init; }
    public int Month { get; init; }
    public required string MonthName { get; init; }
    public decimal TotalIncome { get; init; }
    public decimal TotalExpenses { get; init; }
    public decimal NetBalance { get; init; }
    public decimal SavingsRate { get; init; }
    public required string Currency { get; init; }
    public List<CategoryBreakdownItem> ExpensesByCategory { get; init; } = new();
    public List<IncomeSourceBreakdownItem> IncomeBySource { get; init; } = new();
    public List<DailyBalanceItem> DailyBalance { get; init; } = new();
    public decimal ComparedToPreviousMonth { get; init; }
}

public record YearlyReportResponse
{
    public int Year { get; init; }
    public decimal TotalIncome { get; init; }
    public decimal TotalExpenses { get; init; }
    public decimal NetBalance { get; init; }
    public decimal AverageMonthlySavings { get; init; }
    public required string Currency { get; init; }
    public List<MonthlyTrendItem> MonthlyTrends { get; init; } = new();
    public List<CategoryBreakdownItem> ExpensesByCategory { get; init; } = new();
}

public record CategoryBreakdownItem
{
    public Guid CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public string? CategoryIcon { get; init; }
    public string? CategoryColor { get; init; }
    public decimal Amount { get; init; }
    public decimal Percentage { get; init; }
    public int TransactionCount { get; init; }
}

public record IncomeSourceBreakdownItem
{
    public Guid SourceId { get; init; }
    public required string SourceName { get; init; }
    public decimal Amount { get; init; }
    public decimal Percentage { get; init; }
}

public record DailyBalanceItem
{
    public DateTime Date { get; init; }
    public decimal Income { get; init; }
    public decimal Expenses { get; init; }
    public decimal Balance { get; init; }
}

public record MonthlyTrendItem
{
    public int Month { get; init; }
    public required string MonthName { get; init; }
    public decimal Income { get; init; }
    public decimal Expenses { get; init; }
    public decimal Balance { get; init; }
}

public record ComparisonReportRequest
{
    public DateTime Period1Start { get; init; }
    public DateTime Period1End { get; init; }
    public DateTime Period2Start { get; init; }
    public DateTime Period2End { get; init; }
}

public record ComparisonReportResponse
{
    public PeriodSummary Period1 { get; init; } = null!;
    public PeriodSummary Period2 { get; init; } = null!;
    public decimal IncomeChange { get; init; }
    public decimal IncomeChangePercentage { get; init; }
    public decimal ExpenseChange { get; init; }
    public decimal ExpenseChangePercentage { get; init; }
    public decimal BalanceChange { get; init; }
    public List<CategoryComparisonItem> CategoryComparison { get; init; } = new();
}

public record PeriodSummary
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public decimal TotalIncome { get; init; }
    public decimal TotalExpenses { get; init; }
    public decimal Balance { get; init; }
}

public record CategoryComparisonItem
{
    public Guid CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public decimal Period1Amount { get; init; }
    public decimal Period2Amount { get; init; }
    public decimal Change { get; init; }
    public decimal ChangePercentage { get; init; }
}

public record ExportReportRequest
{
    public required string ReportType { get; init; }
    public required string ExportFormat { get; init; }
    public DateTime FromDate { get; init; }
    public DateTime ToDate { get; init; }
    public List<Guid>? CategoryIds { get; init; }
    public List<Guid>? AccountIds { get; init; }
}
