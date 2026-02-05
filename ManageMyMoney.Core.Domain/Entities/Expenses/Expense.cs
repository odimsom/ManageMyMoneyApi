using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.ValueObjects;

namespace ManageMyMoney.Core.Domain.Entities.Expenses;

public class Expense
{
    public Guid Id { get; private set; }
    public Money Amount { get; private set; } = Money.Zero();
    public string Description { get; private set; } = string.Empty;
    public DateTime Date { get; private set; }
    public Guid CategoryId { get; private set; }
    public Guid? SubcategoryId { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid? PaymentMethodId { get; private set; }
    public Guid UserId { get; private set; }
    public string? Notes { get; private set; }
    public string? Location { get; private set; }
    public bool IsRecurring { get; private set; }
    public Guid? RecurringExpenseId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<ExpenseTag> _tags = [];
    public IReadOnlyCollection<ExpenseTag> Tags => _tags.AsReadOnly();

    private readonly List<ExpenseAttachment> _attachments = [];
    public IReadOnlyCollection<ExpenseAttachment> Attachments => _attachments.AsReadOnly();

    private Expense() { }

    public static OperationResult<Expense> Create(
        decimal amount,
        string currency,
        string description,
        DateTime date,
        Guid categoryId,
        Guid accountId,
        Guid userId,
        Guid? subcategoryId = null,
        Guid? paymentMethodId = null,
        string? notes = null,
        string? location = null)
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
            SubcategoryId = subcategoryId,
            AccountId = accountId,
            PaymentMethodId = paymentMethodId,
            UserId = userId,
            Notes = notes?.Trim(),
            Location = location?.Trim(),
            IsRecurring = false,
            CreatedAt = DateTime.UtcNow
        };

        return OperationResult.Success(expense);
    }

    public OperationResult UpdateAmount(decimal newAmount, bool isFixedCategory)
    {
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

    public OperationResult UpdateCategory(Guid categoryId, Guid? subcategoryId = null)
    {
        if (categoryId == Guid.Empty)
            return OperationResult.Failure("Invalid category");

        CategoryId = categoryId;
        SubcategoryId = subcategoryId;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public void AddTag(ExpenseTag tag)
    {
        if (!_tags.Any(t => t.Id == tag.Id))
        {
            _tags.Add(tag);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RemoveTag(Guid tagId)
    {
        var tag = _tags.FirstOrDefault(t => t.Id == tagId);
        if (tag is not null)
        {
            _tags.Remove(tag);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void AddAttachment(ExpenseAttachment attachment)
    {
        _attachments.Add(attachment);
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsRecurring(Guid recurringExpenseId)
    {
        IsRecurring = true;
        RecurringExpenseId = recurringExpenseId;
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
