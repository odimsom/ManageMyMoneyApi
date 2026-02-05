using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.ValueObjects;

namespace ManageMyMoney.Core.Domain.Entities;

public class Expense
{
    public Guid Id { get; private set; }
    public Money Amount { get; private set; } = Money.Zero();
    public string Description { get; private set; } = string.Empty;
    public DateTime Date { get; private set; }
    public Guid CategoryId { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid UserId { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Expense() { }

    public static OperationResult<Expense> Create(
        decimal amount,
        string currency,
        string description,
        DateTime date,
        Guid categoryId,
        Guid accountId,
        Guid userId,
        string? notes = null)
    {
        var amountResult = Money.Create(amount, currency);
        if (amountResult.IsFailure)
            return OperationResult.Failure<Expense>(amountResult.Error);

        if (amountResult.Value!.Amount <= 0)
            return OperationResult.Failure<Expense>("Expense amount must be greater than zero");

        var validationResult = Validate(description, date);
        if (validationResult.IsFailure)
            return OperationResult.Failure<Expense>(validationResult.Error);

        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            Amount = amountResult.Value!,
            Description = description.Trim(),
            Date = date,
            CategoryId = categoryId,
            AccountId = accountId,
            UserId = userId,
            Notes = notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        return OperationResult.Success(expense);
    }

    public OperationResult UpdateAmount(decimal newAmount, bool isFixedCategory)
    {
        // Fixed expenses cannot be modified after 24 hours to maintain report integrity
        if (isFixedCategory && DateTime.UtcNow - CreatedAt > TimeSpan.FromHours(24))
            return OperationResult.Failure("Fixed expenses cannot be modified after 24 hours");

        var amountResult = Money.Create(newAmount, Amount.Currency);
        if (amountResult.IsFailure)
            return OperationResult.Failure(amountResult.Error);

        if (amountResult.Value!.Amount <= 0)
            return OperationResult.Failure("Expense amount must be greater than zero");

        Amount = amountResult.Value!;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public OperationResult UpdateDescription(string newDescription)
    {
        if (string.IsNullOrWhiteSpace(newDescription))
            return OperationResult.Failure("Description is required");

        if (newDescription.Length > 500)
            return OperationResult.Failure("Description cannot exceed 500 characters");

        Description = newDescription.Trim();
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public OperationResult UpdateCategory(Guid newCategoryId)
    {
        if (newCategoryId == Guid.Empty)
            return OperationResult.Failure("Invalid category");

        CategoryId = newCategoryId;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    private static OperationResult Validate(string description, DateTime date)
    {
        if (string.IsNullOrWhiteSpace(description))
            return OperationResult.Failure("Description is required");

        if (description.Length > 500)
            return OperationResult.Failure("Description cannot exceed 500 characters");

        if (date > DateTime.UtcNow)
            return OperationResult.Failure("Expense date cannot be in the future");

        return OperationResult.Success();
    }
}
