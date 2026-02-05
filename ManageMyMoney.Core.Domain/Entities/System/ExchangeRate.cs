using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Domain.Entities.System;

public class ExchangeRate
{
    public Guid Id { get; private set; }
    public string FromCurrency { get; private set; } = string.Empty;
    public string ToCurrency { get; private set; } = string.Empty;
    public decimal Rate { get; private set; }
    public DateTime EffectiveDate { get; private set; }
    public string? Source { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ExchangeRate() { }

    public static OperationResult<ExchangeRate> Create(
        string fromCurrency,
        string toCurrency,
        decimal rate,
        DateTime effectiveDate,
        string? source = null)
    {
        if (string.IsNullOrWhiteSpace(fromCurrency) || fromCurrency.Length != 3)
            return OperationResult.Failure<ExchangeRate>("Invalid source currency code");

        if (string.IsNullOrWhiteSpace(toCurrency) || toCurrency.Length != 3)
            return OperationResult.Failure<ExchangeRate>("Invalid target currency code");

        if (fromCurrency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
            return OperationResult.Failure<ExchangeRate>("Source and target currencies must be different");

        if (rate <= 0)
            return OperationResult.Failure<ExchangeRate>("Exchange rate must be greater than zero");

        var exchangeRate = new ExchangeRate
        {
            Id = Guid.NewGuid(),
            FromCurrency = fromCurrency.ToUpperInvariant(),
            ToCurrency = toCurrency.ToUpperInvariant(),
            Rate = rate,
            EffectiveDate = effectiveDate,
            Source = source,
            CreatedAt = DateTime.UtcNow
        };

        return OperationResult.Success(exchangeRate);
    }

    public decimal Convert(decimal amount) => amount * Rate;

    public decimal ConvertReverse(decimal amount) => Rate > 0 ? amount / Rate : 0;
}
