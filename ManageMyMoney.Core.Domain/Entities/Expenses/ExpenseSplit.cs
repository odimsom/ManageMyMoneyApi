using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.ValueObjects;

namespace ManageMyMoney.Core.Domain.Entities.Expenses;

public class ExpenseSplit
{
    public Guid Id { get; private set; }
    public Guid ExpenseId { get; private set; }
    public Guid ParticipantUserId { get; private set; }
    public Money Amount { get; private set; } = Money.Zero();
    public Percentage? Percentage { get; private set; }
    public bool IsPaid { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ExpenseSplit() { }

    public static OperationResult<ExpenseSplit> Create(
        Guid expenseId,
        Guid participantUserId,
        decimal amount,
        string currency,
        Percentage? percentage = null)
    {
        if (expenseId == Guid.Empty)
            return OperationResult.Failure<ExpenseSplit>("Expense ID is required");

        if (participantUserId == Guid.Empty)
            return OperationResult.Failure<ExpenseSplit>("Participant user ID is required");

        var amountResult = Money.Create(amount, currency);
        if (amountResult.IsFailure)
            return OperationResult.Failure<ExpenseSplit>(amountResult.Error);

        var split = new ExpenseSplit
        {
            Id = Guid.NewGuid(),
            ExpenseId = expenseId,
            ParticipantUserId = participantUserId,
            Amount = amountResult.Value!,
            Percentage = percentage,
            IsPaid = false,
            CreatedAt = DateTime.UtcNow
        };

        return OperationResult.Success(split);
    }

    public OperationResult MarkAsPaid()
    {
        if (IsPaid)
            return OperationResult.Failure("Split is already marked as paid");

        IsPaid = true;
        PaidAt = DateTime.UtcNow;
        return OperationResult.Success();
    }
}
