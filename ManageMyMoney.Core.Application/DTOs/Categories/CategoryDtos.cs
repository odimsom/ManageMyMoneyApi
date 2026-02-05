namespace ManageMyMoney.Core.Application.DTOs.Categories;

public record CreateCategoryRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? Icon { get; init; }
    public string? Color { get; init; }
    public required string Type { get; init; }
    public required string TransactionType { get; init; }
}

public record UpdateCategoryRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Icon { get; init; }
    public string? Color { get; init; }
}

public record CategoryResponse
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? Icon { get; init; }
    public string? Color { get; init; }
    public required string Type { get; init; }
    public required string TransactionType { get; init; }
    public bool IsDefault { get; init; }
    public bool IsActive { get; init; }
    public List<SubcategoryResponse>? Subcategories { get; init; }
}

public record CreateSubcategoryRequest
{
    public required string Name { get; init; }
    public string? Icon { get; init; }
    public Guid CategoryId { get; init; }
}

public record SubcategoryResponse
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Icon { get; init; }
    public Guid CategoryId { get; init; }
    public bool IsActive { get; init; }
}

public record CategoryBudgetResponse
{
    public Guid Id { get; init; }
    public Guid CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public decimal LimitAmount { get; init; }
    public decimal SpentAmount { get; init; }
    public required string Currency { get; init; }
    public decimal RemainingAmount { get; init; }
    public decimal PercentageUsed { get; init; }
    public bool IsOverBudget { get; init; }
}
