using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.ValueObjects;

namespace ManageMyMoney.Core.Domain.Entities.Accounts;

public class CreditCard
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public Money CreditLimit { get; private set; } = Money.Zero();
    public Money CurrentBalance { get; private set; } = Money.Zero();
    public int StatementClosingDay { get; private set; }
    public int PaymentDueDay { get; private set; }
    public Percentage? InterestRate { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private CreditCard() { }

    public static OperationResult<CreditCard> Create(
        Guid accountId,
        decimal creditLimit,
        string currency,
        int statementClosingDay,
        int paymentDueDay,
        decimal? interestRatePercentage = null)
    {
        if (accountId == Guid.Empty)
            return OperationResult.Failure<CreditCard>("Account ID is required");

        var limitResult = Money.Create(creditLimit, currency);
        if (limitResult.IsFailure)
            return OperationResult.Failure<CreditCard>(limitResult.Error);

        if (limitResult.Value!.Amount <= 0)
            return OperationResult.Failure<CreditCard>("Credit limit must be greater than zero");

        if (statementClosingDay < 1 || statementClosingDay > 31)
            return OperationResult.Failure<CreditCard>("Statement closing day must be between 1 and 31");

        if (paymentDueDay < 1 || paymentDueDay > 31)
            return OperationResult.Failure<CreditCard>("Payment due day must be between 1 and 31");

        Percentage? interestRate = null;
        if (interestRatePercentage.HasValue)
        {
            var rateResult = Percentage.Create(interestRatePercentage.Value);
            if (rateResult.IsFailure)
                return OperationResult.Failure<CreditCard>(rateResult.Error);
            interestRate = rateResult.Value;
        }

        var card = new CreditCard
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            CreditLimit = limitResult.Value!,
            CurrentBalance = Money.Zero(currency),
            StatementClosingDay = statementClosingDay,
            PaymentDueDay = paymentDueDay,
            InterestRate = interestRate,
            CreatedAt = DateTime.UtcNow
        };

        return OperationResult.Success(card);
    }

    public Money AvailableCredit
    {
        get
        {
            var result = CreditLimit.Subtract(CurrentBalance);
            return result.IsSuccess ? result.Value! : Money.Zero(CreditLimit.Currency);
        }
    }

    public Percentage UtilizationRate
    {
        get
        {
            if (CreditLimit.Amount <= 0) return Percentage.Zero;
            var rate = (CurrentBalance.Amount / CreditLimit.Amount) * 100;
            var result = Percentage.Create(rate);
            return result.IsSuccess ? result.Value! : Percentage.Zero;
        }
    }

    public OperationResult AddCharge(Money amount)
    {
        if (amount.Currency != CreditLimit.Currency)
            return OperationResult.Failure("Currency mismatch");

        var newBalance = CurrentBalance.Add(amount);
        if (newBalance.IsFailure)
            return newBalance;

        CurrentBalance = newBalance.Value!;
        return OperationResult.Success();
    }

    public OperationResult MakePayment(Money amount)
    {
        if (amount.Currency != CreditLimit.Currency)
            return OperationResult.Failure("Currency mismatch");

        var newBalance = CurrentBalance.Subtract(amount);
        if (newBalance.IsFailure)
            return newBalance;

        CurrentBalance = newBalance.Value!;
        return OperationResult.Success();
    }
}
