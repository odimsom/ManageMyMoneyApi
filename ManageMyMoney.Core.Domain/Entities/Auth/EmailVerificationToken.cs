using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Domain.Entities.Auth;

public class EmailVerificationToken
{
    public Guid Id { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsUsed { get; private set; }

    private EmailVerificationToken() { }

    public static OperationResult<EmailVerificationToken> Create(
        string token,
        Guid userId,
        int expirationHours = 48)
    {
        if (string.IsNullOrWhiteSpace(token))
            return OperationResult.Failure<EmailVerificationToken>("Token is required");

        var verificationToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddHours(expirationHours),
            CreatedAt = DateTime.UtcNow,
            IsUsed = false
        };

        return OperationResult.Success(verificationToken);
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsValid => !IsUsed && !IsExpired;

    public OperationResult MarkAsUsed()
    {
        if (!IsValid)
            return OperationResult.Failure("Token is invalid or expired");

        IsUsed = true;
        return OperationResult.Success();
    }
}
