using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Enums;
using ManageMyMoney.Core.Domain.ValueObjects;

namespace ManageMyMoney.Core.Domain.Entities.Accounts;

public class Account
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public AccountType Type { get; private set; }
    public Money Balance { get; private set; } = Money.Zero();
    public string Currency { get; private set; } = "USD";
    public string? Icon { get; private set; }
    public string? Color { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsActive { get; private set; }
    public bool IncludeInTotal { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Account() { }

    public static OperationResult<Account> Create(
        string name,
        AccountType type,
        Guid userId,
        decimal initialBalance = 0,
        string currency = "USD",
        string? description = null,
        string? icon = null,
        string? color = null,
        bool includeInTotal = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            return OperationResult.Failure<Account>("Account name is required");

        if (name.Length > 100)
            return OperationResult.Failure<Account>("Account name cannot exceed 100 characters");

        var balanceResult = Money.Create(initialBalance, currency);
        if (balanceResult.IsFailure)
            return OperationResult.Failure<Account>(balanceResult.Error);

        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Type = type,
            Balance = balanceResult.Value!,
            Currency = currency.ToUpperInvariant(),
            Icon = icon,
            Color = color,
            UserId = userId,
            IsActive = true,
            IncludeInTotal = includeInTotal,
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
            return result;

        Balance = result.Value!;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public OperationResult Debit(Money amount)
    {
        if (amount.Currency != Currency)
            return OperationResult.Failure("Currency mismatch");

        var result = Balance.Subtract(amount);
        if (result.IsFailure)
            return result;

        Balance = result.Value!;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public OperationResult UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return OperationResult.Failure("Account name is required");

        Name = newName.Trim();
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public OperationResult Deactivate()
    {
        if (!IsActive)
            return OperationResult.Failure("Account is already inactive");

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public void UpdateAppearance(string? icon, string? color)
    {
        Icon = icon;
        Color = color;
        UpdatedAt = DateTime.UtcNow;
    }
}
