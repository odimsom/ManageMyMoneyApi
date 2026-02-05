using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Infrastructure.Persistence.Context;

namespace ManageMyMoney.Infrastructure.Persistence.Repositories.Base;

public class GenericRepository<T> where T : class
{
    protected readonly ManageMyMoneyContext Context;
    protected readonly DbSet<T> DbSet;

    public GenericRepository(ManageMyMoneyContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<OperationResult<T>> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await DbSet.FindAsync(id);
            return entity is not null
                ? OperationResult.Success(entity)
                : OperationResult.Failure<T>("Entity not found");
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<T>($"Error retrieving entity: {ex.Message}");
        }
    }

    public virtual async Task<OperationResult<IEnumerable<T>>> GetAllAsync()
    {
        try
        {
            var entities = await DbSet.ToListAsync();
            return OperationResult.Success<IEnumerable<T>>(entities);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<IEnumerable<T>>($"Error retrieving entities: {ex.Message}");
        }
    }

    public virtual async Task<OperationResult<IEnumerable<T>>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            var entities = await DbSet.Where(predicate).ToListAsync();
            return OperationResult.Success<IEnumerable<T>>(entities);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<IEnumerable<T>>($"Error finding entities: {ex.Message}");
        }
    }

    public virtual async Task<OperationResult> AddAsync(T entity)
    {
        try
        {
            await DbSet.AddAsync(entity);
            await Context.SaveChangesAsync();
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Error adding entity: {ex.Message}");
        }
    }

    public virtual async Task<OperationResult> UpdateAsync(T entity)
    {
        try
        {
            DbSet.Update(entity);
            await Context.SaveChangesAsync();
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Error updating entity: {ex.Message}");
        }
    }

    public virtual async Task<OperationResult> DeleteAsync(T entity)
    {
        try
        {
            DbSet.Remove(entity);
            await Context.SaveChangesAsync();
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Error deleting entity: {ex.Message}");
        }
    }

    public virtual async Task<OperationResult<bool>> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            var exists = await DbSet.AnyAsync(predicate);
            return OperationResult.Success(exists);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<bool>($"Error checking existence: {ex.Message}");
        }
    }

    public virtual async Task<OperationResult<int>> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        try
        {
            var count = predicate is null
                ? await DbSet.CountAsync()
                : await DbSet.CountAsync(predicate);
            return OperationResult.Success(count);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<int>($"Error counting entities: {ex.Message}");
        }
    }
}
