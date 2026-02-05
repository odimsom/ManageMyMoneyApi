using Microsoft.EntityFrameworkCore;
using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Expenses;
using ManageMyMoney.Core.Domain.Interfaces;
using ManageMyMoney.Core.Domain.ValueObjects;
using ManageMyMoney.Infrastructure.Persistence.Context;
using ManageMyMoney.Infrastructure.Persistence.Repositories.Base;

namespace ManageMyMoney.Infrastructure.Persistence.Repositories.Expenses;

public class ExpenseRepository : GenericRepository<Expense>, IExpenseRepository
{
    public ExpenseRepository(ManageMyMoneyContext context) : base(context)
    {
    }

    public async Task<OperationResult<IEnumerable<Expense>>> GetAllByUserAsync(Guid userId)
    {
        try
        {
            var expenses = await DbSet
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            return OperationResult.Success<IEnumerable<Expense>>(expenses);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<IEnumerable<Expense>>($"Error retrieving expenses: {ex.Message}");
        }
    }

    public async Task<OperationResult<IEnumerable<Expense>>> GetByUserAndDateRangeAsync(Guid userId, DateRange dateRange)
    {
        try
        {
            var expenses = await DbSet
                .Where(e => e.UserId == userId 
                    && e.Date >= dateRange.StartDate 
                    && e.Date <= dateRange.EndDate)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            return OperationResult.Success<IEnumerable<Expense>>(expenses);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<IEnumerable<Expense>>($"Error retrieving expenses: {ex.Message}");
        }
    }

    public async Task<OperationResult<IEnumerable<Expense>>> GetByCategoryAsync(Guid categoryId)
    {
        try
        {
            var expenses = await DbSet
                .Where(e => e.CategoryId == categoryId)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            return OperationResult.Success<IEnumerable<Expense>>(expenses);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<IEnumerable<Expense>>($"Error retrieving expenses: {ex.Message}");
        }
    }

    public async Task<OperationResult<IEnumerable<Expense>>> GetByAccountAsync(Guid accountId)
    {
        try
        {
            var expenses = await DbSet
                .Where(e => e.AccountId == accountId)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            return OperationResult.Success<IEnumerable<Expense>>(expenses);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<IEnumerable<Expense>>($"Error retrieving expenses: {ex.Message}");
        }
    }

    public async Task<OperationResult<IEnumerable<Expense>>> GetByTagAsync(Guid tagId)
    {
        try
        {
            var expenses = await DbSet
                .Where(e => e.Tags.Any(t => t.Id == tagId))
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            return OperationResult.Success<IEnumerable<Expense>>(expenses);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<IEnumerable<Expense>>($"Error retrieving expenses: {ex.Message}");
        }
    }

    public async Task<OperationResult<decimal>> GetTotalByUserAndDateRangeAsync(Guid userId, DateRange dateRange)
    {
        try
        {
            var total = await DbSet
                .Where(e => e.UserId == userId 
                    && e.Date >= dateRange.StartDate 
                    && e.Date <= dateRange.EndDate)
                .SumAsync(e => e.Amount.Amount);

            return OperationResult.Success(total);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<decimal>($"Error calculating total: {ex.Message}");
        }
    }

    public async Task<OperationResult<decimal>> GetTotalByCategoryAndDateRangeAsync(Guid categoryId, DateRange dateRange)
    {
        try
        {
            var total = await DbSet
                .Where(e => e.CategoryId == categoryId 
                    && e.Date >= dateRange.StartDate 
                    && e.Date <= dateRange.EndDate)
                .SumAsync(e => e.Amount.Amount);

            return OperationResult.Success(total);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<decimal>($"Error calculating total: {ex.Message}");
        }
    }

    public async Task<OperationResult> DeleteAsync(Guid id)
    {
        try
        {
            var expense = await DbSet.FindAsync(id);
            if (expense is null)
                return OperationResult.Failure("Expense not found");

            DbSet.Remove(expense);
            await Context.SaveChangesAsync();
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Error deleting expense: {ex.Message}");
        }
    }

    public async Task<OperationResult<bool>> ExistsAsync(Guid id)
    {
        return await ExistsAsync(e => e.Id == id);
    }
}
