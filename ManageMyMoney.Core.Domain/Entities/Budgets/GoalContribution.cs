using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.ValueObjects;

namespace ManageMyMoney.Core.Domain.Entities.Budgets;

public class GoalContribution
{
    public Guid Id { get; private set; }
    public Guid SavingsGoalId { get; private set; }
    public Money Amount { get; private set; } = Money.Zero();
    public string? Notes { get; private set; }
    public DateTime Date { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private GoalContribution() { }

    public static OperationResult<GoalContribution> Create(
        Guid savingsGoalId,
        decimal amount,
        string currency,
        DateTime date,
        string? notes = null)
    {
        if (savingsGoalId == Guid.Empty)
            return OperationResult.Failure<GoalContribution>("Savings goal ID is required");

        var amountResult = Money.Create(amount, currency);
        if (amountResult.IsFailure)
            return OperationResult.Failure<GoalContribution>(amountResult.Error);

        if (amountResult.Value!.Amount <= 0)
            return OperationResult.Failure<GoalContribution>("Contribution amount must be greater than zero");

        var contribution = new GoalContribution
        {
            Id = Guid.NewGuid(),
            SavingsGoalId = savingsGoalId,
            Amount = amountResult.Value!,
            Notes = notes?.Trim(),
            Date = date,
            CreatedAt = DateTime.UtcNow
        };

        return OperationResult.Success(contribution);
    }
}
