using ManageMyMoney.Core.Application.DTOs.Budgets;
using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Budgets;
using ManageMyMoney.Core.Domain.Enums;
using ManageMyMoney.Core.Domain.Interfaces;
using ManageMyMoney.Core.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ManageMyMoney.Core.Application.Features.Budgets;

public class BudgetService : IBudgetService
{
    private readonly ILogger<BudgetService> _logger;
    private readonly IBudgetRepository _budgetRepository;
    private readonly ISavingsGoalRepository _savingsGoalRepository;

    public BudgetService(
        ILogger<BudgetService> logger, 
        IBudgetRepository budgetRepository,
        ISavingsGoalRepository savingsGoalRepository)
    {
        _logger = logger;
        _budgetRepository = budgetRepository;
        _savingsGoalRepository = savingsGoalRepository;
    }

    public async Task<OperationResult<BudgetResponse>> CreateBudgetAsync(Guid userId, CreateBudgetRequest request)
    {
        try
        {
            _logger.LogInformation("Creating budget {BudgetName} for user {UserId}", request.Name, userId);

            // Convertir string a enum
            if (!Enum.TryParse<BudgetPeriod>(request.Period, true, out var budgetPeriod))
                return OperationResult.Failure<BudgetResponse>("Invalid budget period");

            // Crear presupuesto
            var createResult = Budget.Create(
                request.Name,
                request.LimitAmount,
                request.Currency,
                budgetPeriod,
                userId,
                request.StartDate,
                request.EndDate,
                request.Description,
                request.AlertsEnabled
            );

            if (!createResult.IsSuccess)
                return OperationResult.Failure<BudgetResponse>(createResult.Error);

            var budget = createResult.Value;

            // Agregar categorías si se especificaron
            if (request.CategoryIds != null)
            {
                foreach (var categoryId in request.CategoryIds)
                {
                    budget.AddCategory(categoryId);
                }
            }

            // Guardar en el repositorio
            var addResult = await _budgetRepository.AddAsync(budget);
            if (!addResult.IsSuccess)
                return OperationResult.Failure<BudgetResponse>(addResult.Error);

            var response = MapToResponse(budget);
            _logger.LogInformation("Budget {BudgetId} created successfully for user {UserId}", budget.Id, userId);
            
            return OperationResult.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating budget for user {UserId}", userId);
            return OperationResult.Failure<BudgetResponse>("An error occurred while creating the budget");
        }
    }

    public async Task<OperationResult<BudgetResponse>> GetBudgetByIdAsync(Guid userId, Guid budgetId)
    {
        try
        {
            _logger.LogInformation("Getting budget {BudgetId} for user {UserId}", budgetId, userId);

            var result = await _budgetRepository.GetByIdAsync(budgetId);
            if (!result.IsSuccess)
                return OperationResult.Failure<BudgetResponse>(result.Error);

            if (result.Value.UserId != userId)
                return OperationResult.Failure<BudgetResponse>("Budget not found");

            var response = MapToResponse(result.Value);
            return OperationResult.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting budget {BudgetId} for user {UserId}", budgetId, userId);
            return OperationResult.Failure<BudgetResponse>("An error occurred while retrieving the budget");
        }
    }

    public async Task<OperationResult<IEnumerable<BudgetResponse>>> GetBudgetsAsync(Guid userId, bool activeOnly = true)
    {
        try
        {
            _logger.LogInformation("Getting budgets for user {UserId}, activeOnly: {ActiveOnly}", userId, activeOnly);

            var result = await _budgetRepository.GetActiveByUserAsync(userId);
            if (!result.IsSuccess)
                return OperationResult.Failure<IEnumerable<BudgetResponse>>(result.Error);

            var responses = result.Value.Select(MapToResponse);
            return OperationResult.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting budgets for user {UserId}", userId);
            return OperationResult.Failure<IEnumerable<BudgetResponse>>("An error occurred while retrieving budgets");
        }
    }

