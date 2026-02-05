using Microsoft.EntityFrameworkCore;
using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Budgets;
using ManageMyMoney.Core.Domain.Interfaces;
using ManageMyMoney.Infrastructure.Persistence.Context;
using ManageMyMoney.Infrastructure.Persistence.Repositories.Base;

namespace ManageMyMoney.Infrastructure.Persistence.Repositories.Budgets;

public class BudgetRepository : GenericRepository<Budget>, IBudgetRepository
{
    public BudgetRepository(ManageMyMoneyContext context) : base(context)
    {
    }

    public async Task<OperationResult<IEnumerable<Budget>>> GetAllByUserAsync(Guid userId)
    {
        try
        {
            var budgets = await DbSet
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return OperationResult.Success<IEnumerable<Budget>>(budgets);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<IEnumerable<Budget>>($"Error retrieving budgets: {ex.Message}");
        }
    }

    public async Task<OperationResult<IEnumerable<Budget>>> GetActiveByUserAsync(Guid userId)
    {
        try
        {
            var now = DateTime.UtcNow;
            var budgets = await DbSet
                .Where(b => b.UserId == userId 
                    && b.IsActive 
                    && b.DateRange.StartDate <= now 
                    && b.DateRange.EndDate >= now)
                .OrderBy(b => b.Name)
                .ToListAsync();

            return OperationResult.Success<IEnumerable<Budget>>(budgets);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<IEnumerable<Budget>>($"Error retrieving budgets: {ex.Message}");
        }
    }

    public async Task<OperationResult<IEnumerable<Budget>>> GetByCategoryAsync(Guid categoryId)
    {
        try
        {
            var budgets = await DbSet
                .Where(b => b.CategoryIds.Contains(categoryId) && b.IsActive)
                .ToListAsync();

            return OperationResult.Success<IEnumerable<Budget>>(budgets);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<IEnumerable<Budget>>($"Error retrieving budgets: {ex.Message}");
        }
    }

    public async Task<OperationResult> DeleteAsync(Guid id)
    {
        try
        {
            var budget = await DbSet.FindAsync(id);
            if (budget is null)
                return OperationResult.Failure("Budget not found");

            budget.Deactivate();
            await Context.SaveChangesAsync();
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Error deleting budget: {ex.Message}");
        }
    }

    public async Task<OperationResult<bool>> ExistsAsync(Guid id)
    {
        return await ExistsAsync(b => b.Id == id);
    }
}
