using Microsoft.EntityFrameworkCore;
using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Categories;
using ManageMyMoney.Core.Domain.Enums;
using ManageMyMoney.Core.Domain.Interfaces;
using ManageMyMoney.Infrastructure.Persistence.Context;
using ManageMyMoney.Infrastructure.Persistence.Repositories.Base;

namespace ManageMyMoney.Infrastructure.Persistence.Repositories.Categories;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(ManageMyMoneyContext context) : base(context)
    {
    }

    public async Task<OperationResult<IEnumerable<Category>>> GetAllByUserAsync(Guid userId)
    {
        try
        {
            var categories = await DbSet
                .Where(c => c.UserId == userId && c.IsActive)
                .Include(c => c.Subcategories.Where(s => s.IsActive))
                .OrderBy(c => c.Name)
                .ToListAsync();

            return OperationResult.Success<IEnumerable<Category>>(categories);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<IEnumerable<Category>>($"Error retrieving categories: {ex.Message}");
        }
    }

    public async Task<OperationResult<IEnumerable<Category>>> GetByTransactionTypeAsync(Guid userId, TransactionType type)
    {
        try
        {
            var categories = await DbSet
                .Where(c => c.UserId == userId && c.TransactionType == type && c.IsActive)
                .Include(c => c.Subcategories.Where(s => s.IsActive))
                .OrderBy(c => c.Name)
                .ToListAsync();

            return OperationResult.Success<IEnumerable<Category>>(categories);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<IEnumerable<Category>>($"Error retrieving categories: {ex.Message}");
        }
    }

    public async Task<OperationResult<IEnumerable<Category>>> GetDefaultCategoriesAsync()
    {
        try
        {
            var categories = await DbSet
                .Where(c => c.IsDefault && c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return OperationResult.Success<IEnumerable<Category>>(categories);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<IEnumerable<Category>>($"Error retrieving default categories: {ex.Message}");
        }
    }

    public async Task<OperationResult<bool>> NameExistsForUserAsync(string name, Guid userId)
    {
        try
        {
            var exists = await DbSet.AnyAsync(c => 
                c.UserId == userId && 
                c.Name.ToLower() == name.ToLower() && 
                c.IsActive);

            return OperationResult.Success(exists);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<bool>($"Error checking category name: {ex.Message}");
        }
    }

    public async Task<OperationResult> DeleteAsync(Guid id)
    {
        try
        {
            var category = await DbSet.FindAsync(id);
            if (category is null)
                return OperationResult.Failure("Category not found");

            category.Deactivate();
            await Context.SaveChangesAsync();
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Error deleting category: {ex.Message}");
        }
    }

    public async Task<OperationResult<bool>> ExistsAsync(Guid id)
    {
        return await ExistsAsync(c => c.Id == id && c.IsActive);
    }
}