    public async Task<OperationResult<BudgetResponse>> UpdateBudgetAsync(Guid userId, Guid budgetId, UpdateBudgetRequest request)
    {
        try
        {
            _logger.LogInformation("Updating budget {BudgetId} for user {UserId}", budgetId, userId);

            // Obtener presupuesto existente
            var getBudgetResult = await _budgetRepository.GetByIdAsync(budgetId);
            if (!getBudgetResult.IsSuccess)
                return OperationResult.Failure<BudgetResponse>(getBudgetResult.Error);

            var budget = getBudgetResult.Value;
            if (budget.UserId != userId)
                return OperationResult.Failure<BudgetResponse>("Budget not found");

            // Actualizar límite si se especificó
            if (request.LimitAmount.HasValue)
            {
                var updateLimitResult = budget.UpdateLimit(request.LimitAmount.Value);
                if (!updateLimitResult.IsSuccess)
                    return OperationResult.Failure<BudgetResponse>(updateLimitResult.Error);
            }

            // Actualizar categorías si se especificaron
            if (request.CategoryIds != null)
            {
                // Limpiar categorías existentes y agregar las nuevas
                foreach (var existingCategoryId in budget.CategoryIds.ToList())
                {
                    budget.RemoveCategory(existingCategoryId);
                }

                foreach (var categoryId in request.CategoryIds)
                {
                    budget.AddCategory(categoryId);
                }
            }

            // Guardar cambios
            var updateResult = await _budgetRepository.UpdateAsync(budget);
            if (!updateResult.IsSuccess)
                return OperationResult.Failure<BudgetResponse>(updateResult.Error);

            var response = MapToResponse(budget);
            _logger.LogInformation("Budget {BudgetId} updated successfully", budgetId);
            
            return OperationResult.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating budget {BudgetId} for user {UserId}", budgetId, userId);
            return OperationResult.Failure<BudgetResponse>("An error occurred while updating the budget");
        }
    }

    public async Task<OperationResult> DeactivateBudgetAsync(Guid userId, Guid budgetId)
    {
        try
        {
            _logger.LogInformation("Deactivating budget {BudgetId} for user {UserId}", budgetId, userId);

            var getBudgetResult = await _budgetRepository.GetByIdAsync(budgetId);
            if (!getBudgetResult.IsSuccess)
                return OperationResult.Failure(getBudgetResult.Error);

            var budget = getBudgetResult.Value;
            if (budget.UserId != userId)
                return OperationResult.Failure("Budget not found");

            budget.Deactivate();

            var updateResult = await _budgetRepository.UpdateAsync(budget);
            if (!updateResult.IsSuccess)
                return OperationResult.Failure(updateResult.Error);

            _logger.LogInformation("Budget {BudgetId} deactivated successfully", budgetId);
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating budget {BudgetId} for user {UserId}", budgetId, userId);
            return OperationResult.Failure("An error occurred while deactivating the budget");
        }
    }

    public async Task<OperationResult<BudgetProgressResponse>> GetBudgetProgressAsync(Guid userId, Guid budgetId)
    {
        try
        {
            _logger.LogInformation("Getting budget progress for budget {BudgetId}, user {UserId}", budgetId, userId);
            
            // TODO: Implementar cuando se tengan gastos y se pueda calcular el progreso real
            return OperationResult.Failure<BudgetProgressResponse>("Budget progress tracking not yet available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting budget progress for budget {BudgetId}, user {UserId}", budgetId, userId);
            return OperationResult.Failure<BudgetProgressResponse>("An error occurred while retrieving budget progress");
        }
    }

    public async Task<OperationResult<IEnumerable<BudgetProgressResponse>>> GetAllBudgetsProgressAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Getting all budgets progress for user {UserId}", userId);
            
