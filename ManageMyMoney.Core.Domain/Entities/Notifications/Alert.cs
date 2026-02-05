using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Domain.Entities.Notifications;

public class Alert
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string AlertType { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public string? RelatedEntityType { get; private set; }
    public Guid? RelatedEntityId { get; private set; }
    public bool IsAcknowledged { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? AcknowledgedAt { get; private set; }

    private Alert() { }

    public static OperationResult<Alert> CreateBudgetExceededAlert(
        Guid userId,
        Guid budgetId,
        string budgetName,
        decimal percentageUsed)
    {
        return Create(
            "Budget Exceeded",
            $"Your budget '{budgetName}' has exceeded its limit ({percentageUsed:F1}% used)",
            "BudgetExceeded",
            userId,
            "Budget",
            budgetId);
    }

    public static OperationResult<Alert> CreateBudgetWarningAlert(
        Guid userId,
        Guid budgetId,
        string budgetName,
        decimal percentageUsed)
    {
        return Create(
            "Budget Warning",
            $"Your budget '{budgetName}' is at {percentageUsed:F1}% of its limit",
            "BudgetWarning",
            userId,
            "Budget",
            budgetId);
    }

    public static OperationResult<Alert> Create(
        string title,
        string message,
        string alertType,
        Guid userId,
        string? relatedEntityType = null,
        Guid? relatedEntityId = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            return OperationResult.Failure<Alert>("Title is required");

        if (string.IsNullOrWhiteSpace(message))
            return OperationResult.Failure<Alert>("Message is required");

        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Message = message.Trim(),
            AlertType = alertType,
            UserId = userId,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            IsAcknowledged = false,
            CreatedAt = DateTime.UtcNow
        };

        return OperationResult.Success(alert);
    }

    public void Acknowledge()
    {
        if (!IsAcknowledged)
        {
            IsAcknowledged = true;
            AcknowledgedAt = DateTime.UtcNow;
        }
    }
}
