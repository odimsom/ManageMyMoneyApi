using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Domain.ValueObjects;

public sealed class Percentage : IEquatable<Percentage>
{
    public decimal Value { get; }

    private Percentage(decimal value) => Value = value;

    public static OperationResult<Percentage> Create(decimal value)
    {
        if (value < 0)
            return OperationResult.Failure<Percentage>("Percentage cannot be negative");

        if (value > 100)
            return OperationResult.Failure<Percentage>("Percentage cannot exceed 100");

        return OperationResult.Success(new Percentage(Math.Round(value, 2)));
    }

    public static Percentage Zero => new(0);
    public static Percentage Full => new(100);

    public decimal AsDecimal => Value / 100m;

    public Money ApplyTo(Money amount)
    {
        var result = Money.Create(amount.Amount * AsDecimal, amount.Currency);
        return result.Value!;
    }

    public bool Equals(Percentage? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => Equals(obj as Percentage);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => $"{Value}%";
}
