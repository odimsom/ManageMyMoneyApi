using ManageMyMoney.Core.Application.DTOs.Budgets;
using ManageMyMoney.Core.Application.Features.Budgets;
using ManageMyMoney.Presentation.Api.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManageMyMoney.Presentation.Api.Controllers;

[Authorize]
public class BudgetsController : BaseApiController
{
    private readonly IBudgetService _budgetService;

    public BudgetsController(IBudgetService budgetService)
    {
        _budgetService = budgetService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<BudgetResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetRequest request)
    {
        var result = await _budgetService.CreateBudgetAsync(CurrentUserId, request);
        return HandleCreated(result, nameof(GetBudgetById), new { id = result.Value?.Id });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<BudgetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBudgetById(Guid id)
    {
        var result = await _budgetService.GetBudgetByIdAsync(CurrentUserId, id);
        return HandleNotFound(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<BudgetResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBudgets([FromQuery] bool activeOnly = true)
    {
        var result = await _budgetService.GetBudgetsAsync(CurrentUserId, activeOnly);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<BudgetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateBudget(Guid id, [FromBody] UpdateBudgetRequest request)
    {
        var result = await _budgetService.UpdateBudgetAsync(CurrentUserId, id, request);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeactivateBudget(Guid id)
    {
        var result = await _budgetService.DeactivateBudgetAsync(CurrentUserId, id);
        return HandleResult(result, "Budget deactivated successfully");
    }

    [HttpGet("{id:guid}/progress")]
    [ProducesResponseType(typeof(ApiResponse<BudgetProgressResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBudgetProgress(Guid id)
    {
        var result = await _budgetService.GetBudgetProgressAsync(CurrentUserId, id);
        return HandleResult(result);
    }

    [HttpGet("progress")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<BudgetProgressResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllBudgetsProgress()
    {
        var result = await _budgetService.GetAllBudgetsProgressAsync(CurrentUserId);
        return HandleResult(result);
    }

    [HttpPost("check-alerts")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckBudgetAlerts()
    {
        var result = await _budgetService.CheckBudgetAlertsAsync(CurrentUserId);
        return HandleResult(result, "Budget alerts checked");
    }

    // Savings Goals
    [HttpPost("goals")]
    [ProducesResponseType(typeof(ApiResponse<SavingsGoalResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateSavingsGoal([FromBody] CreateSavingsGoalRequest request)
    {
        var result = await _budgetService.CreateSavingsGoalAsync(CurrentUserId, request);
        return HandleCreated(result, nameof(GetSavingsGoalById), new { goalId = result.Value?.Id });
    }

    [HttpGet("goals/{goalId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SavingsGoalResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSavingsGoalById(Guid goalId)
    {
        var result = await _budgetService.GetSavingsGoalByIdAsync(CurrentUserId, goalId);
        return HandleNotFound(result);
    }

    [HttpGet("goals")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SavingsGoalResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSavingsGoals([FromQuery] string? status = null)
    {
        var result = await _budgetService.GetSavingsGoalsAsync(CurrentUserId, status);
        return HandleResult(result);
    }

    [HttpPut("goals/{goalId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SavingsGoalResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateSavingsGoal(Guid goalId, [FromBody] UpdateSavingsGoalRequest request)
    {
        var result = await _budgetService.UpdateSavingsGoalAsync(CurrentUserId, goalId, request);
        return HandleResult(result);
    }

    [HttpPost("goals/{goalId:guid}/pause")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> PauseSavingsGoal(Guid goalId)
    {
        var result = await _budgetService.PauseSavingsGoalAsync(CurrentUserId, goalId);
        return HandleResult(result, "Goal paused");
    }

    [HttpPost("goals/{goalId:guid}/resume")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResumeSavingsGoal(Guid goalId)
    {
        var result = await _budgetService.ResumeSavingsGoalAsync(CurrentUserId, goalId);
        return HandleResult(result, "Goal resumed");
    }

    [HttpPost("goals/{goalId:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelSavingsGoal(Guid goalId)
    {
        var result = await _budgetService.CancelSavingsGoalAsync(CurrentUserId, goalId);
        return HandleResult(result, "Goal cancelled");
    }

    [HttpPost("goals/{goalId:guid}/contributions")]
    [ProducesResponseType(typeof(ApiResponse<ContributionResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddContribution(Guid goalId, [FromBody] AddContributionRequest request)
    {
        var result = await _budgetService.AddContributionAsync(CurrentUserId, goalId, request);
        return HandleResult(result);
    }

    [HttpGet("goals/{goalId:guid}/contributions")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ContributionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContributions(Guid goalId)
    {
        var result = await _budgetService.GetContributionsAsync(CurrentUserId, goalId);
        return HandleResult(result);
    }

    [HttpPost("goals/{goalId:guid}/withdraw")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> WithdrawFromGoal(Guid goalId, [FromQuery] decimal amount)
    {
        var result = await _budgetService.WithdrawFromGoalAsync(CurrentUserId, goalId, amount);
        return HandleResult(result, "Withdrawal successful");
    }
}
