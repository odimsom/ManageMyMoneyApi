using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Income;

namespace ManageMyMoney.Core.Domain.Interfaces;

public interface IIncomeSourceRepository
{
    Task<OperationResult<IncomeSource>> GetByIdAsync(Guid id);
    Task<OperationResult<IEnumerable<IncomeSource>>> GetAllByUserAsync(Guid userId);
    Task<OperationResult> AddAsync(IncomeSource source);
    Task<OperationResult> UpdateAsync(IncomeSource source);
    Task<OperationResult> DeleteAsync(Guid id);
    Task<OperationResult<bool>> ExistsAsync(Guid id);
}
