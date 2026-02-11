using ManageMyMoney.Core.Application.DTOs.Notifications;
using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Notifications;
using ManageMyMoney.Core.Domain.Enums;
using ManageMyMoney.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ManageMyMoney.Core.Application.Features.Notifications;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IReminderRepository _reminderRepository;
    private readonly IAlertRepository _alertRepository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        IReminderRepository reminderRepository,
        IAlertRepository alertRepository,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _reminderRepository = reminderRepository;
        _alertRepository = alertRepository;
        _logger = logger;
    }

    public async Task<OperationResult<IEnumerable<NotificationResponse>>> GetNotificationsAsync(Guid userId, bool unreadOnly = false)
    {
        var result = unreadOnly 
            ? await _notificationRepository.GetUnreadByUserAsync(userId)
            : await _notificationRepository.GetAllByUserAsync(userId);
        
        if (result.IsFailure)
            return OperationResult.Failure<IEnumerable<NotificationResponse>>(result.Error);

        return OperationResult.Success(result.Value!.Select(MapToResponse));
    }

    public async Task<OperationResult<UnreadCountResponse>> GetUnreadCountAsync(Guid userId)
    {
        var result = await _notificationRepository.GetUnreadCountByUserAsync(userId);
        if (result.IsFailure)
            return OperationResult.Failure<UnreadCountResponse>(result.Error);

        return OperationResult.Success(new UnreadCountResponse
        {
            TotalUnread = result.Value,
            Notifications = result.Value, // Simplified
            Alerts = 0,
            Reminders = 0
        });
    }

    public async Task<OperationResult> MarkAsReadAsync(Guid userId, Guid notificationId)
    {
        var result = await _notificationRepository.GetByIdAsync(notificationId);
        if (result.IsFailure)
            return OperationResult.Failure(result.Error);

        if (result.Value!.UserId != userId)
            return OperationResult.Failure("Notification not found");

        result.Value.MarkAsRead();
        return await _notificationRepository.UpdateAsync(result.Value);
    }

    public async Task<OperationResult> MarkAllAsReadAsync(Guid userId)
    {
        return await _notificationRepository.MarkAllAsReadAsync(userId);
    }

    public async Task<OperationResult> DeleteNotificationAsync(Guid userId, Guid notificationId)
    {
        var result = await _notificationRepository.GetByIdAsync(notificationId);
        if (result.IsFailure)
            return OperationResult.Failure(result.Error);

        if (result.Value!.UserId != userId)
            return OperationResult.Failure("Notification not found");

        return await _notificationRepository.DeleteAsync(notificationId);
    }

    public async Task<OperationResult<ReminderResponse>> CreateReminderAsync(Guid userId, CreateReminderRequest request)
    {
        var reminderResult = Reminder.Create(
            request.Title,
            request.DueDate,
            userId,
            request.Description);

        if (reminderResult.IsFailure)
            return OperationResult.Failure<ReminderResponse>(reminderResult.Error);

        var saveResult = await _reminderRepository.AddAsync(reminderResult.Value!);
        if (saveResult.IsFailure)
            return OperationResult.Failure<ReminderResponse>(saveResult.Error);

        return OperationResult.Success(MapToReminderResponse(reminderResult.Value!));
    }

    public async Task<OperationResult<IEnumerable<ReminderResponse>>> GetRemindersAsync(Guid userId, bool pendingOnly = true)
    {
        var result = await _reminderRepository.GetByUserAsync(userId, pendingOnly);
        if (result.IsFailure)
            return OperationResult.Failure<IEnumerable<ReminderResponse>>(result.Error);

        return OperationResult.Success(result.Value!.Select(MapToReminderResponse));
    }

    public async Task<OperationResult> CompleteReminderAsync(Guid userId, Guid reminderId)
    {
        var result = await _reminderRepository.GetByIdAsync(reminderId);
        if (result.IsFailure)
            return OperationResult.Failure(result.Error);

        if (result.Value!.UserId != userId)
            return OperationResult.Failure("Reminder not found");

        var completeResult = result.Value.Complete();
        if (completeResult.IsFailure)
            return completeResult;

        return await _reminderRepository.UpdateAsync(result.Value);
    }

    public async Task<OperationResult> DeleteReminderAsync(Guid userId, Guid reminderId)
    {
        var result = await _reminderRepository.GetByIdAsync(reminderId);
        if (result.IsFailure)
            return OperationResult.Failure(result.Error);

        if (result.Value!.UserId != userId)
            return OperationResult.Failure("Reminder not found");

        return await _reminderRepository.DeleteAsync(reminderId);
    }

    public async Task<OperationResult<IEnumerable<AlertResponse>>> GetAlertsAsync(Guid userId, bool unacknowledgedOnly = true)
    {
        var result = await _alertRepository.GetByUserAsync(userId, unacknowledgedOnly);
        if (result.IsFailure)
            return OperationResult.Failure<IEnumerable<AlertResponse>>(result.Error);

        return OperationResult.Success(result.Value!.Select(MapToAlertResponse));
    }

    public async Task<OperationResult> AcknowledgeAlertAsync(Guid userId, Guid alertId)
    {
        var result = await _alertRepository.GetByIdAsync(alertId);
        if (result.IsFailure)
            return OperationResult.Failure(result.Error);

        if (result.Value!.UserId != userId)
            return OperationResult.Failure("Alert not found");

        result.Value.Acknowledge();
        return await _alertRepository.UpdateAsync(result.Value);
    }

    public async Task<OperationResult> SendBudgetAlertAsync(Guid userId, Guid budgetId, string budgetName, decimal percentageUsed)
    {
        var notification = Notification.Create(
            "Budget Alert",
            $"Your budget '{budgetName}' has reached {percentageUsed:P0} of its limit.",
            NotificationType.Warning,
            userId,
            relatedEntityType: "Budget",
            relatedEntityId: budgetId);

        if (notification.IsFailure)
            return OperationResult.Failure(notification.Error);

        return await _notificationRepository.AddAsync(notification.Value!);
    }

    public async Task<OperationResult> SendGoalCompletedNotificationAsync(Guid userId, Guid goalId, string goalName)
    {
        var notification = Notification.Create(
            "Goal Completed!",
            $"Congratulations! You've reached your savings goal '{goalName}'.",
            NotificationType.Success,
            userId,
            relatedEntityType: "SavingsGoal",
            relatedEntityId: goalId);

        if (notification.IsFailure)
            return OperationResult.Failure(notification.Error);

        return await _notificationRepository.AddAsync(notification.Value!);
    }

    public async Task<OperationResult> SendPaymentReminderAsync(Guid userId, string description, DateTime dueDate)
    {
        var notification = Notification.Create(
            "Payment Reminder",
            $"Reminder: Your payment for '{description}' is due on {dueDate:d}.",
            NotificationType.Reminder,
            userId);

        if (notification.IsFailure)
            return OperationResult.Failure(notification.Error);

        return await _notificationRepository.AddAsync(notification.Value!);
    }

    private static NotificationResponse MapToResponse(Notification notification)
    {
        return new NotificationResponse
        {
            Id = notification.Id,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type.ToString(),
            ActionUrl = notification.ActionUrl,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            ReadAt = notification.ReadAt
        };
    }

    private static ReminderResponse MapToReminderResponse(Reminder reminder)
    {
        return new ReminderResponse
        {
            Id = reminder.Id,
            Title = reminder.Title,
            Description = reminder.Description,
            DueDate = reminder.DueDate,
            IsCompleted = reminder.IsCompleted,
            IsSent = reminder.IsSent,
            CreatedAt = reminder.CreatedAt,
            IsRecurring = reminder.IsRecurring,
            Recurrence = reminder.Recurrence?.ToString()
        };
    }

    private static AlertResponse MapToAlertResponse(Alert alert)
    {
        return new AlertResponse
        {
            Id = alert.Id,
            Title = alert.Title,
            Message = alert.Message,
            AlertType = alert.AlertType,
            IsAcknowledged = alert.IsAcknowledged,
            CreatedAt = alert.CreatedAt,
            AcknowledgedAt = alert.AcknowledgedAt
        };
    }
}
