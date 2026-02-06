using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Income;
using ManageMyMoney.Core.Domain.Interfaces;
using ManageMyMoney.Core.Domain.ValueObjects;
using ManageMyMoney.Infrastructure.Persistence.Context;
using ManageMyMoney.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace ManageMyMoney.Infrastructure.Persistence.Repositories.Income;

public class IncomeRepository : GenericRepository<Core.Domain.Entities.Income.Income>, IIncomeRepository
{
    public IncomeRepository(ManageMyMoneyContext context) : base(context)
    {
    }

    public async Task<OperationResult<IEnumerable<Core.Domain.Entities.Income.Income>>> GetAllByUserAsync(Guid userId)
    {
        try
        {
            var incomes = await DbSet
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.Date)
                .ToListAsync();

            return OperationResult.Success<IEnumerable<Core.Domain.Entities.Income.Income>>(incomes);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<IEnumerable<Core.Domain.Entities.Income.Income>>($"Error retrieving incomes: {ex.Message}");
        }
    }

    public async Task<OperationResult<IEnumerable<Core.Domain.Entities.Income.Income>>> GetByUserAndDateRangeAsync(Guid userId, DateRange dateRange)
    {
        try
        {
            var incomes = await DbSet
                .Where(i => i.UserId == userId && 
                           i.Date >= dateRange.StartDate && 
                           i.Date <= dateRange.EndDate)
                .OrderByDescending(i => i.Date)
                .ToListAsync();

            return OperationResult.Success<IEnumerable<Core.Domain.Entities.Income.Income>>(incomes);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<IEnumerable<Core.Domain.Entities.Income.Income>>($"Error retrieving incomes by date range: {ex.Message}");
        }
    }

    public async Task<OperationResult<IEnumerable<Core.Domain.Entities.Income.Income>>> GetBySourceAsync(Guid incomeSourceId)
    {
        try
        {
            var incomes = await DbSet
                .Where(i => i.IncomeSourceId == incomeSourceId)
                .OrderByDescending(i => i.Date)
                .ToListAsync();

            return OperationResult.Success<IEnumerable<Core.Domain.Entities.Income.Income>>(incomes);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<IEnumerable<Core.Domain.Entities.Income.Income>>($"Error retrieving incomes by source: {ex.Message}");
        }
    }

    public async Task<OperationResult<decimal>> GetTotalByUserAndDateRangeAsync(Guid userId, DateRange dateRange)
    {
        try
        {
            var total = await DbSet
                .Where(i => i.UserId == userId && 
                           i.Date >= dateRange.StartDate && 
                           i.Date <= dateRange.EndDate)
                .SumAsync(i => i.Amount.Amount);

            return OperationResult.Success(total);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<decimal>($"Error calculating income total: {ex.Message}");
        }
    }

    public async Task<OperationResult> DeleteAsync(Guid id)
    {
        try
        {
            var entity = await DbSet.FindAsync(id);
            if (entity == null)
                return OperationResult.Failure("Income not found");

            DbSet.Remove(entity);
            await Context.SaveChangesAsync();
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Error deleting income: {ex.Message}");
        }
    }

    public async Task<OperationResult<bool>> ExistsAsync(Guid id)
    {
        try
        {
            var exists = await DbSet.AnyAsync(e => EF.Property<Guid>(e, "Id") == id);
            return OperationResult.Success(exists);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<bool>($"Error checking income existence: {ex.Message}");
        }
    }
}