using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Accounts;

namespace ManageMyMoney.Core.Domain.Interfaces;

public interface IAccountRepository
{
    Task<OperationResult<Account>> GetByIdAsync(Guid id);
    Task<OperationResult<IEnumerable<Account>>> GetAllByUserAsync(Guid userId);
    Task<OperationResult<IEnumerable<Account>>> GetActiveByUserAsync(Guid userId);
    Task<OperationResult<decimal>> GetTotalBalanceByUserAsync(Guid userId, string? currency = null);
    Task<OperationResult> AddAsync(Account account);
    Task<OperationResult> UpdateAsync(Account account);
    Task<OperationResult> DeleteAsync(Guid id);
    Task<OperationResult<bool>> ExistsAsync(Guid id);
    Task<OperationResult<bool>> NameExistsForUserAsync(string name, Guid userId);
}
