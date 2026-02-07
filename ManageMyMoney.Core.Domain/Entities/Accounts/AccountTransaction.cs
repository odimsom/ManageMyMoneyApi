using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.ValueObjects;

namespace ManageMyMoney.Core.Domain.Entities.Accounts;

public class AccountTransaction
{
    public Guid Id { get; private set; }
    public Guid FromAccountId { get; private set; }
    public Guid ToAccountId { get; private set; }
    public Money Amount { get; private set; } = Money.Zero();
    public string? Description { get; private set; }
    public DateTime Date { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private AccountTransaction() { }

    public static OperationResult<AccountTransaction> Create(
        Guid fromAccountId,
        Guid toAccountId,
        decimal amount,
        string currency,
        Guid userId,
        DateTime date,
        string? description = null)
    {
        if (fromAccountId == Guid.Empty)
            return OperationResult.Failure<AccountTransaction>("Source account is required");

        if (toAccountId == Guid.Empty)
            return OperationResult.Failure<AccountTransaction>("Destination account is required");

        if (fromAccountId == toAccountId)
            return OperationResult.Failure<AccountTransaction>("Cannot transfer to the same account");

        var amountResult = Money.Create(amount, currency);
        if (amountResult.IsFailure)
            return OperationResult.Failure<AccountTransaction>(amountResult.Error);

        if (amountResult.Value!.Amount <= 0)
            return OperationResult.Failure<AccountTransaction>("Transfer amount must be greater than zero");

        var transaction = new AccountTransaction
        {
            Id = Guid.NewGuid(),
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            Amount = amountResult.Value!,
            Description = description?.Trim(),
            Date = date.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(date, DateTimeKind.Utc) : date.ToUniversalTime(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        return OperationResult.Success(transaction);
    }
}
