using Microsoft.EntityFrameworkCore;
using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Accounts;
using ManageMyMoney.Core.Domain.Interfaces;
using ManageMyMoney.Infrastructure.Persistence.Context;
using ManageMyMoney.Infrastructure.Persistence.Repositories.Base;

namespace ManageMyMoney.Infrastructure.Persistence.Repositories.Accounts;

public class AccountRepository : GenericRepository<Account>, IAccountRepository
{
    public AccountRepository(ManageMyMoneyContext context) : base(context)
    {
    }

    public async Task<OperationResult<IEnumerable<Account>>> GetAllByUserAsync(Guid userId)
    {
        try
        {
            var accounts = await DbSet
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.Name)
                .ToListAsync();

            return OperationResult.Success<IEnumerable<Account>>(accounts);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<IEnumerable<Account>>($"Error retrieving accounts: {ex.Message}");
        }
    }

    public async Task<OperationResult<IEnumerable<Account>>> GetActiveByUserAsync(Guid userId)
    {
        try
        {
            var accounts = await DbSet
                .Where(a => a.UserId == userId && a.IsActive)
                .OrderBy(a => a.Name)
                .ToListAsync();

            return OperationResult.Success<IEnumerable<Account>>(accounts);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<IEnumerable<Account>>($"Error retrieving accounts: {ex.Message}");
        }
    }

    public async Task<OperationResult<decimal>> GetTotalBalanceByUserAsync(Guid userId, string? currency = null)
    {
        try
        {
            var query = DbSet.Where(a => a.UserId == userId && a.IsActive && a.IncludeInTotal);
            
            if (!string.IsNullOrEmpty(currency))
                query = query.Where(a => a.Currency == currency);

            var total = await query.SumAsync(a => a.Balance.Amount);
            return OperationResult.Success(total);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<decimal>($"Error calculating balance: {ex.Message}");
        }
    }

    public async Task<OperationResult<bool>> NameExistsForUserAsync(string name, Guid userId)
    {
        try
        {
            var exists = await DbSet.AnyAsync(a => 
                a.UserId == userId && 
                a.Name.ToLower() == name.ToLower());

            return OperationResult.Success(exists);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<bool>($"Error checking account name: {ex.Message}");
        }
    }

    public async Task<OperationResult> DeleteAsync(Guid id)
    {
        try
        {
            var account = await DbSet.FindAsync(id);
            if (account is null)
                return OperationResult.Failure("Account not found");

            var result = account.Deactivate();
            if (result.IsFailure)
                return result;

            await Context.SaveChangesAsync();
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Error deleting account: {ex.Message}");
        }
    }

    public async Task<OperationResult<bool>> ExistsAsync(Guid id)
    {
        return await ExistsAsync(a => a.Id == id);
    }
}
