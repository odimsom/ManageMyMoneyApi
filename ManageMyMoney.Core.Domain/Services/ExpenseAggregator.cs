using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Expenses;
using ManageMyMoney.Core.Domain.ValueObjects;

namespace ManageMyMoney.Core.Domain.Services;

public class ExpenseAggregator
{
    public OperationResult<Money> CalculateTotal(IEnumerable<Expense> expenses, string currency)
    {
        var expenseList = expenses.ToList();
        
        if (!expenseList.Any())
            return Money.Create(0, currency);

        if (expenseList.Any(e => e.Amount.Currency != currency))
            return OperationResult.Failure<Money>("All expenses must have the same currency");

        var total = expenseList.Sum(e => e.Amount.Amount);
        return Money.Create(total, currency);
    }

    public Dictionary<Guid, Money> GroupByCategory(IEnumerable<Expense> expenses, string currency)
    {
        return expenses
            .GroupBy(e => e.CategoryId)
            .ToDictionary(
                g => g.Key,
                g => Money.Create(g.Sum(e => e.Amount.Amount), currency).Value!
            );
    }

    public Dictionary<DateTime, Money> GroupByDay(IEnumerable<Expense> expenses, string currency)
    {
        return expenses
            .GroupBy(e => e.Date.Date)
            .OrderBy(g => g.Key)
            .ToDictionary(
                g => g.Key,
                g => Money.Create(g.Sum(e => e.Amount.Amount), currency).Value!
            );
    }

    public Dictionary<int, Money> GroupByMonth(IEnumerable<Expense> expenses, string currency)
    {
        return expenses
            .GroupBy(e => e.Date.Month)
            .OrderBy(g => g.Key)
            .ToDictionary(
                g => g.Key,
                g => Money.Create(g.Sum(e => e.Amount.Amount), currency).Value!
            );
    }

    public OperationResult<Money> CalculateAverage(IEnumerable<Expense> expenses, string currency)
    {
        var expenseList = expenses.ToList();
        
        if (!expenseList.Any())
            return Money.Create(0, currency);

        var average = expenseList.Average(e => e.Amount.Amount);
        return Money.Create(average, currency);
    }
}
