namespace ManageMyMoney.Core.Application.DTOs.Budgets;

public record CreateBudgetRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public decimal LimitAmount { get; init; }
    public string Currency { get; init; } = "USD";
    public required string Period { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public List<Guid>? CategoryIds { get; init; }
    public bool AlertsEnabled { get; init; } = true;
}

public record UpdateBudgetRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public decimal? LimitAmount { get; init; }
    public List<Guid>? CategoryIds { get; init; }
    public bool? AlertsEnabled { get; init; }
}

public record BudgetResponse
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public decimal LimitAmount { get; init; }
    public decimal SpentAmount { get; init; }
    public decimal RemainingAmount { get; init; }
    public required string Currency { get; init; }
    public required string Period { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public decimal PercentageUsed { get; init; }
    public bool IsOverBudget { get; init; }
    public bool IsNearLimit { get; init; }
    public bool IsActive { get; init; }
    public bool AlertsEnabled { get; init; }
    public List<Guid> CategoryIds { get; init; } = new();
    public DateTime CreatedAt { get; init; }
}

public record BudgetProgressResponse
{
    public Guid BudgetId { get; init; }
    public required string BudgetName { get; init; }
    public decimal LimitAmount { get; init; }
    public decimal SpentAmount { get; init; }
    public decimal RemainingAmount { get; init; }
    public required string Currency { get; init; }
    public decimal PercentageUsed { get; init; }
    public decimal DailyBudget { get; init; }
    public decimal ProjectedSpending { get; init; }
    public bool WillExceedBudget { get; init; }
    public int DaysRemaining { get; init; }
    public List<DailySpendingItem> DailySpending { get; init; } = new();
}

public record DailySpendingItem
{
    public DateTime Date { get; init; }
    public decimal Amount { get; init; }
}

public record CreateSavingsGoalRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public decimal TargetAmount { get; init; }
    public string Currency { get; init; } = "USD";
    public DateTime? TargetDate { get; init; }
    public string? Icon { get; init; }
    public string? Color { get; init; }
    public Guid? LinkedAccountId { get; init; }
}

public record UpdateSavingsGoalRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public decimal? TargetAmount { get; init; }
    public DateTime? TargetDate { get; init; }
    public string? Icon { get; init; }
    public string? Color { get; init; }
}

public record SavingsGoalResponse
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public decimal TargetAmount { get; init; }
    public decimal CurrentAmount { get; init; }
    public decimal RemainingAmount { get; init; }
    public required string Currency { get; init; }
    public DateTime? TargetDate { get; init; }
    public int? DaysRemaining { get; init; }
    public decimal ProgressPercentage { get; init; }
    public decimal? RequiredMonthlyContribution { get; init; }
    public string? Icon { get; init; }
    public string? Color { get; init; }
    public required string Status { get; init; }
    public Guid? LinkedAccountId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}

public record AddContributionRequest
{
    public decimal Amount { get; init; }
    public DateTime Date { get; init; }
    public string? Notes { get; init; }
}

public record ContributionResponse
{
    public Guid Id { get; init; }
    public Guid SavingsGoalId { get; init; }
    public decimal Amount { get; init; }
    public required string Currency { get; init; }
    public DateTime Date { get; init; }
    public string? Notes { get; init; }
}
