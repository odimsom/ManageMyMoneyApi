using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Domain.Entities.Expenses;

public class ExpenseTag
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Color { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ExpenseTag() { }

    public static OperationResult<ExpenseTag> Create(string name, Guid userId, string? color = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return OperationResult.Failure<ExpenseTag>("Tag name is required");

        if (name.Length > 30)
            return OperationResult.Failure<ExpenseTag>("Tag name cannot exceed 30 characters");

        var tag = new ExpenseTag
        {
            Id = Guid.NewGuid(),
            Name = name.Trim().ToLowerInvariant(),
            Color = color,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        return OperationResult.Success(tag);
    }

    public OperationResult UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return OperationResult.Failure("Tag name is required");

        if (newName.Length > 30)
            return OperationResult.Failure("Tag name cannot exceed 30 characters");

        Name = newName.Trim().ToLowerInvariant();
        return OperationResult.Success();
    }
}
