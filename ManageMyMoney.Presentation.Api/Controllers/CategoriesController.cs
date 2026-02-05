using ManageMyMoney.Core.Application.DTOs.Categories;
using ManageMyMoney.Core.Application.Features.Categories;
using ManageMyMoney.Presentation.Api.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ManageMyMoney.Presentation.Api.Controllers;

[Authorize]
public class CategoriesController : BaseApiController
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var result = await _categoryService.CreateCategoryAsync(CurrentUserId, request);
        return HandleCreated(result, nameof(GetCategoryById), new { id = result.Value?.Id });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryById(Guid id)
    {
        var result = await _categoryService.GetCategoryByIdAsync(CurrentUserId, id);
        return HandleNotFound(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories([FromQuery] string? transactionType = null)
    {
        var result = await _categoryService.GetCategoriesAsync(CurrentUserId, transactionType);
        return HandleResult(result);
    }

    [HttpGet("expenses")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpenseCategories()
    {
        var result = await _categoryService.GetExpenseCategoriesAsync(CurrentUserId);
        return HandleResult(result);
    }

    [HttpGet("income")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIncomeCategories()
    {
        var result = await _categoryService.GetIncomeCategoriesAsync(CurrentUserId);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var result = await _categoryService.UpdateCategoryAsync(CurrentUserId, id, request);
        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var result = await _categoryService.DeleteCategoryAsync(CurrentUserId, id);
        return HandleResult(result, "Category deleted successfully");
    }

    [HttpPost("subcategories")]
    [ProducesResponseType(typeof(ApiResponse<SubcategoryResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateSubcategory([FromBody] CreateSubcategoryRequest request)
    {
        var result = await _categoryService.CreateSubcategoryAsync(CurrentUserId, request);
        return HandleResult(result);
    }

    [HttpGet("{categoryId:guid}/subcategories")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SubcategoryResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubcategories(Guid categoryId)
    {
        var result = await _categoryService.GetSubcategoriesAsync(CurrentUserId, categoryId);
        return HandleResult(result);
    }

    [HttpDelete("subcategories/{subcategoryId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteSubcategory(Guid subcategoryId)
    {
        var result = await _categoryService.DeleteSubcategoryAsync(CurrentUserId, subcategoryId);
        return HandleResult(result, "Subcategory deleted successfully");
    }

    [HttpGet("budgets")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryBudgetResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategoryBudgets()
    {
        var result = await _categoryService.GetCategoryBudgetsAsync(CurrentUserId);
        return HandleResult(result);
    }

    [HttpPost("initialize-defaults")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> InitializeDefaultCategories()
    {
        var result = await _categoryService.InitializeDefaultCategoriesAsync(CurrentUserId);
        return HandleResult(result, "Default categories initialized");
    }
}
