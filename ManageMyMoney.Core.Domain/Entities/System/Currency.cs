using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Domain.Entities.System;

public class Currency
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Symbol { get; private set; } = string.Empty;
    public int DecimalPlaces { get; private set; }
    public bool IsActive { get; private set; }

    private Currency() { }

    public static OperationResult<Currency> Create(
        string code,
        string name,
        string symbol,
        int decimalPlaces = 2)
    {
        if (string.IsNullOrWhiteSpace(code))
            return OperationResult.Failure<Currency>("Currency code is required");

        if (code.Length != 3)
            return OperationResult.Failure<Currency>("Currency code must be 3 characters");

        if (string.IsNullOrWhiteSpace(name))
            return OperationResult.Failure<Currency>("Currency name is required");

        if (string.IsNullOrWhiteSpace(symbol))
            return OperationResult.Failure<Currency>("Currency symbol is required");

        if (decimalPlaces < 0 || decimalPlaces > 4)
            return OperationResult.Failure<Currency>("Decimal places must be between 0 and 4");

        var currency = new Currency
        {
            Code = code.ToUpperInvariant(),
            Name = name.Trim(),
            Symbol = symbol,
            DecimalPlaces = decimalPlaces,
            IsActive = true
        };

        return OperationResult.Success(currency);
    }

    public void Deactivate() => IsActive = false;
}
