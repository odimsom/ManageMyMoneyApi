using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Enums;

namespace ManageMyMoney.Core.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Icon { get; private set; }
    public string? Color { get; private set; }
    public CategoryType Type { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsDefault { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Category() { }

    public static OperationResult<Category> Create(
        string name,
        CategoryType type,
        Guid userId,
        string? icon = null,
        string? color = null,
        bool isDefault = false)
    {
        var validationResult = ValidateName(name);
        if (validationResult.IsFailure)
            return OperationResult.Failure<Category>(validationResult.Error);

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Type = type,
            UserId = userId,
            Icon = icon,
            Color = color,
            IsDefault = isDefault,
            CreatedAt = DateTime.UtcNow
        };

        return OperationResult.Success(category);
    }

    public OperationResult UpdateName(string newName)
    {
        var validationResult = ValidateName(newName);
        if (validationResult.IsFailure)
            return validationResult;

        Name = newName.Trim();
        return OperationResult.Success();
    }

    public void UpdateAppearance(string? icon, string? color)
    {
        Icon = icon;
        Color = color;
    }

    public bool IsFixed => Type == CategoryType.Fixed;

    private static OperationResult ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return OperationResult.Failure("Category name is required");

        if (name.Length > 50)
            return OperationResult.Failure("Category name cannot exceed 50 characters");

        return OperationResult.Success();
    }
}
