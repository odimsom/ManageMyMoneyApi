using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.System;
using ManageMyMoney.Core.Domain.ValueObjects;

namespace ManageMyMoney.Core.Domain.Services;

public class CurrencyConverter
{
    public OperationResult<Money> Convert(Money amount, string targetCurrency, ExchangeRate rate)
    {
        if (amount.Currency == targetCurrency)
            return OperationResult.Success(amount);

        if (rate.FromCurrency != amount.Currency || rate.ToCurrency != targetCurrency)
            return OperationResult.Failure<Money>("Exchange rate does not match currencies");

        var convertedAmount = rate.Convert(amount.Amount);
        return Money.Create(convertedAmount, targetCurrency);
    }

    public OperationResult<Money> ConvertWithInverseRate(Money amount, string targetCurrency, ExchangeRate rate)
    {
        if (amount.Currency == targetCurrency)
            return OperationResult.Success(amount);

        if (rate.ToCurrency != amount.Currency || rate.FromCurrency != targetCurrency)
            return OperationResult.Failure<Money>("Exchange rate does not match currencies for inverse conversion");

        var convertedAmount = rate.ConvertReverse(amount.Amount);
        return Money.Create(convertedAmount, targetCurrency);
    }
}
