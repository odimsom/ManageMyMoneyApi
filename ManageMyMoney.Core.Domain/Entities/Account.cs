using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Enums;
using ManageMyMoney.Core.Domain.ValueObjects;

namespace ManageMyMoney.Core.Domain.Entities;

public class Account
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public AccountType Type { get; private set; }
    public Money Balance { get; private set; } = Money.Zero();
    public string Currency { get; private set; } = "USD";
    public Guid UserId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Account() { }

    public static OperationResult<Account> Create(
        string name,
        AccountType type,
        Guid userId,
        decimal initialBalance = 0,
        string currency = "USD")
    {
        var validationResult = ValidateName(name);
        if (validationResult.IsFailure)
            return OperationResult.Failure<Account>(validationResult.Error);

        var balanceResult = Money.Create(initialBalance, currency);
        if (balanceResult.IsFailure)
            return OperationResult.Failure<Account>(balanceResult.Error);

        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Type = type,
            UserId = userId,
            Balance = balanceResult.Value!,
            Currency = currency.ToUpperInvariant(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return OperationResult.Success(account);
    }

    public OperationResult Credit(Money amount)
    {
        if (amount.Currency != Currency)
            return OperationResult.Failure("Currency mismatch");

        var result = Balance.Add(amount);
        if (result.IsFailure)
            return OperationResult.Failure(result.Error);

        Balance = result.Value!;
        return OperationResult.Success();
    }

    public OperationResult Debit(Money amount)
    {
        if (amount.Currency != Currency)
            return OperationResult.Failure("Currency mismatch");

        var result = Balance.Subtract(amount);
        if (result.IsFailure)
            return OperationResult.Failure(result.Error);

        Balance = result.Value!;
        return OperationResult.Success();
    }

    public OperationResult Deactivate()
    {
        if (!IsActive)
            return OperationResult.Failure("Account is already inactive");

        IsActive = false;
        return OperationResult.Success();
    }

    public OperationResult UpdateName(string newName)
    {
        var validationResult = ValidateName(newName);
        if (validationResult.IsFailure)
            return validationResult;

        Name = newName.Trim();
        return OperationResult.Success();
    }

    private static OperationResult ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return OperationResult.Failure("Account name is required");

        if (name.Length > 100)
            return OperationResult.Failure("Account name cannot exceed 100 characters");

        return OperationResult.Success();
    }
}
