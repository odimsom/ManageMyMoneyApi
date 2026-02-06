using ManageMyMoney.Core.Application.DTOs.Categories;
using ManageMyMoney.Core.Domain.Common;
using Microsoft.Extensions.Logging;

namespace ManageMyMoney.Core.Application.Features.Categories;

public class CategoryService : ICategoryService
{
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ILogger<CategoryService> logger)
    {
        _logger = logger;
    }

    public Task<OperationResult<CategoryResponse>> CreateCategoryAsync(Guid userId, CreateCategoryRequest request)
    {
        _logger.LogWarning("CategoryService.CreateCategoryAsync not implemented yet");
        return Task.FromResult(OperationResult.Failure<CategoryResponse>("Not implemented yet"));
    }

    public Task<OperationResult<CategoryResponse>> GetCategoryByIdAsync(Guid userId, Guid categoryId)
    {
        _logger.LogWarning("CategoryService.GetCategoryByIdAsync not implemented yet");
        return Task.FromResult(OperationResult.Failure<CategoryResponse>("Not implemented yet"));
    }

    public Task<OperationResult<IEnumerable<CategoryResponse>>> GetCategoriesAsync(Guid userId, string? transactionType = null)
    {
        _logger.LogWarning("CategoryService.GetCategoriesAsync not implemented yet");
        return Task.FromResult(OperationResult.Failure<IEnumerable<CategoryResponse>>("Not implemented yet"));
    }

    public Task<OperationResult<IEnumerable<CategoryResponse>>> GetExpenseCategoriesAsync(Guid userId)
    {
        _logger.LogWarning("CategoryService.GetExpenseCategoriesAsync not implemented yet");
        return Task.FromResult(OperationResult.Failure<IEnumerable<CategoryResponse>>("Not implemented yet"));
    }

    public Task<OperationResult<IEnumerable<CategoryResponse>>> GetIncomeCategoriesAsync(Guid userId)
    {
        _logger.LogWarning("CategoryService.GetIncomeCategoriesAsync not implemented yet");
        return Task.FromResult(OperationResult.Failure<IEnumerable<CategoryResponse>>("Not implemented yet"));
    }

    public Task<OperationResult<CategoryResponse>> UpdateCategoryAsync(Guid userId, Guid categoryId, UpdateCategoryRequest request)
    {
        _logger.LogWarning("CategoryService.UpdateCategoryAsync not implemented yet");
        return Task.FromResult(OperationResult.Failure<CategoryResponse>("Not implemented yet"));
    }

    public Task<OperationResult> DeleteCategoryAsync(Guid userId, Guid categoryId)
    {
        _logger.LogWarning("CategoryService.DeleteCategoryAsync not implemented yet");
        return Task.FromResult(OperationResult.Failure("Not implemented yet"));
    }

    public Task<OperationResult<SubcategoryResponse>> CreateSubcategoryAsync(Guid userId, CreateSubcategoryRequest request)
    {
        _logger.LogWarning("CategoryService.CreateSubcategoryAsync not implemented yet");
        return Task.FromResult(OperationResult.Failure<SubcategoryResponse>("Not implemented yet"));
    }

    public Task<OperationResult<IEnumerable<SubcategoryResponse>>> GetSubcategoriesAsync(Guid userId, Guid categoryId)
    {
        _logger.LogWarning("CategoryService.GetSubcategoriesAsync not implemented yet");
        return Task.FromResult(OperationResult.Failure<IEnumerable<SubcategoryResponse>>("Not implemented yet"));
    }

    public Task<OperationResult> DeleteSubcategoryAsync(Guid userId, Guid subcategoryId)
    {
        _logger.LogWarning("CategoryService.DeleteSubcategoryAsync not implemented yet");
        return Task.FromResult(OperationResult.Failure("Not implemented yet"));
    }

    public Task<OperationResult<IEnumerable<CategoryBudgetResponse>>> GetCategoryBudgetsAsync(Guid userId)
    {
        _logger.LogWarning("CategoryService.GetCategoryBudgetsAsync not implemented yet");
        return Task.FromResult(OperationResult.Failure<IEnumerable<CategoryBudgetResponse>>("Not implemented yet"));
    }

    public Task<OperationResult<CategoryBudgetResponse>> GetCategoryBudgetAsync(Guid userId, Guid categoryId)
    {
        _logger.LogWarning("CategoryService.GetCategoryBudgetAsync not implemented yet");
        return Task.FromResult(OperationResult.Failure<CategoryBudgetResponse>("Not implemented yet"));
    }

    public Task<OperationResult> InitializeDefaultCategoriesAsync(Guid userId)
    {
        _logger.LogWarning("CategoryService.InitializeDefaultCategoriesAsync not implemented yet");
        return Task.FromResult(OperationResult.Failure("Not implemented yet"));
    }
}