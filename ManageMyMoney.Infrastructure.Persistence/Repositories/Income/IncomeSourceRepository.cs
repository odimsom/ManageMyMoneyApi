using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Income;
using ManageMyMoney.Core.Domain.Interfaces;
using ManageMyMoney.Infrastructure.Persistence.Context;
using ManageMyMoney.Infrastructure.Persistence.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace ManageMyMoney.Infrastructure.Persistence.Repositories.Income;

public class IncomeSourceRepository : GenericRepository<IncomeSource>, IIncomeSourceRepository
{
    public IncomeSourceRepository(ManageMyMoneyContext context) : base(context)
    {
    }

    public async Task<OperationResult<IEnumerable<IncomeSource>>> GetAllByUserAsync(Guid userId)
    {
        try
        {
            var sources = await DbSet
                .Where(s => s.UserId == userId && s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return OperationResult.Success<IEnumerable<IncomeSource>>(sources);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<IEnumerable<IncomeSource>>($"Error retrieving income sources: {ex.Message}");
        }
    }

    public async Task<OperationResult> DeleteAsync(Guid id)
    {
        try
        {
            var entity = await DbSet.FindAsync(id);
            if (entity == null)
                return OperationResult.Failure("Income source not found");

            DbSet.Remove(entity);
            await Context.SaveChangesAsync();
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Error deleting income source: {ex.Message}");
        }
    }

    public async Task<OperationResult<bool>> ExistsAsync(Guid id)
    {
        try
        {
            var exists = await DbSet.AnyAsync(s => s.Id == id);
            return OperationResult.Success(exists);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<bool>($"Error checking income source existence: {ex.Message}");
        }
    }
}
