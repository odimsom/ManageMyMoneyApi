using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Notifications;

namespace ManageMyMoney.Core.Domain.Interfaces;

public interface IAlertRepository
{
    Task<OperationResult<Alert>> GetByIdAsync(Guid id);
    Task<OperationResult<IEnumerable<Alert>>> GetByUserAsync(Guid userId, bool unacknowledgedOnly = true);
    Task<OperationResult> AddAsync(Alert alert);
    Task<OperationResult> UpdateAsync(Alert alert);
    Task<OperationResult> DeleteAsync(Guid id);
}
