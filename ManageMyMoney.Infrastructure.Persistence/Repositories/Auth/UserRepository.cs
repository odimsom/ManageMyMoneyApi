using Microsoft.EntityFrameworkCore;
using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Auth;
using ManageMyMoney.Core.Domain.Interfaces;
using ManageMyMoney.Infrastructure.Persistence.Context;
using ManageMyMoney.Infrastructure.Persistence.Repositories.Base;

namespace ManageMyMoney.Infrastructure.Persistence.Repositories.Auth;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(ManageMyMoneyContext context) : base(context)
    {
    }

    public async Task<OperationResult<User>> GetByEmailAsync(string email)
    {
        try
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var user = await DbSet
                .FirstOrDefaultAsync(u => u.Email.Value == normalizedEmail);

            return user is not null
                ? OperationResult.Success(user)
                : OperationResult.Failure<User>("User not found");
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<User>($"Error retrieving user: {ex.Message}");
        }
    }

    public async Task<OperationResult<bool>> EmailExistsAsync(string email)
    {
        try
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var exists = await DbSet.AnyAsync(u => u.Email.Value == normalizedEmail);
            return OperationResult.Success(exists);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<bool>($"Error checking email: {ex.Message}");
        }
    }

    public async Task<OperationResult> DeleteAsync(Guid id)
    {
        try
        {
            var user = await DbSet.FindAsync(id);
            if (user is null)
                return OperationResult.Failure("User not found");

            DbSet.Remove(user);
            await Context.SaveChangesAsync();
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Error deleting user: {ex.Message}");
        }
    }
}
