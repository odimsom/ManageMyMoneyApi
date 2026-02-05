using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Income;
using ManageMyMoney.Core.Domain.ValueObjects;

namespace ManageMyMoney.Core.Domain.Interfaces;

public interface IIncomeRepository
{
    Task<OperationResult<Income>> GetByIdAsync(Guid id);
    Task<OperationResult<IEnumerable<Income>>> GetAllByUserAsync(Guid userId);
    Task<OperationResult<IEnumerable<Income>>> GetByUserAndDateRangeAsync(Guid userId, DateRange dateRange);
    Task<OperationResult<IEnumerable<Income>>> GetBySourceAsync(Guid incomeSourceId);
    Task<OperationResult<decimal>> GetTotalByUserAndDateRangeAsync(Guid userId, DateRange dateRange);
    Task<OperationResult> AddAsync(Income income);
    Task<OperationResult> UpdateAsync(Income income);
    Task<OperationResult> DeleteAsync(Guid id);
    Task<OperationResult<bool>> ExistsAsync(Guid id);
}
