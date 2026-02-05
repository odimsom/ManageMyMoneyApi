using ManageMyMoney.Core.Domain.Entities.System;
using ManageMyMoney.Infrastructure.Persistence.Context;

namespace ManageMyMoney.Infrastructure.Persistence.Seeds;

public static class CurrencySeed
{
    public static async Task SeedAsync(ManageMyMoneyContext context)
    {
        if (context.Currencies.Any())
            return;

        var currencies = new List<(string Code, string Name, string Symbol, int Decimals)>
        {
            ("USD", "US Dollar", "$", 2),
            ("EUR", "Euro", "€", 2),
            ("GBP", "British Pound", "£", 2),
            ("JPY", "Japanese Yen", "¥", 0),
            ("CAD", "Canadian Dollar", "CA$", 2),
            ("AUD", "Australian Dollar", "A$", 2),
            ("CHF", "Swiss Franc", "CHF", 2),
            ("CNY", "Chinese Yuan", "¥", 2),
            ("MXN", "Mexican Peso", "$", 2),
            ("BRL", "Brazilian Real", "R$", 2),
            ("ARS", "Argentine Peso", "$", 2),
            ("COP", "Colombian Peso", "$", 2),
            ("CLP", "Chilean Peso", "$", 0),
            ("PEN", "Peruvian Sol", "S/", 2),
            ("DOP", "Dominican Peso", "RD$", 2)
        };

        foreach (var (code, name, symbol, decimals) in currencies)
        {
            var result = Currency.Create(code, name, symbol, decimals);
            if (result.IsSuccess)
                context.Currencies.Add(result.Value!);
        }

        await context.SaveChangesAsync();
    }
}
