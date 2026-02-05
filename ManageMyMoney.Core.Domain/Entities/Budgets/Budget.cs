using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Enums;
using ManageMyMoney.Core.Domain.ValueObjects;

namespace ManageMyMoney.Core.Domain.Entities.Budgets;

public class Budget
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Money Limit { get; private set; } = Money.Zero();
    public Money Spent { get; private set; } = Money.Zero();
    public BudgetPeriod Period { get; private set; }
    public DateRange DateRange { get; private set; } = DateRange.CurrentMonth();
    public Guid UserId { get; private set; }
    public bool IsActive { get; private set; }
    public bool AlertsEnabled { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<Guid> _categoryIds = [];
    public IReadOnlyCollection<Guid> CategoryIds => _categoryIds.AsReadOnly();

    private Budget() { }

    public static OperationResult<Budget> Create(
        string name,
        decimal limitAmount,
        string currency,
        BudgetPeriod period,
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        string? description = null,
        bool alertsEnabled = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            return OperationResult.Failure<Budget>("Budget name is required");

        if (name.Length > 100)
            return OperationResult.Failure<Budget>("Budget name cannot exceed 100 characters");

        var limitResult = Money.Create(limitAmount, currency);
        if (limitResult.IsFailure)
            return OperationResult.Failure<Budget>(limitResult.Error);

        if (limitResult.Value!.Amount <= 0)
            return OperationResult.Failure<Budget>("Budget limit must be greater than zero");

        var dateRangeResult = DateRange.Create(startDate, endDate);
        if (dateRangeResult.IsFailure)
            return OperationResult.Failure<Budget>(dateRangeResult.Error);

        var budget = new Budget
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Limit = limitResult.Value!,
            Spent = Money.Zero(currency),
            Period = period,
            DateRange = dateRangeResult.Value!,
            UserId = userId,
            IsActive = true,
            AlertsEnabled = alertsEnabled,
            CreatedAt = DateTime.UtcNow
        };

        return OperationResult.Success(budget);
    }

    public OperationResult AddExpense(Money amount)
    {
        if (amount.Currency != Limit.Currency)
            return OperationResult.Failure("Currency mismatch");

        var result = Spent.Add(amount);
        if (result.IsFailure)
            return result;

        Spent = result.Value!;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public OperationResult RemoveExpense(Money amount)
    {
        if (amount.Currency != Limit.Currency)
            return OperationResult.Failure("Currency mismatch");

        var result = Spent.Subtract(amount);
        if (result.IsFailure)
            return result;

        Spent = result.Value!;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public void AddCategory(Guid categoryId)
    {
        if (!_categoryIds.Contains(categoryId))
            _categoryIds.Add(categoryId);
    }

    public void RemoveCategory(Guid categoryId) => _categoryIds.Remove(categoryId);

    public decimal RemainingAmount => Limit.Amount - Spent.Amount;

    public decimal PercentageUsed => Limit.Amount > 0
        ? Math.Round((Spent.Amount / Limit.Amount) * 100, 2)
        : 0;

    public bool IsOverBudget => Spent.Amount > Limit.Amount;

    public bool IsNearLimit(decimal thresholdPercentage = 80) =>
        PercentageUsed >= thresholdPercentage && !IsOverBudget;

    public OperationResult UpdateLimit(decimal newLimit)
    {
        var limitResult = Money.Create(newLimit, Limit.Currency);
        if (limitResult.IsFailure)
            return limitResult;

        if (limitResult.Value!.Amount <= 0)
            return OperationResult.Failure("Budget limit must be greater than zero");

        Limit = limitResult.Value!;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
