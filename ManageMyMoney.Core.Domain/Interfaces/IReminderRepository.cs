using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Notifications;

namespace ManageMyMoney.Core.Domain.Interfaces;

public interface IReminderRepository
{
    Task<OperationResult<Reminder>> GetByIdAsync(Guid id);
    Task<OperationResult<IEnumerable<Reminder>>> GetByUserAsync(Guid userId, bool pendingOnly = true);
    Task<OperationResult<IEnumerable<Reminder>>> GetDueRemindersAsync();
    Task<OperationResult> AddAsync(Reminder reminder);
    Task<OperationResult> UpdateAsync(Reminder reminder);
    Task<OperationResult> DeleteAsync(Guid id);
}
