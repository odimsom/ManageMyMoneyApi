using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Auth;
using ManageMyMoney.Core.Domain.Entities.Expenses;
using ManageMyMoney.Core.Domain.Enums;
using ManageMyMoney.Core.Domain.ValueObjects;

namespace ManageMyMoney.Core.Domain.Entities.Accounts;

public class Account
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public AccountType Type { get; private set; }
    public Money Balance { get; private set; } = null!;
    public string Currency { get; private set; } = "USD";
    public string? Icon { get; private set; }
    public string? Color { get; private set; }
    public bool IncludeInTotal { get; private set; } = true;
    public Guid UserId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties
    public virtual User User { get; private set; } = null!;
    public virtual ICollection<Expense> Expenses { get; private set; } = new List<Expense>();
    public virtual CreditCard? CreditCard { get; private set; }

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
            Name = name,
            Description = description,
            Type = type,
            Balance = balanceResult.Value!,
            Currency = currency,
            Icon = icon,
            Color = color,
            IncludeInTotal = includeInTotal,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return OperationResult.Success(account);
    }

    public OperationResult Credit(Money amount)
    {
        if (amount.Currency != Balance.Currency)
            return OperationResult.Failure("Currency mismatch");

        var newBalanceResult = Money.Create(Balance.Amount + amount.Amount, Balance.Currency);
        if (newBalanceResult.IsFailure)
            return OperationResult.Failure(newBalanceResult.Error);

        Balance = newBalanceResult.Value!;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public OperationResult Debit(Money amount)
    {
        if (amount.Currency != Balance.Currency)
            return OperationResult.Failure("Currency mismatch");

        var newBalanceResult = Money.Create(Balance.Amount - amount.Amount, Balance.Currency);
        if (newBalanceResult.IsFailure)
            return OperationResult.Failure(newBalanceResult.Error);

        Balance = newBalanceResult.Value!;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public OperationResult UpdateDetails(string? name = null, string? description = null, string? icon = null, string? color = null, bool? includeInTotal = null)
    {
        if (name != null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return OperationResult.Failure("Account name cannot be empty");
            Name = name;
        }

        if (description != null) Description = description;
        if (icon != null) Icon = icon;
        if (color != null) Color = color;
        if (includeInTotal.HasValue) IncludeInTotal = includeInTotal.Value;

        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public OperationResult Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public OperationResult Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }
}
