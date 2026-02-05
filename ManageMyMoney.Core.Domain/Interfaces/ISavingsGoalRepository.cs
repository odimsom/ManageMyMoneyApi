using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Budgets;
using ManageMyMoney.Core.Domain.Enums;

namespace ManageMyMoney.Core.Domain.Interfaces;

public interface ISavingsGoalRepository
{
    Task<OperationResult<SavingsGoal>> GetByIdAsync(Guid id);
    Task<OperationResult<IEnumerable<SavingsGoal>>> GetAllByUserAsync(Guid userId);
    Task<OperationResult<IEnumerable<SavingsGoal>>> GetByStatusAsync(Guid userId, GoalStatus status);
    Task<OperationResult> AddAsync(SavingsGoal goal);
    Task<OperationResult> UpdateAsync(SavingsGoal goal);
    Task<OperationResult> DeleteAsync(Guid id);
    Task<OperationResult<bool>> ExistsAsync(Guid id);
}
