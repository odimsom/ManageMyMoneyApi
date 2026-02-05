using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Enums;

namespace ManageMyMoney.Core.Domain.Entities.Notifications;

public class Notification
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public NotificationType Type { get; private set; }
    public Guid UserId { get; private set; }
    public string? ActionUrl { get; private set; }
    public string? RelatedEntityType { get; private set; }
    public Guid? RelatedEntityId { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ReadAt { get; private set; }

    private Notification() { }

    public static OperationResult<Notification> Create(
        string title,
        string message,
        NotificationType type,
        Guid userId,
        string? actionUrl = null,
        string? relatedEntityType = null,
        Guid? relatedEntityId = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            return OperationResult.Failure<Notification>("Title is required");

        if (title.Length > 100)
            return OperationResult.Failure<Notification>("Title cannot exceed 100 characters");

        if (string.IsNullOrWhiteSpace(message))
            return OperationResult.Failure<Notification>("Message is required");

        if (message.Length > 500)
            return OperationResult.Failure<Notification>("Message cannot exceed 500 characters");

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Message = message.Trim(),
            Type = type,
            UserId = userId,
            ActionUrl = actionUrl,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        return OperationResult.Success(notification);
    }

    public void MarkAsRead()
    {
        if (!IsRead)
        {
            IsRead = true;
            ReadAt = DateTime.UtcNow;
        }
    }
}
