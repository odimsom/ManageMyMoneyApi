namespace ManageMyMoney.Core.Application.DTOs.Notifications;

public record NotificationResponse
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
    public required string Type { get; init; }
    public string? ActionUrl { get; init; }
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ReadAt { get; init; }
}

public record CreateReminderRequest
{
    public required string Title { get; init; }
    public string? Description { get; init; }
    public DateTime DueDate { get; init; }
    public bool IsRecurring { get; init; }
    public string? Recurrence { get; init; }
    public string? RelatedEntityType { get; init; }
    public Guid? RelatedEntityId { get; init; }
}

public record ReminderResponse
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public DateTime DueDate { get; init; }
    public bool IsRecurring { get; init; }
    public string? Recurrence { get; init; }
    public bool IsCompleted { get; init; }
    public bool IsSent { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record AlertResponse
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
    public required string AlertType { get; init; }
    public string? RelatedEntityType { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public bool IsAcknowledged { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? AcknowledgedAt { get; init; }
}

public record UnreadCountResponse
{
    public int TotalUnread { get; init; }
    public int Notifications { get; init; }
    public int Alerts { get; init; }
    public int Reminders { get; init; }
}
