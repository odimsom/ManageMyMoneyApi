using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Domain.Entities.Categories;

public class Subcategory
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Icon { get; private set; }
    public Guid CategoryId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Subcategory() { }

    public static OperationResult<Subcategory> Create(
        string name,
        Guid categoryId,
        string? icon = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return OperationResult.Failure<Subcategory>("Subcategory name is required");

        if (name.Length > 50)
            return OperationResult.Failure<Subcategory>("Subcategory name cannot exceed 50 characters");

        var subcategory = new Subcategory
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            CategoryId = categoryId,
            Icon = icon,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return OperationResult.Success(subcategory);
    }

    public OperationResult UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return OperationResult.Failure("Name is required");

        Name = newName.Trim();
        return OperationResult.Success();
    }

    public void Deactivate() => IsActive = false;
}
