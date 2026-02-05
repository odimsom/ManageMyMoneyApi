using ManageMyMoney.Core.Application.DTOs.Notifications;
using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Application.Features.Notifications;

public interface INotificationService
{
    // Notifications
    Task<OperationResult<IEnumerable<NotificationResponse>>> GetNotificationsAsync(Guid userId, bool unreadOnly = false);
    Task<OperationResult<UnreadCountResponse>> GetUnreadCountAsync(Guid userId);
    Task<OperationResult> MarkAsReadAsync(Guid userId, Guid notificationId);
    Task<OperationResult> MarkAllAsReadAsync(Guid userId);
    Task<OperationResult> DeleteNotificationAsync(Guid userId, Guid notificationId);

    // Reminders
    Task<OperationResult<ReminderResponse>> CreateReminderAsync(Guid userId, CreateReminderRequest request);
    Task<OperationResult<IEnumerable<ReminderResponse>>> GetRemindersAsync(Guid userId, bool pendingOnly = true);
    Task<OperationResult> CompleteReminderAsync(Guid userId, Guid reminderId);
    Task<OperationResult> DeleteReminderAsync(Guid userId, Guid reminderId);

    // Alerts
    Task<OperationResult<IEnumerable<AlertResponse>>> GetAlertsAsync(Guid userId, bool unacknowledgedOnly = true);
    Task<OperationResult> AcknowledgeAlertAsync(Guid userId, Guid alertId);

    // System Notifications (internal use)
    Task<OperationResult> SendBudgetAlertAsync(Guid userId, Guid budgetId, string budgetName, decimal percentageUsed);
    Task<OperationResult> SendGoalCompletedNotificationAsync(Guid userId, Guid goalId, string goalName);
    Task<OperationResult> SendPaymentReminderAsync(Guid userId, string description, DateTime dueDate);
}
