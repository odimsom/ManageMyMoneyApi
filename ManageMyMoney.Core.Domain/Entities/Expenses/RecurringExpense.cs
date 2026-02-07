using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Enums;
using ManageMyMoney.Core.Domain.ValueObjects;

namespace ManageMyMoney.Core.Domain.Entities.Expenses;

public class RecurringExpense
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Money Amount { get; private set; } = Money.Zero();
    public string? Description { get; private set; }
    public RecurrenceType Recurrence { get; private set; }
    public int DayOfMonth { get; private set; }
    public DayOfWeek? DayOfWeek { get; private set; }
    public Guid CategoryId { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public DateTime? LastGeneratedDate { get; private set; }
    public DateTime? NextDueDate { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private RecurringExpense() { }

    public static OperationResult<RecurringExpense> Create(
        string name,
        decimal amount,
        string currency,
        RecurrenceType recurrence,
        Guid categoryId,
        Guid accountId,
        Guid userId,
        DateTime startDate,
        int dayOfMonth = 1,
        DayOfWeek? dayOfWeek = null,
        DateTime? endDate = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return OperationResult.Failure<RecurringExpense>("Name is required");

        if (name.Length > 100)
            return OperationResult.Failure<RecurringExpense>("Name cannot exceed 100 characters");

        var amountResult = Money.Create(amount, currency);
        if (amountResult.IsFailure)
            return OperationResult.Failure<RecurringExpense>(amountResult.Error);

        if (amountResult.Value!.Amount <= 0)
            return OperationResult.Failure<RecurringExpense>("Amount must be greater than zero");

        if (dayOfMonth < 1 || dayOfMonth > 31)
            return OperationResult.Failure<RecurringExpense>("Day of month must be between 1 and 31");

        if (endDate.HasValue && endDate < startDate)
            return OperationResult.Failure<RecurringExpense>("End date cannot be before start date");

        var recurring = new RecurringExpense
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Amount = amountResult.Value!,
            Description = description?.Trim(),
            Recurrence = recurrence,
            DayOfMonth = dayOfMonth,
            DayOfWeek = dayOfWeek,
            CategoryId = categoryId,
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
            RecurrenceType.Daily => baseDate.AddDays(1),
            RecurrenceType.Weekly => baseDate.AddDays(7),
            RecurrenceType.BiWeekly => baseDate.AddDays(14),
            RecurrenceType.Monthly => baseDate.AddMonths(1),
            RecurrenceType.Quarterly => baseDate.AddMonths(3),
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

    public OperationResult Pause()
    {
        if (!IsActive)
            return OperationResult.Failure("Recurring expense is already paused");

        IsActive = false;
        return OperationResult.Success();
    }

    public OperationResult Resume()
    {
        if (IsActive)
            return OperationResult.Failure("Recurring expense is already active");

        IsActive = true;
        CalculateNextDueDate();
        return OperationResult.Success();
    }

    public bool IsDueToday => NextDueDate?.Date == DateTime.UtcNow.Date;
}
