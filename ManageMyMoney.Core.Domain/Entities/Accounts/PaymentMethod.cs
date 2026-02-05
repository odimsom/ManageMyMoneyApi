using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Domain.Entities.Accounts;

public class PaymentMethod
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public string? LastFourDigits { get; private set; }
    public string? Icon { get; private set; }
    public Guid? AccountId { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private PaymentMethod() { }

    public static OperationResult<PaymentMethod> Create(
        string name,
        string type,
        Guid userId,
        Guid? accountId = null,
        string? lastFourDigits = null,
        string? icon = null,
        bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            return OperationResult.Failure<PaymentMethod>("Payment method name is required");

        if (string.IsNullOrWhiteSpace(type))
            return OperationResult.Failure<PaymentMethod>("Payment method type is required");

        if (lastFourDigits is not null && lastFourDigits.Length != 4)
            return OperationResult.Failure<PaymentMethod>("Last four digits must be exactly 4 characters");

        var method = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Type = type.Trim(),
            LastFourDigits = lastFourDigits,
            Icon = icon,
            AccountId = accountId,
            UserId = userId,
            IsDefault = isDefault,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return OperationResult.Success(method);
    }

    public void SetAsDefault() => IsDefault = true;
    public void RemoveDefault() => IsDefault = false;
    public void Deactivate() => IsActive = false;
}
