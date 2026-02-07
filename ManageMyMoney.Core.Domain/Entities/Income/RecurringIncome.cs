using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Enums;
using ManageMyMoney.Core.Domain.ValueObjects;

namespace ManageMyMoney.Core.Domain.Entities.Income;

public class RecurringIncome
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Money Amount { get; private set; } = Money.Zero();
    public string? Description { get; private set; }
    public RecurrenceType Recurrence { get; private set; }
    public int DayOfMonth { get; private set; }
    public Guid IncomeSourceId { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public DateTime? LastGeneratedDate { get; private set; }
    public DateTime? NextDueDate { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private RecurringIncome() { }

    public static OperationResult<RecurringIncome> Create(
        string name,
        decimal amount,
        string currency,
        RecurrenceType recurrence,
        Guid incomeSourceId,
        Guid accountId,
        Guid userId,
        DateTime startDate,
        int dayOfMonth = 1,
        DateTime? endDate = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return OperationResult.Failure<RecurringIncome>("Name is required");

        var amountResult = Money.Create(amount, currency);
        if (amountResult.IsFailure)
            return OperationResult.Failure<RecurringIncome>(amountResult.Error);

        if (amountResult.Value!.Amount <= 0)
            return OperationResult.Failure<RecurringIncome>("Amount must be greater than zero");

        var recurring = new RecurringIncome
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Amount = amountResult.Value!,
            Description = description?.Trim(),
            Recurrence = recurrence,
            DayOfMonth = dayOfMonth,
            IncomeSourceId = incomeSourceId,
            AccountId = accountId,
            UserId = userId,
            StartDate = startDate.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(startDate, DateTimeKind.Utc) : startDate.ToUniversalTime(),
            EndDate = endDate.HasValue ? (endDate.Value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc) : endDate.Value.ToUniversalTime()) : null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        recurring.CalculateNextDueDate();
        return OperationResult.Success(recurring);
    }

    public void CalculateNextDueDate()
    {
        var baseDate = LastGeneratedDate ?? StartDate;
        
        NextDueDate = Recurrence switch
        {
            RecurrenceType.Weekly => baseDate.AddDays(7),
            RecurrenceType.BiWeekly => baseDate.AddDays(14),
            RecurrenceType.Monthly => baseDate.AddMonths(1),
            RecurrenceType.Yearly => baseDate.AddYears(1),
            _ => baseDate.AddMonths(1)
        };

        if (EndDate.HasValue && NextDueDate > EndDate)
        {
            NextDueDate = null;
            IsActive = false;
        }
    }

    public void RecordGeneration(DateTime generatedDate)
    {
        LastGeneratedDate = generatedDate;
        CalculateNextDueDate();
    }
}