            // TODO: Implementar cuando se tengan gastos y se pueda calcular el progreso real
            var emptyList = Enumerable.Empty<BudgetProgressResponse>();
            return OperationResult.Success(emptyList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all budgets progress for user {UserId}", userId);
            return OperationResult.Failure<IEnumerable<BudgetProgressResponse>>("An error occurred while retrieving budgets progress");
        }
    }

    public async Task<OperationResult> CheckBudgetAlertsAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Checking budget alerts for user {UserId}", userId);
            
            // TODO: Implementar sistema de alertas cuando se tengan gastos reales
            _logger.LogInformation("Budget alerts checked for user {UserId}", userId);
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking budget alerts for user {UserId}", userId);
            return OperationResult.Failure("An error occurred while checking budget alerts");
        }
    }

    // Savings Goals methods
    public async Task<OperationResult<SavingsGoalResponse>> CreateSavingsGoalAsync(Guid userId, CreateSavingsGoalRequest request)
    {
        try
        {
            _logger.LogInformation("Creating savings goal {GoalName} for user {UserId}", request.Name, userId);

            // TODO: Implementar cuando se complete la entidad SavingsGoal
            return OperationResult.Failure<SavingsGoalResponse>("Savings goal functionality not yet available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating savings goal for user {UserId}", userId);
            return OperationResult.Failure<SavingsGoalResponse>("An error occurred while creating the savings goal");
        }
    }

    public async Task<OperationResult<SavingsGoalResponse>> GetSavingsGoalByIdAsync(Guid userId, Guid goalId)
    {
        try
        {
            _logger.LogInformation("Getting savings goal {GoalId} for user {UserId}", goalId, userId);
            
            return OperationResult.Failure<SavingsGoalResponse>("Savings goal functionality not yet available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting savings goal {GoalId} for user {UserId}", goalId, userId);
            return OperationResult.Failure<SavingsGoalResponse>("An error occurred while retrieving the savings goal");
        }
    }

    public async Task<OperationResult<IEnumerable<SavingsGoalResponse>>> GetSavingsGoalsAsync(Guid userId, string? status = null)
    {
        try
        {
            _logger.LogInformation("Getting savings goals for user {UserId}, status: {Status}", userId, status);
            
            var emptyList = Enumerable.Empty<SavingsGoalResponse>();
            return OperationResult.Success(emptyList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting savings goals for user {UserId}", userId);
            return OperationResult.Failure<IEnumerable<SavingsGoalResponse>>("An error occurred while retrieving savings goals");
        }
    }

    public async Task<OperationResult<SavingsGoalResponse>> UpdateSavingsGoalAsync(Guid userId, Guid goalId, UpdateSavingsGoalRequest request)
    {
        try
        {
            _logger.LogInformation("Updating savings goal {GoalId} for user {UserId}", goalId, userId);
            
            return OperationResult.Failure<SavingsGoalResponse>("Savings goal functionality not yet available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating savings goal {GoalId} for user {UserId}", goalId, userId);
            return OperationResult.Failure<SavingsGoalResponse>("An error occurred while updating the savings goal");
        }
    }

    public async Task<OperationResult> PauseSavingsGoalAsync(Guid userId, Guid goalId)
    {
        try
        {
            _logger.LogInformation("Pausing savings goal {GoalId} for user {UserId}", goalId, userId);
            
            return OperationResult.Failure("Savings goal functionality not yet available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing savings goal {GoalId} for user {UserId}", goalId, userId);
            return OperationResult.Failure("An error occurred while pausing the savings goal");
        }
    }

    public async Task<OperationResult> ResumeSavingsGoalAsync(Guid userId, Guid goalId)
    {
        try
        {
            _logger.LogInformation("Resuming savings goal {GoalId} for user {UserId}", goalId, userId);
            
            return OperationResult.Failure("Savings goal functionality not yet available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming savings goal {GoalId} for user {UserId}", goalId, userId);
            return OperationResult.Failure("An error occurred while resuming the savings goal");
        }
    }

    public async Task<OperationResult> CancelSavingsGoalAsync(Guid userId, Guid goalId)
    {
        try
        {
            _logger.LogInformation("Canceling savings goal {GoalId} for user {UserId}", goalId, userId);
            
            return OperationResult.Failure("Savings goal functionality not yet available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling savings goal {GoalId} for user {UserId}", goalId, userId);
            return OperationResult.Failure("An error occurred while canceling the savings goal");
        }
    }

    public async Task<OperationResult<ContributionResponse>> AddContributionAsync(Guid userId, Guid goalId, AddContributionRequest request)
    {
        try
        {
            _logger.LogInformation("Adding contribution to savings goal {GoalId} for user {UserId}", goalId, userId);
            
            return OperationResult.Failure<ContributionResponse>("Savings goal functionality not yet available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding contribution to savings goal {GoalId} for user {UserId}", goalId, userId);
            return OperationResult.Failure<ContributionResponse>("An error occurred while adding the contribution");
        }
    }

    public async Task<OperationResult<IEnumerable<ContributionResponse>>> GetContributionsAsync(Guid userId, Guid goalId)
    {
        try
        {
            _logger.LogInformation("Getting contributions for savings goal {GoalId}, user {UserId}", goalId, userId);
            
            var emptyList = Enumerable.Empty<ContributionResponse>();
            return OperationResult.Success(emptyList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contributions for savings goal {GoalId}, user {UserId}", goalId, userId);
            return OperationResult.Failure<IEnumerable<ContributionResponse>>("An error occurred while retrieving contributions");
        }
    }

    public async Task<OperationResult> WithdrawFromGoalAsync(Guid userId, Guid goalId, decimal amount)
    {
        try
        {
            _logger.LogInformation("Withdrawing {Amount} from savings goal {GoalId} for user {UserId}", amount, goalId, userId);
            
            return OperationResult.Failure("Savings goal functionality not yet available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error withdrawing from savings goal {GoalId} for user {UserId}", goalId, userId);
            return OperationResult.Failure("An error occurred while processing the withdrawal");
        }
    }

    // Helper methods
    private static BudgetResponse MapToResponse(Budget budget)
    {
        return new BudgetResponse
        {
            Id = budget.Id,
            Name = budget.Name,
            Description = budget.Description,
            LimitAmount = budget.Limit.Amount,
            SpentAmount = budget.Spent.Amount,
            RemainingAmount = budget.RemainingAmount,
            Currency = budget.Limit.Currency,
            Period = budget.Period.ToString(),
            StartDate = budget.DateRange.StartDate,
            EndDate = budget.DateRange.EndDate,
            PercentageUsed = budget.PercentageUsed,
            IsOverBudget = budget.IsOverBudget,
            IsNearLimit = budget.IsNearLimit(),
            IsActive = budget.IsActive,
            AlertsEnabled = budget.AlertsEnabled,
            CategoryIds = budget.CategoryIds.ToList(),
            CreatedAt = budget.CreatedAt
        };
    }
}