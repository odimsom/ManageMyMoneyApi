using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.ValueObjects;

namespace ManageMyMoney.Core.Domain.Entities.Auth;

public class User
{
    public Guid Id { get; private set; }
    public Email Email { get; private set; } = null!;
    public Password PasswordHash { get; private set; } = null!;
    public string FirstName { get; private set; } = string.Empty;
    public string? LastName { get; private set; }
    public PhoneNumber? PhoneNumber { get; private set; }
    public string PreferredCurrency { get; private set; } = "USD";
    public string? TimeZone { get; private set; }
    public string? AvatarUrl { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsTwoFactorEnabled { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    private User() { }

    public static OperationResult<User> Create(
        string email,
        string passwordHash,
        string firstName,
        string? lastName = null,
        string preferredCurrency = "USD")
    {
        var emailResult = Email.Create(email);
        if (emailResult.IsFailure)
            return OperationResult.Failure<User>(emailResult.Error);

        var passwordResult = Password.Create(passwordHash);
        if (passwordResult.IsFailure)
            return OperationResult.Failure<User>(passwordResult.Error);

        if (string.IsNullOrWhiteSpace(firstName))
            return OperationResult.Failure<User>("First name is required");

        if (firstName.Length > 100)
            return OperationResult.Failure<User>("First name cannot exceed 100 characters");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = emailResult.Value!,
            PasswordHash = passwordResult.Value!,
            FirstName = firstName.Trim(),
            LastName = lastName?.Trim(),
            PreferredCurrency = preferredCurrency.ToUpperInvariant(),
            IsEmailVerified = false,
            IsActive = true,
            IsTwoFactorEnabled = false,
            CreatedAt = DateTime.UtcNow
        };

        return OperationResult.Success(user);
    }

    public OperationResult VerifyEmail()
    {
        if (IsEmailVerified)
            return OperationResult.Failure("Email is already verified");

        IsEmailVerified = true;
        return OperationResult.Success();
    }

    public OperationResult UpdatePassword(string newPasswordHash)
    {
        var passwordResult = Password.Create(newPasswordHash);
        if (passwordResult.IsFailure)
            return OperationResult.Failure(passwordResult.Error);

        PasswordHash = passwordResult.Value!;
        return OperationResult.Success();
    }

    public OperationResult UpdateProfile(string firstName, string? lastName, string? timeZone)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return OperationResult.Failure("First name is required");

        FirstName = firstName.Trim();
        LastName = lastName?.Trim();
        TimeZone = timeZone;
        return OperationResult.Success();
    }

    public void RecordLogin() => LastLoginAt = DateTime.UtcNow;

    public OperationResult Deactivate()
    {
        if (!IsActive)
            return OperationResult.Failure("User is already inactive");

        IsActive = false;
        return OperationResult.Success();
    }

    public void EnableTwoFactor() => IsTwoFactorEnabled = true;
    public void DisableTwoFactor() => IsTwoFactorEnabled = false;

    public string FullName => string.IsNullOrWhiteSpace(LastName) 
        ? FirstName 
        : $"{FirstName} {LastName}";
}
