using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Income;
using ManageMyMoney.Core.Domain.Interfaces;
using ManageMyMoney.Infrastructure.Persistence.Context;
using ManageMyMoney.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace ManageMyMoney.Infrastructure.Persistence.Repositories.Income;

public class RecurringIncomeRepository : GenericRepository<RecurringIncome>, IRecurringIncomeRepository
{
    public RecurringIncomeRepository(ManageMyMoneyContext context) : base(context)
    {
    }

    public async Task<OperationResult<IEnumerable<RecurringIncome>>> GetAllByUserAsync(Guid userId)
    {
        try
        {
            var recurringIncomes = await DbSet
                .Where(ri => ri.UserId == userId)
                .OrderBy(ri => ri.Name)
                .ToListAsync();

            return OperationResult.Success<IEnumerable<RecurringIncome>>(recurringIncomes);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<IEnumerable<RecurringIncome>>($"Error retrieving recurring incomes: {ex.Message}");
        }
    }

    public async Task<OperationResult<IEnumerable<RecurringIncome>>> GetActiveByUserAsync(Guid userId)
    {
        try
        {
            var recurringIncomes = await DbSet
                .Where(ri => ri.UserId == userId && ri.IsActive)
                .OrderBy(ri => ri.Name)
                .ToListAsync();

            return OperationResult.Success<IEnumerable<RecurringIncome>>(recurringIncomes);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<IEnumerable<RecurringIncome>>($"Error retrieving active recurring incomes: {ex.Message}");
        }
    }

    public async Task<OperationResult> DeleteAsync(Guid id)
    {
        try
        {
            var entity = await DbSet.FindAsync(id);
            if (entity == null)
                return OperationResult.Failure("Recurring income not found");

            DbSet.Remove(entity);
            await Context.SaveChangesAsync();
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Error deleting recurring income: {ex.Message}");
        }
    }

    public async Task<OperationResult<bool>> ExistsAsync(Guid id)
    {
        try
        {
            var exists = await DbSet.AnyAsync(ri => ri.Id == id);
            return OperationResult.Success(exists);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<bool>($"Error checking recurring income existence: {ex.Message}");
        }
    }
}
