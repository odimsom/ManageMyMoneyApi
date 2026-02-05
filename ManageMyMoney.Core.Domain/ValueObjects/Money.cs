using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Domain.ValueObjects;

public sealed class Money : IEquatable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static OperationResult<Money> Create(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            return OperationResult.Failure<Money>("Amount cannot be negative");

        if (string.IsNullOrWhiteSpace(currency))
            return OperationResult.Failure<Money>("Currency is required");

        if (currency.Length != 3)
            return OperationResult.Failure<Money>("Currency must be a 3-letter ISO code");

        return OperationResult.Success(new Money(amount, currency.ToUpperInvariant()));
    }

    public static Money Zero(string currency = "USD") => new(0, currency);

    public OperationResult<Money> Add(Money other)
    {
        if (Currency != other.Currency)
            return OperationResult.Failure<Money>("Cannot add money with different currencies");

        return OperationResult.Success(new Money(Amount + other.Amount, Currency));
    }

    public OperationResult<Money> Subtract(Money other)
    {
        if (Currency != other.Currency)
            return OperationResult.Failure<Money>("Cannot subtract money with different currencies");

        return OperationResult.Success(new Money(Amount - other.Amount, Currency));
    }

    public bool Equals(Money? other)
    {
        if (other is null) return false;
        return Amount == other.Amount && Currency == other.Currency;
    }

    public override bool Equals(object? obj) => Equals(obj as Money);

    public override int GetHashCode() => HashCode.Combine(Amount, Currency);

    public static bool operator ==(Money? left, Money? right) => Equals(left, right);

    public static bool operator !=(Money? left, Money? right) => !Equals(left, right);

    public override string ToString() => $"{Amount:N2} {Currency}";
}
