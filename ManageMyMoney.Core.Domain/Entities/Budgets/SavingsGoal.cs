using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Enums;
using ManageMyMoney.Core.Domain.ValueObjects;

namespace ManageMyMoney.Core.Domain.Entities.Budgets;

public class SavingsGoal
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Money TargetAmount { get; private set; } = Money.Zero();
    public Money CurrentAmount { get; private set; } = Money.Zero();
    public DateTime? TargetDate { get; private set; }
    public string? Icon { get; private set; }
    public string? Color { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? LinkedAccountId { get; private set; }
    public GoalStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private readonly List<GoalContribution> _contributions = [];
    public IReadOnlyCollection<GoalContribution> Contributions => _contributions.AsReadOnly();

    private SavingsGoal() { }

    public static OperationResult<SavingsGoal> Create(
        string name,
        decimal targetAmount,
        string currency,
        Guid userId,
        DateTime? targetDate = null,
        string? description = null,
        string? icon = null,
        string? color = null,
        Guid? linkedAccountId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return OperationResult.Failure<SavingsGoal>("Goal name is required");

        if (name.Length > 100)
            return OperationResult.Failure<SavingsGoal>("Goal name cannot exceed 100 characters");

        var targetResult = Money.Create(targetAmount, currency);
        if (targetResult.IsFailure)
            return OperationResult.Failure<SavingsGoal>(targetResult.Error);

        if (targetResult.Value!.Amount <= 0)
            return OperationResult.Failure<SavingsGoal>("Target amount must be greater than zero");

        if (targetDate.HasValue && targetDate.Value <= DateTime.UtcNow)
            return OperationResult.Failure<SavingsGoal>("Target date must be in the future");

        var goal = new SavingsGoal
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            TargetAmount = targetResult.Value!,
            CurrentAmount = Money.Zero(currency),
            TargetDate = targetDate,
            Icon = icon,
            Color = color,
            UserId = userId,
            LinkedAccountId = linkedAccountId,
            Status = GoalStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        return OperationResult.Success(goal);
    }

    public OperationResult AddContribution(GoalContribution contribution)
    {
        if (Status != GoalStatus.Active)
            return OperationResult.Failure("Cannot add contribution to inactive goal");

        if (contribution.Amount.Currency != TargetAmount.Currency)
            return OperationResult.Failure("Currency mismatch");

        var result = CurrentAmount.Add(contribution.Amount);
        if (result.IsFailure)
            return result;

        CurrentAmount = result.Value!;
        _contributions.Add(contribution);

        if (CurrentAmount.Amount >= TargetAmount.Amount)
        {
            Status = GoalStatus.Completed;
            CompletedAt = DateTime.UtcNow;
        }

        return OperationResult.Success();
    }

    public OperationResult Withdraw(Money amount)
    {
        if (amount.Currency != TargetAmount.Currency)
            return OperationResult.Failure("Currency mismatch");

        var result = CurrentAmount.Subtract(amount);
        if (result.IsFailure)
            return result;

        CurrentAmount = result.Value!;

        if (Status == GoalStatus.Completed && CurrentAmount.Amount < TargetAmount.Amount)
        {
            Status = GoalStatus.Active;
            CompletedAt = null;
        }

        return OperationResult.Success();
    }

    public decimal ProgressPercentage => TargetAmount.Amount > 0
        ? Math.Round((CurrentAmount.Amount / TargetAmount.Amount) * 100, 2)
        : 0;

    public Money RemainingAmount
    {
        get
        {
            var result = TargetAmount.Subtract(CurrentAmount);
            return result.IsSuccess ? result.Value! : Money.Zero(TargetAmount.Currency);
        }
    }

    public int? DaysRemaining => TargetDate.HasValue
        ? Math.Max(0, (int)(TargetDate.Value - DateTime.UtcNow).TotalDays)
        : null;

    public Money? RequiredMonthlyContribution
    {
        get
        {
            if (!DaysRemaining.HasValue || DaysRemaining.Value <= 0) return null;
            var months = Math.Ceiling(DaysRemaining.Value / 30.0);
            if (months <= 0) return null;
            var amount = RemainingAmount.Amount / (decimal)months;
            var result = Money.Create(amount, TargetAmount.Currency);
            return result.IsSuccess ? result.Value : null;
        }
    }

    public void Pause() => Status = GoalStatus.Paused;
    public void Resume() => Status = GoalStatus.Active;
    public void Cancel() => Status = GoalStatus.Cancelled;

    public OperationResult Update(
        string? name = null,
        decimal? targetAmount = null,
        DateTime? targetDate = null,
        string? description = null,
        string? icon = null,
        string? color = null)
    {
        if (name != null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return OperationResult.Failure("Goal name is required");
            if (name.Length > 100)
                return OperationResult.Failure("Goal name cannot exceed 100 characters");
            Name = name.Trim();
        }

        if (targetAmount.HasValue)
        {
            var targetResult = Money.Create(targetAmount.Value, TargetAmount.Currency);
            if (targetResult.IsFailure)
                return targetResult;
            if (targetResult.Value!.Amount <= 0)
                return OperationResult.Failure("Target amount must be greater than zero");
            TargetAmount = targetResult.Value!;
        }

        if (targetDate.HasValue)
        {
            if (targetDate.Value <= DateTime.UtcNow)
                return OperationResult.Failure("Target date must be in the future");
            TargetDate = targetDate;
        }

        if (description != null) Description = description.Trim();
        if (icon != null) Icon = icon;
        if (color != null) Color = color;

        return OperationResult.Success();
    }
}
