using Microsoft.EntityFrameworkCore;
using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Budgets;
using ManageMyMoney.Core.Domain.Enums;
using ManageMyMoney.Core.Domain.Interfaces;
using ManageMyMoney.Infrastructure.Persistence.Context;
using ManageMyMoney.Infrastructure.Persistence.Repositories.Base;

namespace ManageMyMoney.Infrastructure.Persistence.Repositories.Budgets;

public class SavingsGoalRepository : GenericRepository<SavingsGoal>, ISavingsGoalRepository
{
    public SavingsGoalRepository(ManageMyMoneyContext context) : base(context)
    {
    }

    public async Task<OperationResult<IEnumerable<SavingsGoal>>> GetAllByUserAsync(Guid userId)
    {
        try
        {
            var goals = await DbSet
                .Where(g => g.UserId == userId)
                .Include(g => g.Contributions)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();

            return OperationResult.Success<IEnumerable<SavingsGoal>>(goals);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<IEnumerable<SavingsGoal>>($"Error retrieving goals: {ex.Message}");
        }
    }

    public async Task<OperationResult<IEnumerable<SavingsGoal>>> GetByStatusAsync(Guid userId, GoalStatus status)
    {
        try
        {
            var goals = await DbSet
                .Where(g => g.UserId == userId && g.Status == status)
                .Include(g => g.Contributions)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();

            return OperationResult.Success<IEnumerable<SavingsGoal>>(goals);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure<IEnumerable<SavingsGoal>>($"Error retrieving goals: {ex.Message}");
        }
    }

    public async Task<OperationResult> DeleteAsync(Guid id)
    {
        try
        {
            var goal = await DbSet.FindAsync(id);
            if (goal is null)
                return OperationResult.Failure("Savings goal not found");

            goal.Cancel();
            await Context.SaveChangesAsync();
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Error deleting savings goal: {ex.Message}");
        }
    }

    public async Task<OperationResult<bool>> ExistsAsync(Guid id)
    {
        return await ExistsAsync(g => g.Id == id);
    }
}
