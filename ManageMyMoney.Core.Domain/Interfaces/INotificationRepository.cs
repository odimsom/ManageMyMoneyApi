using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Notifications;

namespace ManageMyMoney.Core.Domain.Interfaces;

public interface INotificationRepository
{
    Task<OperationResult<Notification>> GetByIdAsync(Guid id);
    Task<OperationResult<IEnumerable<Notification>>> GetAllByUserAsync(Guid userId);
    Task<OperationResult<IEnumerable<Notification>>> GetUnreadByUserAsync(Guid userId);
    Task<OperationResult<int>> GetUnreadCountByUserAsync(Guid userId);
    Task<OperationResult> AddAsync(Notification notification);
    Task<OperationResult> UpdateAsync(Notification notification);
    Task<OperationResult> DeleteAsync(Guid id);
    Task<OperationResult> MarkAllAsReadAsync(Guid userId);
}
