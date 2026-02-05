using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Categories;
using ManageMyMoney.Core.Domain.Enums;

namespace ManageMyMoney.Core.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<OperationResult<Category>> GetByIdAsync(Guid id);
    Task<OperationResult<IEnumerable<Category>>> GetAllByUserAsync(Guid userId);
    Task<OperationResult<IEnumerable<Category>>> GetByTransactionTypeAsync(Guid userId, TransactionType type);
    Task<OperationResult<IEnumerable<Category>>> GetDefaultCategoriesAsync();
    Task<OperationResult> AddAsync(Category category);
    Task<OperationResult> UpdateAsync(Category category);
    Task<OperationResult> DeleteAsync(Guid id);
    Task<OperationResult<bool>> ExistsAsync(Guid id);
    Task<OperationResult<bool>> NameExistsForUserAsync(string name, Guid userId);
}
