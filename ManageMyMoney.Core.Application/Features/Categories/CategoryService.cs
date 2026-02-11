using ManageMyMoney.Core.Application.DTOs.Categories;
using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Categories;
using ManageMyMoney.Core.Domain.Enums;
using ManageMyMoney.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ManageMyMoney.Core.Application.Features.Categories;

public class CategoryService : ICategoryService
{
    private readonly ILogger<CategoryService> _logger;
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ILogger<CategoryService> logger, ICategoryRepository categoryRepository)
    {
        _logger = logger;
        _categoryRepository = categoryRepository;
    }

    public async Task<OperationResult<CategoryResponse>> CreateCategoryAsync(Guid userId, CreateCategoryRequest request)
    {
        try
        {
            _logger.LogInformation("Creating category {CategoryName} for user {UserId}", request.Name, userId);

            // Validar que el nombre no esté duplicado
            var nameExistsResult = await _categoryRepository.NameExistsForUserAsync(request.Name, userId);
            if (!nameExistsResult.IsSuccess)
                return OperationResult.Failure<CategoryResponse>(nameExistsResult.Error);

            if (nameExistsResult.Value)
                return OperationResult.Failure<CategoryResponse>("A category with this name already exists");

            // Convertir string a enum
            if (!Enum.TryParse<CategoryType>(request.Type, true, out var categoryType))
                return OperationResult.Failure<CategoryResponse>("Invalid category type");

            if (!Enum.TryParse<TransactionType>(request.TransactionType, true, out var transactionType))
                return OperationResult.Failure<CategoryResponse>("Invalid transaction type");

            // Crear categoria
            var createResult = Category.Create(
                request.Name,
                categoryType,
                transactionType,
                userId,
                request.Description,
                request.Icon,
                request.Color
            );

            if (!createResult.IsSuccess || createResult.Value == null)
                return OperationResult.Failure<CategoryResponse>(createResult.Error);

            // Guardar en el repositorio
            var addResult = await _categoryRepository.AddAsync(createResult.Value);
            if (!addResult.IsSuccess)
                return OperationResult.Failure<CategoryResponse>(addResult.Error);

            var response = MapToResponse(createResult.Value);
            _logger.LogInformation("Category {CategoryId} created successfully for user {UserId}", createResult.Value.Id, userId);
            
            return OperationResult.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category for user {UserId}", userId);
            return OperationResult.Failure<CategoryResponse>("An error occurred while creating the category");
        }
    }

    public async Task<OperationResult<CategoryResponse>> GetCategoryByIdAsync(Guid userId, Guid categoryId)
    {
        try
        {
            _logger.LogInformation("Getting category {CategoryId} for user {UserId}", categoryId, userId);

            var result = await _categoryRepository.GetByIdAsync(categoryId);
            if (!result.IsSuccess || result.Value == null)
                return OperationResult.Failure<CategoryResponse>(result.Error ?? "Category not found");

            if (result.Value.UserId != userId)
                return OperationResult.Failure<CategoryResponse>("Category not found");

            var response = MapToResponse(result.Value);
            return OperationResult.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category {CategoryId} for user {UserId}", categoryId, userId);
            return OperationResult.Failure<CategoryResponse>("An error occurred while retrieving the category");
        }
    }

    public async Task<OperationResult<IEnumerable<CategoryResponse>>> GetCategoriesAsync(Guid userId, string? transactionType = null)
    {
        try
        {
            _logger.LogInformation("Getting categories for user {UserId}, transactionType: {TransactionType}", userId, transactionType);

            OperationResult<IEnumerable<Category>> result;

            if (!string.IsNullOrEmpty(transactionType))
            {
                if (!Enum.TryParse<TransactionType>(transactionType, true, out var type))
                    return OperationResult.Failure<IEnumerable<CategoryResponse>>("Invalid transaction type");

                result = await _categoryRepository.GetByTransactionTypeAsync(userId, type);
            }
            else
            {
                result = await _categoryRepository.GetAllByUserAsync(userId);
            }

            if (!result.IsSuccess || result.Value == null)
                return OperationResult.Failure<IEnumerable<CategoryResponse>>(result.Error ?? "Categories not found");

            var responses = result.Value.Select(MapToResponse);
            return OperationResult.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories for user {UserId}", userId);
            return OperationResult.Failure<IEnumerable<CategoryResponse>>("An error occurred while retrieving categories");
        }
    }

    public async Task<OperationResult<IEnumerable<CategoryResponse>>> GetExpenseCategoriesAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Getting expense categories for user {UserId}", userId);

            var result = await _categoryRepository.GetByTransactionTypeAsync(userId, TransactionType.Expense);
            if (!result.IsSuccess || result.Value == null)
                return OperationResult.Failure<IEnumerable<CategoryResponse>>(result.Error ?? "Expense categories not found");

            var responses = result.Value.Select(MapToResponse);
            return OperationResult.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expense categories for user {UserId}", userId);
            return OperationResult.Failure<IEnumerable<CategoryResponse>>("An error occurred while retrieving expense categories");
        }
    }

    public async Task<OperationResult<IEnumerable<CategoryResponse>>> GetIncomeCategoriesAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Getting income categories for user {UserId}", userId);

            var result = await _categoryRepository.GetByTransactionTypeAsync(userId, TransactionType.Income);
            if (!result.IsSuccess || result.Value == null)
                return OperationResult.Failure<IEnumerable<CategoryResponse>>(result.Error ?? "Income categories not found");

            var responses = result.Value.Select(MapToResponse);
            return OperationResult.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting income categories for user {UserId}", userId);
            return OperationResult.Failure<IEnumerable<CategoryResponse>>("An error occurred while retrieving income categories");
        }
    }

    public async Task<OperationResult<CategoryResponse>> UpdateCategoryAsync(Guid userId, Guid categoryId, UpdateCategoryRequest request)
    {
        try
        {
            _logger.LogInformation("Updating category {CategoryId} for user {UserId}", categoryId, userId);

            // Obtener categoria existente
            var getCategoryResult = await _categoryRepository.GetByIdAsync(categoryId);
            if (!getCategoryResult.IsSuccess || getCategoryResult.Value == null)
                return OperationResult.Failure<CategoryResponse>(getCategoryResult.Error ?? "Category not found");

            var category = getCategoryResult.Value;
            if (category.UserId != userId)
                return OperationResult.Failure<CategoryResponse>("Category not found");

            // Validar nombre si se está cambiando
            if (!string.IsNullOrEmpty(request.Name) && request.Name != category.Name)
            {
                var nameExistsResult = await _categoryRepository.NameExistsForUserAsync(request.Name, userId);
                if (!nameExistsResult.IsSuccess)
                    return OperationResult.Failure<CategoryResponse>(nameExistsResult.Error);

                if (nameExistsResult.Value)
                    return OperationResult.Failure<CategoryResponse>("A category with this name already exists");

                var updateNameResult = category.UpdateName(request.Name);
                if (!updateNameResult.IsSuccess)
                    return OperationResult.Failure<CategoryResponse>(updateNameResult.Error);
            }

            // Actualizar apariencia
            if (request.Icon != null || request.Color != null)
            {
                category.UpdateAppearance(request.Icon, request.Color);
            }

            // Guardar cambios
            var updateResult = await _categoryRepository.UpdateAsync(category);
            if (!updateResult.IsSuccess)
                return OperationResult.Failure<CategoryResponse>(updateResult.Error);

            var response = MapToResponse(category);
            _logger.LogInformation("Category {CategoryId} updated successfully", categoryId);
            
            return OperationResult.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId} for user {UserId}", categoryId, userId);
            return OperationResult.Failure<CategoryResponse>("An error occurred while updating the category");
        }
    }

    public async Task<OperationResult> DeleteCategoryAsync(Guid userId, Guid categoryId)
    {
        try
        {
            _logger.LogInformation("Deleting category {CategoryId} for user {UserId}", categoryId, userId);

            // Obtener categoria existente
            var getCategoryResult = await _categoryRepository.GetByIdAsync(categoryId);
            if (!getCategoryResult.IsSuccess || getCategoryResult.Value == null)
                return OperationResult.Failure(getCategoryResult.Error ?? "Category not found");

            var category = getCategoryResult.Value;
            if (category.UserId != userId)
                return OperationResult.Failure("Category not found");

            // No permitir eliminar categorias por defecto
            if (category.IsDefault)
                return OperationResult.Failure("Cannot delete default categories");

            // Desactivar categoria (soft delete)
            category.Deactivate();
            var updateResult = await _categoryRepository.UpdateAsync(category);
            if (!updateResult.IsSuccess)
                return OperationResult.Failure(updateResult.Error);

            _logger.LogInformation("Category {CategoryId} deleted successfully", categoryId);
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId} for user {UserId}", categoryId, userId);
            return OperationResult.Failure("An error occurred while deleting the category");
        }
    }

    public async Task<OperationResult<SubcategoryResponse>> CreateSubcategoryAsync(Guid userId, CreateSubcategoryRequest request)
    {
        try
        {
            _logger.LogInformation("Creating subcategory {SubcategoryName} for category {CategoryId}, user {UserId}", request.Name, request.CategoryId, userId);

            // Verificar que la categoria existe y pertenece al usuario
            var getCategoryResult = await _categoryRepository.GetByIdAsync(request.CategoryId);
            if (!getCategoryResult.IsSuccess || getCategoryResult.Value == null)
                return OperationResult.Failure<SubcategoryResponse>("Category not found");

            var category = getCategoryResult.Value;
            if (category.UserId != userId)
                return OperationResult.Failure<SubcategoryResponse>("Category not found");

            // Crear subcategoria
            var createResult = Subcategory.Create(request.Name, request.CategoryId, request.Icon);
            if (!createResult.IsSuccess)
                return OperationResult.Failure<SubcategoryResponse>(createResult.Error);

            // Agregar a la categoria
            if (createResult.Value == null)
                return OperationResult.Failure<SubcategoryResponse>("Failed to create subcategory");

            var addResult = category.AddSubcategory(createResult.Value);
            if (!addResult.IsSuccess)
                return OperationResult.Failure<SubcategoryResponse>(addResult.Error);

            // Actualizar categoria en el repositorio
            var updateResult = await _categoryRepository.UpdateAsync(category);
            if (!updateResult.IsSuccess)
                return OperationResult.Failure<SubcategoryResponse>(updateResult.Error);

            var response = MapToSubcategoryResponse(createResult.Value);
            _logger.LogInformation("Subcategory {SubcategoryId} created successfully", createResult.Value.Id);
            
            return OperationResult.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subcategory for user {UserId}", userId);
            return OperationResult.Failure<SubcategoryResponse>("An error occurred while creating the subcategory");
        }
    }

    public async Task<OperationResult<IEnumerable<SubcategoryResponse>>> GetSubcategoriesAsync(Guid userId, Guid categoryId)
    {
        try
        {
            _logger.LogInformation("Getting subcategories for category {CategoryId}, user {UserId}", categoryId, userId);

            // Verificar que la categoria existe y pertenece al usuario
            var getCategoryResult = await _categoryRepository.GetByIdAsync(categoryId);
            if (!getCategoryResult.IsSuccess || getCategoryResult.Value == null)
                return OperationResult.Failure<IEnumerable<SubcategoryResponse>>("Category not found");

            var category = getCategoryResult.Value;
            if (category.UserId != userId)
                return OperationResult.Failure<IEnumerable<SubcategoryResponse>>("Category not found");

            var responses = category.Subcategories
                .Where(s => s.IsActive)
                .Select(MapToSubcategoryResponse);
            
            return OperationResult.Success(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subcategories for category {CategoryId}, user {UserId}", categoryId, userId);
            return OperationResult.Failure<IEnumerable<SubcategoryResponse>>("An error occurred while retrieving subcategories");
        }
    }

    public async Task<OperationResult> DeleteSubcategoryAsync(Guid userId, Guid subcategoryId)
    {
        try
        {
            _logger.LogInformation("Deleting subcategory {SubcategoryId} for user {UserId}", subcategoryId, userId);

            // Obtener todas las categorias del usuario para encontrar la subcategoria
            var getCategoriesResult = await _categoryRepository.GetAllByUserAsync(userId);
            if (!getCategoriesResult.IsSuccess || getCategoriesResult.Value == null)
                return OperationResult.Failure(getCategoriesResult.Error ?? "Categories not found");

            var category = getCategoriesResult.Value
                .FirstOrDefault(c => c.Subcategories.Any(s => s.Id == subcategoryId));

            if (category == null)
                return OperationResult.Failure("Subcategory not found");

            var subcategory = category.Subcategories.FirstOrDefault(s => s.Id == subcategoryId);
            if (subcategory == null)
                return OperationResult.Failure("Subcategory not found");

            // Desactivar subcategoria (soft delete)
            subcategory.Deactivate();
            
            // Actualizar categoria en el repositorio
            var updateResult = await _categoryRepository.UpdateAsync(category);
            if (!updateResult.IsSuccess)
                return OperationResult.Failure(updateResult.Error);

            _logger.LogInformation("Subcategory {SubcategoryId} deleted successfully", subcategoryId);
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting subcategory {SubcategoryId} for user {UserId}", subcategoryId, userId);
            return OperationResult.Failure("An error occurred while deleting the subcategory");
        }
    }

    public Task<OperationResult<IEnumerable<CategoryBudgetResponse>>> GetCategoryBudgetsAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Getting category budgets for user {UserId}", userId);
            
            // TODO: Implementar cuando se implemente el repositorio de CategoryBudget
            // Por ahora retornamos una lista vacia
            var emptyList = Enumerable.Empty<CategoryBudgetResponse>();
            return Task.FromResult(OperationResult.Success(emptyList));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category budgets for user {UserId}", userId);
            return Task.FromResult(OperationResult.Failure<IEnumerable<CategoryBudgetResponse>>("An error occurred while retrieving category budgets"));
        }
    }

    public Task<OperationResult<CategoryBudgetResponse>> GetCategoryBudgetAsync(Guid userId, Guid categoryId)
    {
        try
        {
            _logger.LogInformation("Getting category budget for category {CategoryId}, user {UserId}", categoryId, userId);
            
            // TODO: Implementar cuando se implemente el repositorio de CategoryBudget
            return Task.FromResult(OperationResult.Failure<CategoryBudgetResponse>("Category budget functionality not yet available"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category budget for category {CategoryId}, user {UserId}", categoryId, userId);
            return Task.FromResult(OperationResult.Failure<CategoryBudgetResponse>("An error occurred while retrieving the category budget"));
        }
    }

    public async Task<OperationResult> InitializeDefaultCategoriesAsync(Guid userId)
    {
        try
        {
            _logger.LogInformation("Initializing default categories for user {UserId}", userId);

            // Obtener categorias por defecto
            var defaultCategoriesResult = await _categoryRepository.GetDefaultCategoriesAsync();
            if (!defaultCategoriesResult.IsSuccess || defaultCategoriesResult.Value == null)
                return OperationResult.Failure(defaultCategoriesResult.Error ?? "Default categories not found");

            // Crear categorias por defecto para el usuario
            foreach (var defaultCategory in defaultCategoriesResult.Value)
            {
                var createResult = Category.Create(
                    defaultCategory.Name,
                    defaultCategory.Type,
                    defaultCategory.TransactionType,
                    userId,
                    defaultCategory.Description,
                    defaultCategory.Icon,
                    defaultCategory.Color,
                    true // isDefault
                );

                if (!createResult.IsSuccess || createResult.Value == null)
                {
                    _logger.LogWarning("Failed to create default category {CategoryName}: {Error}", 
                        defaultCategory.Name, createResult.Error);
                    continue;
                }

                var addResult = await _categoryRepository.AddAsync(createResult.Value);
                if (!addResult.IsSuccess)
                {
                    _logger.LogWarning("Failed to save default category {CategoryName}: {Error}", 
                        defaultCategory.Name, addResult.Error);
                }
            }

            _logger.LogInformation("Default categories initialized for user {UserId}", userId);
            return OperationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing default categories for user {UserId}", userId);
            return OperationResult.Failure("An error occurred while initializing default categories");
        }
    }

    // Helper methods for mapping entities to DTOs
    private static CategoryResponse MapToResponse(Category category)
    {
        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Icon = category.Icon,
            Color = category.Color,
            Type = category.Type.ToString(),
            TransactionType = category.TransactionType.ToString(),
            IsDefault = category.IsDefault,
            IsActive = category.IsActive,
            Subcategories = category.Subcategories
                .Where(s => s.IsActive)
                .Select(MapToSubcategoryResponse)
                .ToList()
        };
    }

    private static SubcategoryResponse MapToSubcategoryResponse(Subcategory subcategory)
    {
        return new SubcategoryResponse
        {
            Id = subcategory.Id,
            Name = subcategory.Name,
            Icon = subcategory.Icon,
            CategoryId = subcategory.CategoryId,
            IsActive = subcategory.IsActive
        };
    }
}
