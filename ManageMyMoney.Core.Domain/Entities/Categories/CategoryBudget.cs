using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Enums;
using ManageMyMoney.Core.Domain.ValueObjects;

namespace ManageMyMoney.Core.Domain.Entities.Categories;

public class CategoryBudget
{
    public Guid Id { get; private set; }
    public Guid CategoryId { get; private set; }
    public Guid UserId { get; private set; }
    public Money Limit { get; private set; } = Money.Zero();
    public Money Spent { get; private set; } = Money.Zero();
    public BudgetPeriod Period { get; private set; }
    public DateRange DateRange { get; private set; } = DateRange.CurrentMonth();
    public bool AlertEnabled { get; private set; }
    public Percentage? AlertThreshold { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private CategoryBudget() { }

    public static OperationResult<CategoryBudget> Create(
        Guid categoryId,
        Guid userId,
        decimal limitAmount,
        string currency,
        BudgetPeriod period,
        DateTime startDate,
        DateTime endDate,
        bool alertEnabled = true,
        decimal alertThresholdPercentage = 80)
    {
        var limitResult = Money.Create(limitAmount, currency);
        if (limitResult.IsFailure)
            return OperationResult.Failure<CategoryBudget>(limitResult.Error);

        if (limitResult.Value!.Amount <= 0)
            return OperationResult.Failure<CategoryBudget>("Budget limit must be greater than zero");

        var dateRangeResult = DateRange.Create(startDate, endDate);
        if (dateRangeResult.IsFailure)
            return OperationResult.Failure<CategoryBudget>(dateRangeResult.Error);

        var thresholdResult = Percentage.Create(alertThresholdPercentage);
        if (thresholdResult.IsFailure)
            return OperationResult.Failure<CategoryBudget>(thresholdResult.Error);

        var budget = new CategoryBudget
        {
            Id = Guid.NewGuid(),
            CategoryId = categoryId,
            UserId = userId,
            Limit = limitResult.Value!,
            Spent = Money.Zero(currency),
            Period = period,
            DateRange = dateRangeResult.Value!,
            AlertEnabled = alertEnabled,
            AlertThreshold = thresholdResult.Value!,
            CreatedAt = DateTime.UtcNow
        };

        return OperationResult.Success(budget);
    }

    public OperationResult AddSpending(Money amount)
    {
        if (amount.Currency != Limit.Currency)
            return OperationResult.Failure("Currency mismatch");

        var result = Spent.Add(amount);
        if (result.IsFailure)
            return result;

        Spent = result.Value!;
        return OperationResult.Success();
    }

    public decimal PercentageUsed => Limit.Amount > 0 
        ? Math.Round((Spent.Amount / Limit.Amount) * 100, 2) 
        : 0;

    public bool IsOverBudget => Spent.Amount > Limit.Amount;

    public bool ShouldAlert => AlertEnabled 
        && AlertThreshold is not null 
        && PercentageUsed >= AlertThreshold.Value 
        && !IsOverBudget;
}
