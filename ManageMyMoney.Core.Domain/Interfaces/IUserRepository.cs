using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Auth;

namespace ManageMyMoney.Core.Domain.Interfaces;

public interface IUserRepository
{
    Task<OperationResult<User>> GetByIdAsync(Guid id);
    Task<OperationResult<User>> GetByEmailAsync(string email);
    Task<OperationResult> AddAsync(User user);
    Task<OperationResult> UpdateAsync(User user);
    Task<OperationResult> DeleteAsync(Guid id);
    Task<OperationResult<bool>> EmailExistsAsync(string email);
}
