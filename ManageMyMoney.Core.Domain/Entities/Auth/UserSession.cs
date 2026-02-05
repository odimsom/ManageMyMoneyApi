using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Domain.Entities.Auth;

public class UserSession
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? DeviceInfo { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastActivityAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public bool IsActive { get; private set; }

    private UserSession() { }

    public static OperationResult<UserSession> Create(
        Guid userId,
        string? ipAddress = null,
        string? userAgent = null,
        string? deviceInfo = null)
    {
        if (userId == Guid.Empty)
            return OperationResult.Failure<UserSession>("User ID is required");

        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DeviceInfo = deviceInfo,
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
            IsActive = true
        };

        return OperationResult.Success(session);
    }

    public void RecordActivity() => LastActivityAt = DateTime.UtcNow;

    public OperationResult End()
    {
        if (!IsActive)
            return OperationResult.Failure("Session is already ended");

        IsActive = false;
        EndedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }
}
