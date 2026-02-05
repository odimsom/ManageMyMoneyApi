using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Accounts;
using ManageMyMoney.Core.Domain.Entities.Expenses;
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
    public string? AvatarUrl { get; private set; }
    public string PreferredCurrency { get; private set; } = "USD";
    public string? TimeZone { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public bool IsTwoFactorEnabled { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Computed properties
    public string FullName => string.IsNullOrWhiteSpace(LastName) ? FirstName : $"{FirstName} {LastName}";

    // Navigation properties
    public virtual ICollection<Account> Accounts { get; private set; } = new List<Account>();
    public virtual ICollection<Expense> Expenses { get; private set; } = new List<Expense>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
    public virtual ICollection<UserSession> Sessions { get; private set; } = new List<UserSession>();

    private User() { }

    public static OperationResult<User> Create(
        string email,
        string hashedPassword,
        string firstName,
        string? lastName = null,
        string preferredCurrency = "USD")
    {
        var emailResult = Email.Create(email);
        if (emailResult.IsFailure)
            return OperationResult.Failure<User>(emailResult.Error);

        var passwordResult = Password.CreateFromHash(hashedPassword);
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
            FirstName = firstName,
            LastName = lastName,
            PreferredCurrency = preferredCurrency,
            IsEmailVerified = false,
            IsTwoFactorEnabled = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return OperationResult.Success(user);
    }

    public OperationResult UpdateProfile(string firstName, string? lastName = null, string? timeZone = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return OperationResult.Failure("First name is required");

        FirstName = firstName;
        LastName = lastName;
        TimeZone = timeZone;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public OperationResult UpdateAvatar(string? avatarUrl)
    {
        AvatarUrl = avatarUrl;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public OperationResult UpdatePassword(string newHashedPassword)
    {
        var passwordResult = Password.CreateFromHash(newHashedPassword);
        if (passwordResult.IsFailure)
            return OperationResult.Failure(passwordResult.Error);

        PasswordHash = passwordResult.Value!;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public OperationResult VerifyEmail()
    {
        IsEmailVerified = true;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public OperationResult Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public OperationResult Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public OperationResult EnableTwoFactor()
    {
        IsTwoFactorEnabled = true;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }

    public OperationResult DisableTwoFactor()
    {
        IsTwoFactorEnabled = false;
        UpdatedAt = DateTime.UtcNow;
        return OperationResult.Success();
    }
}
