using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Accounts;
using ManageMyMoney.Core.Domain.Entities.Auth;
using ManageMyMoney.Core.Domain.Entities.Categories;
using ManageMyMoney.Core.Domain.ValueObjects;

namespace ManageMyMoney.Core.Domain.Entities.Expenses;

public class Expense
{
    public Guid Id { get; private set; }
    public Money Amount { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public DateTime Date { get; private set; }
    public string? Notes { get; private set; }
    public string? Location { get; private set; }
    public bool IsRecurring { get; private set; }
    public Guid? RecurringExpenseId { get; private set; }
    public Guid CategoryId { get; private set; }
    public Guid? SubcategoryId { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid? PaymentMethodId { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties
    public virtual Category Category { get; private set; } = null!;
    public virtual Subcategory? Subcategory { get; private set; }
    public virtual Account Account { get; private set; } = null!;
    public virtual PaymentMethod? PaymentMethod { get; private set; }
    public virtual User User { get; private set; } = null!;
    public virtual RecurringExpense? RecurringExpense { get; private set; }
    public virtual ICollection<ExpenseTag> Tags { get; private set; } = new List<ExpenseTag>();
    public virtual ICollection<ExpenseAttachment> Attachments { get; private set; } = new List<ExpenseAttachment>();

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
        string? location = null,
        bool isRecurring = false,
        Guid? recurringExpenseId = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            return OperationResult.Failure<Expense>("Description is required");

        if (description.Length > 500)
            return OperationResult.Failure<Expense>("Description cannot exceed 500 characters");

        var moneyResult = Money.Create(amount, currency);
        if (moneyResult.IsFailure)
            return OperationResult.Failure<Expense>(moneyResult.Error);

        if (moneyResult.Value!.Amount <= 0)
            return OperationResult.Failure<Expense>("Amount must be greater than zero");

        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            Amount = moneyResult.Value!,
            Description = description,
            Date = date.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(date, DateTimeKind.Utc) : date.ToUniversalTime(),
            CategoryId = categoryId,
            SubcategoryId = subcategoryId,
            AccountId = accountId,
            PaymentMethodId = paymentMethodId,
            UserId = userId,
            Notes = notes,
            Location = location,
            IsRecurring = isRecurring,
            RecurringExpenseId = recurringExpenseId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return OperationResult.Success(expense);
    }

    public OperationResult UpdateAmount(decimal newAmount, bool isFixed = false)
    {
        if (isFixed)
            return OperationResult.Failure("Cannot update amount of a fixed expense");

        var moneyResult = Money.Create(newAmount, Amount.Currency);
        if (moneyResult.IsFailure)
            return OperationResult.Failure(moneyResult.Error);

        if (moneyResult.Value!.Amount <= 0)
            return OperationResult.Failure("Amount must be greater than zero");

        Amount = moneyResult.Value!;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public OperationResult UpdateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return OperationResult.Failure("Description is required");

        if (description.Length > 500)
            return OperationResult.Failure("Description cannot exceed 500 characters");

        Description = description;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public OperationResult UpdateCategory(Guid categoryId, Guid? subcategoryId = null)
    {
        CategoryId = categoryId;
        SubcategoryId = subcategoryId;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public OperationResult UpdateAccount(Guid accountId)
    {
        AccountId = accountId;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public OperationResult UpdateNotes(string? notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public OperationResult UpdateLocation(string? location)
    {
        Location = location;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public void AddAttachment(ExpenseAttachment attachment)
    {
        Attachments.Add(attachment);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveAttachment(ExpenseAttachment attachment)
    {
        Attachments.Remove(attachment);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddTag(ExpenseTag tag)
    {
        if (!Tags.Any(t => t.Id == tag.Id))
        {
            Tags.Add(tag);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RemoveTag(ExpenseTag tag)
    {
        Tags.Remove(tag);
        UpdatedAt = DateTime.UtcNow;
    }

    public OperationResult Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }
}
