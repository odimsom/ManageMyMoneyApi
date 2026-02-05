using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Enums;
using ManageMyMoney.Core.Domain.ValueObjects;

namespace ManageMyMoney.Core.Domain.Entities;

public class Budget
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Money Limit { get; private set; } = Money.Zero();
    public Money Spent { get; private set; } = Money.Zero();
    public BudgetPeriod Period { get; private set; }
    public Guid? CategoryId { get; private set; }
    public Guid UserId { get; private set; }
    public DateRange DateRange { get; private set; } = DateRange.CurrentMonth();
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Budget() { }

    public static OperationResult<Budget> Create(
        string name,
        decimal limitAmount,
        string currency,
        BudgetPeriod period,
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        Guid? categoryId = null)
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
            Limit = limitResult.Value!,
            Spent = Money.Zero(currency),
            Period = period,
            CategoryId = categoryId,
            UserId = userId,
            DateRange = dateRangeResult.Value!,
            IsActive = true,
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
            return OperationResult.Failure(result.Error);

        Spent = result.Value!;
        return OperationResult.Success();
    }

    public OperationResult RemoveExpense(Money amount)
    {
        if (amount.Currency != Limit.Currency)
            return OperationResult.Failure("Currency mismatch");

        var result = Spent.Subtract(amount);
        if (result.IsFailure)
            return OperationResult.Failure(result.Error);

        Spent = result.Value!;
        return OperationResult.Success();
    }

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
            return OperationResult.Failure(limitResult.Error);

        if (limitResult.Value!.Amount <= 0)
            return OperationResult.Failure("Budget limit must be greater than zero");

        Limit = limitResult.Value!;
        return OperationResult.Success();
    }

    public void Deactivate() => IsActive = false;
}
