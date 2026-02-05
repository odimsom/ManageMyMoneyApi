using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Budgets;
using ManageMyMoney.Core.Domain.ValueObjects;

namespace ManageMyMoney.Core.Domain.Services;

public class BudgetCalculator
{
    public OperationResult<Money> CalculateDailyBudget(Budget budget)
    {
        var daysRemaining = budget.DateRange.TotalDays;
        if (daysRemaining <= 0)
            return OperationResult.Failure<Money>("Budget period has ended");

        var dailyAmount = budget.RemainingAmount / daysRemaining;
        return Money.Create(dailyAmount, budget.Limit.Currency);
    }

    public OperationResult<Money> CalculateWeeklyBudget(Budget budget)
    {
        var dailyResult = CalculateDailyBudget(budget);
        if (dailyResult.IsFailure)
            return dailyResult;

        var weeklyAmount = dailyResult.Value!.Amount * 7;
        return Money.Create(weeklyAmount, budget.Limit.Currency);
    }

    public decimal CalculateProjectedSpending(Budget budget)
    {
        var daysPassed = (DateTime.UtcNow - budget.DateRange.StartDate).TotalDays;
        if (daysPassed <= 0) return 0;

        var dailySpendingRate = budget.Spent.Amount / (decimal)daysPassed;
        var totalDays = budget.DateRange.TotalDays;

        return dailySpendingRate * totalDays;
    }

    public bool WillExceedBudget(Budget budget)
    {
        var projectedSpending = CalculateProjectedSpending(budget);
        return projectedSpending > budget.Limit.Amount;
    }

    public OperationResult<Percentage> CalculateSavingsRate(Money income, Money expenses)
    {
        if (income.Currency != expenses.Currency)
            return OperationResult.Failure<Percentage>("Currency mismatch");

        if (income.Amount <= 0)
            return OperationResult.Failure<Percentage>("Income must be greater than zero");

        var savings = income.Amount - expenses.Amount;
        var savingsRate = (savings / income.Amount) * 100;

        return Percentage.Create(Math.Max(0, savingsRate));
    }
}
