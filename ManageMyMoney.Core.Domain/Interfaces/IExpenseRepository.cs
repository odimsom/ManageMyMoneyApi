using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Expenses;
using ManageMyMoney.Core.Domain.ValueObjects;

namespace ManageMyMoney.Core.Domain.Interfaces;

public interface IExpenseRepository
{
    Task<OperationResult<Expense>> GetByIdAsync(Guid id);
    Task<OperationResult<IEnumerable<Expense>>> GetAllByUserAsync(Guid userId);
    Task<OperationResult<IEnumerable<Expense>>> GetByUserAndDateRangeAsync(Guid userId, DateRange dateRange);
    Task<OperationResult<IEnumerable<Expense>>> GetByCategoryAsync(Guid categoryId);
    Task<OperationResult<IEnumerable<Expense>>> GetByAccountAsync(Guid accountId);
    Task<OperationResult<IEnumerable<Expense>>> GetByTagAsync(Guid tagId);
    Task<OperationResult<decimal>> GetTotalByUserAndDateRangeAsync(Guid userId, DateRange dateRange);
    Task<OperationResult<decimal>> GetTotalByCategoryAndDateRangeAsync(Guid categoryId, DateRange dateRange);
    Task<OperationResult> AddAsync(Expense expense);
    Task<OperationResult> UpdateAsync(Expense expense);
    Task<OperationResult> DeleteAsync(Guid id);
    Task<OperationResult<bool>> ExistsAsync(Guid id);
}
