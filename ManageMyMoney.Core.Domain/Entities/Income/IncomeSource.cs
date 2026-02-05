using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Domain.Entities.Income;

public class IncomeSource
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Icon { get; private set; }
    public string? Color { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private IncomeSource() { }

    public static OperationResult<IncomeSource> Create(
        string name,
        Guid userId,
        string? description = null,
        string? icon = null,
        string? color = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return OperationResult.Failure<IncomeSource>("Income source name is required");

        if (name.Length > 100)
            return OperationResult.Failure<IncomeSource>("Name cannot exceed 100 characters");

        var source = new IncomeSource
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Icon = icon,
            Color = color,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return OperationResult.Success(source);
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
