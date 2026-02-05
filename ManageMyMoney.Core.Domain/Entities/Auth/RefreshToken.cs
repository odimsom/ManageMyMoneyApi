using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Domain.Entities.Auth;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? CreatedByIp { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string? ReplacedByToken { get; private set; }

    private RefreshToken() { }

    public static OperationResult<RefreshToken> Create(
        string token,
        Guid userId,
        int expirationDays = 7,
        string? createdByIp = null)
    {
        if (string.IsNullOrWhiteSpace(token))
            return OperationResult.Failure<RefreshToken>("Token is required");

        if (userId == Guid.Empty)
            return OperationResult.Failure<RefreshToken>("User ID is required");

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = createdByIp
        };

        return OperationResult.Success(refreshToken);
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive => !IsRevoked && !IsExpired;

    public OperationResult Revoke(string? revokedByIp = null, string? replacedByToken = null)
    {
        if (IsRevoked)
            return OperationResult.Failure("Token is already revoked");

        RevokedAt = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
        ReplacedByToken = replacedByToken;
        return OperationResult.Success();
    }
}
