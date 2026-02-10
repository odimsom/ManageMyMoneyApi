using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Income;

namespace ManageMyMoney.Core.Domain.Interfaces;

public interface IRecurringIncomeRepository
{
    Task<OperationResult<RecurringIncome>> GetByIdAsync(Guid id);
    Task<OperationResult<IEnumerable<RecurringIncome>>> GetAllByUserAsync(Guid userId);
    Task<OperationResult<IEnumerable<RecurringIncome>>> GetActiveByUserAsync(Guid userId);
    Task<OperationResult> AddAsync(RecurringIncome recurringIncome);
    Task<OperationResult> UpdateAsync(RecurringIncome recurringIncome);
    Task<OperationResult> DeleteAsync(Guid id);
    Task<OperationResult<bool>> ExistsAsync(Guid id);
}
