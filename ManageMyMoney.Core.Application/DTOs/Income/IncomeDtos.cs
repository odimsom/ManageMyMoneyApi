namespace ManageMyMoney.Core.Application.DTOs.Income;

public record CreateIncomeRequest
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public required string Description { get; init; }
    public DateTime Date { get; init; }
    public Guid IncomeSourceId { get; init; }
    public Guid AccountId { get; init; }
    public string? Notes { get; init; }
}

public record UpdateIncomeRequest
{
    public decimal? Amount { get; init; }
    public string? Description { get; init; }
    public DateTime? Date { get; init; }
    public Guid? IncomeSourceId { get; init; }
    public string? Notes { get; init; }
}

public record IncomeResponse
{
    public Guid Id { get; init; }
    public decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required string Description { get; init; }
    public DateTime Date { get; init; }
    public Guid IncomeSourceId { get; init; }
    public required string IncomeSourceName { get; init; }
    public Guid AccountId { get; init; }
    public required string AccountName { get; init; }
    public string? Notes { get; init; }
    public bool IsRecurring { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record IncomeSummaryResponse
{
    public decimal TotalAmount { get; init; }
    public required string Currency { get; init; }
    public int IncomeCount { get; init; }
    public decimal AverageIncome { get; init; }
    public required string TopSourceName { get; init; }
    public decimal TopSourceAmount { get; init; }
}

public record CreateIncomeSourceRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? Icon { get; init; }
    public string? Color { get; init; }
}

public record IncomeSourceResponse
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? Icon { get; init; }
    public string? Color { get; init; }
    public bool IsActive { get; init; }
}

public record CreateRecurringIncomeRequest
{
    public required string Name { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public string? Description { get; init; }
    public required string Recurrence { get; init; }
    public int DayOfMonth { get; init; } = 1;
    public Guid IncomeSourceId { get; init; }
    public Guid AccountId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
