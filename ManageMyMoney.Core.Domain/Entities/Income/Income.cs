using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.ValueObjects;

namespace ManageMyMoney.Core.Domain.Entities.Income;

public class Income
{
    public Guid Id { get; private set; }
    public Money Amount { get; private set; } = Money.Zero();
    public string Description { get; private set; } = string.Empty;
    public DateTime Date { get; private set; }
    public Guid IncomeSourceId { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid UserId { get; private set; }
    public string? Notes { get; private set; }
    public bool IsRecurring { get; private set; }
    public Guid? RecurringIncomeId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Income() { }

    public static OperationResult<Income> Create(
        decimal amount,
        string currency,
        string description,
        DateTime date,
        Guid incomeSourceId,
        Guid accountId,
        Guid userId,
        string? notes = null)
    {
        var amountResult = Money.Create(amount, currency);
        if (amountResult.IsFailure)
            return OperationResult.Failure<Income>(amountResult.Error);

        if (amountResult.Value!.Amount <= 0)
            return OperationResult.Failure<Income>("Income amount must be greater than zero");

        if (string.IsNullOrWhiteSpace(description))
            return OperationResult.Failure<Income>("Description is required");

        if (description.Length > 500)
            return OperationResult.Failure<Income>("Description cannot exceed 500 characters");

        var income = new Income
        {
            Id = Guid.NewGuid(),
            Amount = amountResult.Value!,
            Description = description.Trim(),
            Date = date,
            IncomeSourceId = incomeSourceId,
            AccountId = accountId,
            UserId = userId,
            Notes = notes?.Trim(),
            IsRecurring = false,
            CreatedAt = DateTime.UtcNow
        };

        return OperationResult.Success(income);
    }

    public OperationResult UpdateAmount(decimal newAmount)
    {
        var amountResult = Money.Create(newAmount, Amount.Currency);
        if (amountResult.IsFailure)
            return OperationResult.Failure(amountResult.Error);

        if (amountResult.Value!.Amount <= 0)
            return OperationResult.Failure("Income amount must be greater than zero");

        Amount = amountResult.Value!;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public void MarkAsRecurring(Guid recurringIncomeId)
    {
        IsRecurring = true;
        RecurringIncomeId = recurringIncomeId;
    }
}
