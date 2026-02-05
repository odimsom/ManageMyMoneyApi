using ManageMyMoney.Core.Application.DTOs.Notifications;
using ManageMyMoney.Core.Application.Features.Notifications;
using ManageMyMoney.Presentation.Api.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManageMyMoney.Presentation.Api.Controllers;

[Authorize]
public class NotificationsController : BaseApiController
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<NotificationResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications([FromQuery] bool unreadOnly = false)
    {
        var result = await _notificationService.GetNotificationsAsync(CurrentUserId, unreadOnly);
        return HandleResult(result);
    }

    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(ApiResponse<UnreadCountResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount()
    {
        var result = await _notificationService.GetUnreadCountAsync(CurrentUserId);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}/read")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var result = await _notificationService.MarkAsReadAsync(CurrentUserId, id);
        return HandleResult(result, "Notification marked as read");
    }

    [HttpPut("read-all")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var result = await _notificationService.MarkAllAsReadAsync(CurrentUserId);
        return HandleResult(result, "All notifications marked as read");
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteNotification(Guid id)
    {
        var result = await _notificationService.DeleteNotificationAsync(CurrentUserId, id);
        return HandleResult(result, "Notification deleted");
    }

    // Reminders
    [HttpPost("reminders")]
    [ProducesResponseType(typeof(ApiResponse<ReminderResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateReminder([FromBody] CreateReminderRequest request)
    {
        var result = await _notificationService.CreateReminderAsync(CurrentUserId, request);
        return HandleResult(result);
    }

    [HttpGet("reminders")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ReminderResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReminders([FromQuery] bool pendingOnly = true)
    {
        var result = await _notificationService.GetRemindersAsync(CurrentUserId, pendingOnly);
        return HandleResult(result);
    }

    [HttpPut("reminders/{id:guid}/complete")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CompleteReminder(Guid id)
    {
        var result = await _notificationService.CompleteReminderAsync(CurrentUserId, id);
        return HandleResult(result, "Reminder completed");
    }

    [HttpDelete("reminders/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteReminder(Guid id)
    {
        var result = await _notificationService.DeleteReminderAsync(CurrentUserId, id);
        return HandleResult(result, "Reminder deleted");
    }

    // Alerts
    [HttpGet("alerts")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AlertResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAlerts([FromQuery] bool unacknowledgedOnly = true)
    {
        var result = await _notificationService.GetAlertsAsync(CurrentUserId, unacknowledgedOnly);
        return HandleResult(result);
    }

    [HttpPut("alerts/{id:guid}/acknowledge")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> AcknowledgeAlert(Guid id)
    {
        var result = await _notificationService.AcknowledgeAlertAsync(CurrentUserId, id);
        return HandleResult(result, "Alert acknowledged");
    }
}
