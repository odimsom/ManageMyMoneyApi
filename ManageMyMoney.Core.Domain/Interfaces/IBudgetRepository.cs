using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Budgets;

namespace ManageMyMoney.Core.Domain.Interfaces;

public interface IBudgetRepository
{
    Task<OperationResult<Budget>> GetByIdAsync(Guid id);
    Task<OperationResult<IEnumerable<Budget>>> GetAllByUserAsync(Guid userId);
    Task<OperationResult<IEnumerable<Budget>>> GetActiveByUserAsync(Guid userId);
    Task<OperationResult<IEnumerable<Budget>>> GetByCategoryAsync(Guid categoryId);
    Task<OperationResult> AddAsync(Budget budget);
    Task<OperationResult> UpdateAsync(Budget budget);
    Task<OperationResult> DeleteAsync(Guid id);
    Task<OperationResult<bool>> ExistsAsync(Guid id);
}
