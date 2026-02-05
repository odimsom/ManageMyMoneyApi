using ManageMyMoney.Core.Application.DTOs.Categories;
using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Application.Features.Categories;

public interface ICategoryService
{
    // Categories
    Task<OperationResult<CategoryResponse>> CreateCategoryAsync(Guid userId, CreateCategoryRequest request);
    Task<OperationResult<CategoryResponse>> GetCategoryByIdAsync(Guid userId, Guid categoryId);
    Task<OperationResult<IEnumerable<CategoryResponse>>> GetCategoriesAsync(Guid userId, string? transactionType = null);
    Task<OperationResult<IEnumerable<CategoryResponse>>> GetExpenseCategoriesAsync(Guid userId);
    Task<OperationResult<IEnumerable<CategoryResponse>>> GetIncomeCategoriesAsync(Guid userId);
    Task<OperationResult<CategoryResponse>> UpdateCategoryAsync(Guid userId, Guid categoryId, UpdateCategoryRequest request);
    Task<OperationResult> DeleteCategoryAsync(Guid userId, Guid categoryId);

    // Subcategories
    Task<OperationResult<SubcategoryResponse>> CreateSubcategoryAsync(Guid userId, CreateSubcategoryRequest request);
    Task<OperationResult<IEnumerable<SubcategoryResponse>>> GetSubcategoriesAsync(Guid userId, Guid categoryId);
    Task<OperationResult> DeleteSubcategoryAsync(Guid userId, Guid subcategoryId);

    // Category Budgets
    Task<OperationResult<IEnumerable<CategoryBudgetResponse>>> GetCategoryBudgetsAsync(Guid userId);
    Task<OperationResult<CategoryBudgetResponse>> GetCategoryBudgetAsync(Guid userId, Guid categoryId);

    // Default Categories
    Task<OperationResult> InitializeDefaultCategoriesAsync(Guid userId);
}
