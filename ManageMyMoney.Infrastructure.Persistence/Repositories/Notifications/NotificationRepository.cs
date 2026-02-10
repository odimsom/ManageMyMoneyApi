using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Notifications;
using ManageMyMoney.Core.Domain.Interfaces;
using ManageMyMoney.Infrastructure.Persistence.Context;
using ManageMyMoney.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace ManageMyMoney.Infrastructure.Persistence.Repositories.Notifications;

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(ManageMyMoneyContext context) : base(context)
    {
    }

    public async Task<OperationResult<IEnumerable<Notification>>> GetAllByUserAsync(Guid userId)
    {
        try
        {
            var notifications = await DbSet
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return OperationResult.Success<IEnumerable<Notification>>(notifications);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<IEnumerable<Notification>>($"Error retrieving notifications: {ex.Message}");
        }
    }

    public async Task<OperationResult<IEnumerable<Notification>>> GetUnreadByUserAsync(Guid userId)
    {
        try
        {
            var notifications = await DbSet
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return OperationResult.Success<IEnumerable<Notification>>(notifications);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<IEnumerable<Notification>>($"Error retrieving unread notifications: {ex.Message}");
        }
    }

    public async Task<OperationResult<int>> GetUnreadCountByUserAsync(Guid userId)
    {
        try
        {
            var count = await DbSet.CountAsync(n => n.UserId == userId && !n.IsRead);
            return OperationResult.Success(count);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<int>($"Error counting unread notifications: {ex.Message}");
        }
    }

    public async Task<OperationResult> MarkAllAsReadAsync(Guid userId)
    {
        try
        {
            var unreadNotifications = await DbSet
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.MarkAsRead();
            }

            await Context.SaveChangesAsync();
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Error marking all notifications as read: {ex.Message}");
        }
    }

    public async Task<OperationResult> DeleteAsync(Guid id)
    {
        try
        {
            var entity = await DbSet.FindAsync(id);
            if (entity == null)
                return OperationResult.Failure("Notification not found");

            DbSet.Remove(entity);
            await Context.SaveChangesAsync();
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Error deleting notification: {ex.Message}");
        }
    }
}
