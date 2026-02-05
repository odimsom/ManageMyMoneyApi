using ManageMyMoney.Core.Application.DTOs.Budgets;
using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Application.Features.Budgets;

public interface IBudgetService
{
    // Budgets
    Task<OperationResult<BudgetResponse>> CreateBudgetAsync(Guid userId, CreateBudgetRequest request);
    Task<OperationResult<BudgetResponse>> GetBudgetByIdAsync(Guid userId, Guid budgetId);
    Task<OperationResult<IEnumerable<BudgetResponse>>> GetBudgetsAsync(Guid userId, bool activeOnly = true);
    Task<OperationResult<BudgetResponse>> UpdateBudgetAsync(Guid userId, Guid budgetId, UpdateBudgetRequest request);
    Task<OperationResult> DeactivateBudgetAsync(Guid userId, Guid budgetId);

    // Progress & Tracking
    Task<OperationResult<BudgetProgressResponse>> GetBudgetProgressAsync(Guid userId, Guid budgetId);
    Task<OperationResult<IEnumerable<BudgetProgressResponse>>> GetAllBudgetsProgressAsync(Guid userId);
    Task<OperationResult> CheckBudgetAlertsAsync(Guid userId);

    // Savings Goals
    Task<OperationResult<SavingsGoalResponse>> CreateSavingsGoalAsync(Guid userId, CreateSavingsGoalRequest request);
    Task<OperationResult<SavingsGoalResponse>> GetSavingsGoalByIdAsync(Guid userId, Guid goalId);
    Task<OperationResult<IEnumerable<SavingsGoalResponse>>> GetSavingsGoalsAsync(Guid userId, string? status = null);
    Task<OperationResult<SavingsGoalResponse>> UpdateSavingsGoalAsync(Guid userId, Guid goalId, UpdateSavingsGoalRequest request);
    Task<OperationResult> PauseSavingsGoalAsync(Guid userId, Guid goalId);
    Task<OperationResult> ResumeSavingsGoalAsync(Guid userId, Guid goalId);
    Task<OperationResult> CancelSavingsGoalAsync(Guid userId, Guid goalId);

    // Contributions
    Task<OperationResult<ContributionResponse>> AddContributionAsync(Guid userId, Guid goalId, AddContributionRequest request);
    Task<OperationResult<IEnumerable<ContributionResponse>>> GetContributionsAsync(Guid userId, Guid goalId);
    Task<OperationResult> WithdrawFromGoalAsync(Guid userId, Guid goalId, decimal amount);
}
