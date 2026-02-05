using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Domain.Entities.Auth;

public class PasswordResetToken
{
    public Guid Id { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTime? UsedAt { get; private set; }

    private PasswordResetToken() { }

    public static OperationResult<PasswordResetToken> Create(
        string token,
        Guid userId,
        int expirationHours = 24)
    {
        if (string.IsNullOrWhiteSpace(token))
            return OperationResult.Failure<PasswordResetToken>("Token is required");

        if (userId == Guid.Empty)
            return OperationResult.Failure<PasswordResetToken>("User ID is required");

        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddHours(expirationHours),
            CreatedAt = DateTime.UtcNow,
            IsUsed = false
        };

        return OperationResult.Success(resetToken);
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsValid => !IsUsed && !IsExpired;

    public OperationResult MarkAsUsed()
    {
        if (IsUsed)
            return OperationResult.Failure("Token has already been used");

        if (IsExpired)
            return OperationResult.Failure("Token has expired");

        IsUsed = true;
        UsedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }
}
