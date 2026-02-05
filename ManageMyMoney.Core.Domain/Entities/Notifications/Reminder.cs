using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Enums;

namespace ManageMyMoney.Core.Domain.Entities.Notifications;

public class Reminder
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime DueDate { get; private set; }
    public Guid UserId { get; private set; }
    public string? RelatedEntityType { get; private set; }
    public Guid? RelatedEntityId { get; private set; }
    public bool IsRecurring { get; private set; }
    public RecurrenceType? Recurrence { get; private set; }
    public bool IsCompleted { get; private set; }
    public bool IsSent { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Reminder() { }

    public static OperationResult<Reminder> Create(
        string title,
        DateTime dueDate,
        Guid userId,
        string? description = null,
        string? relatedEntityType = null,
        Guid? relatedEntityId = null,
        bool isRecurring = false,
        RecurrenceType? recurrence = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            return OperationResult.Failure<Reminder>("Title is required");

        if (dueDate <= DateTime.UtcNow)
            return OperationResult.Failure<Reminder>("Due date must be in the future");

        if (isRecurring && !recurrence.HasValue)
            return OperationResult.Failure<Reminder>("Recurrence type is required for recurring reminders");

        var reminder = new Reminder
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Description = description?.Trim(),
            DueDate = dueDate,
            UserId = userId,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            IsRecurring = isRecurring,
            Recurrence = recurrence,
            IsCompleted = false,
            IsSent = false,
            CreatedAt = DateTime.UtcNow
        };

        return OperationResult.Success(reminder);
    }

    public OperationResult MarkAsSent()
    {
        if (IsSent)
            return OperationResult.Failure("Reminder has already been sent");

        IsSent = true;
        return OperationResult.Success();
    }

    public OperationResult Complete()
    {
        if (IsCompleted)
            return OperationResult.Failure("Reminder is already completed");

        IsCompleted = true;
        return OperationResult.Success();
    }

    public bool IsDue => !IsCompleted && !IsSent && DueDate <= DateTime.UtcNow;
}
