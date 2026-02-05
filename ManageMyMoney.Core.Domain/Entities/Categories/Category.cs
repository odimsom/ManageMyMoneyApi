using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Enums;

namespace ManageMyMoney.Core.Domain.Entities.Categories;

public class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Icon { get; private set; }
    public string? Color { get; private set; }
    public CategoryType Type { get; private set; }
    public TransactionType TransactionType { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<Subcategory> _subcategories = [];
    public IReadOnlyCollection<Subcategory> Subcategories => _subcategories.AsReadOnly();

    private Category() { }

    public static OperationResult<Category> Create(
        string name,
        CategoryType type,
        TransactionType transactionType,
        Guid userId,
        string? description = null,
        string? icon = null,
        string? color = null,
        bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            return OperationResult.Failure<Category>("Category name is required");

        if (name.Length > 50)
            return OperationResult.Failure<Category>("Category name cannot exceed 50 characters");

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Type = type,
            TransactionType = transactionType,
            UserId = userId,
            Icon = icon,
            Color = color,
            IsDefault = isDefault,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return OperationResult.Success(category);
    }

    public OperationResult UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return OperationResult.Failure("Category name is required");

        if (newName.Length > 50)
            return OperationResult.Failure("Category name cannot exceed 50 characters");

        Name = newName.Trim();
        return OperationResult.Success();
    }

    public void UpdateAppearance(string? icon, string? color)
    {
        Icon = icon;
        Color = color;
    }

    public OperationResult AddSubcategory(Subcategory subcategory)
    {
        if (_subcategories.Any(s => s.Name.Equals(subcategory.Name, StringComparison.OrdinalIgnoreCase)))
            return OperationResult.Failure("Subcategory with this name already exists");

        _subcategories.Add(subcategory);
        return OperationResult.Success();
    }

    public void Deactivate() => IsActive = false;

    public bool IsFixed => Type == CategoryType.Fixed;
}
