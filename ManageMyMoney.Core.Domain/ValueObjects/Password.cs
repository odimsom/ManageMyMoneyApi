using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Domain.ValueObjects;

public class Password
{
    public string HashedValue { get; private set; } = string.Empty;

    private Password() { }

    private Password(string hashedValue)
    {
        HashedValue = hashedValue;
    }

    /// <summary>
    /// Creates a Password from an already hashed value (from database or after hashing)
    /// </summary>
    public static OperationResult<Password> CreateFromHash(string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword))
            return OperationResult.Failure<Password>("Password hash cannot be empty");

        return OperationResult.Success(new Password(hashedPassword));
    }

    /// <summary>
    /// Validates a raw password against security requirements (before hashing)
    /// </summary>
    public static OperationResult ValidateRawPassword(string rawPassword)
    {
        if (string.IsNullOrWhiteSpace(rawPassword))
            return OperationResult.Failure("Password is required");

        if (rawPassword.Length < 8)
            return OperationResult.Failure("Password must be at least 8 characters long");

        if (rawPassword.Length > 128)
            return OperationResult.Failure("Password cannot exceed 128 characters");

        // Check for at least one uppercase letter
        if (!rawPassword.Any(char.IsUpper))
            return OperationResult.Failure("Password must contain at least one uppercase letter");

        // Check for at least one lowercase letter
        if (!rawPassword.Any(char.IsLower))
            return OperationResult.Failure("Password must contain at least one lowercase letter");

        // Check for at least one digit
        if (!rawPassword.Any(char.IsDigit))
            return OperationResult.Failure("Password must contain at least one digit");

        return OperationResult.Success();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Password other)
            return false;

        return HashedValue == other.HashedValue;
    }

    public override int GetHashCode() => HashedValue.GetHashCode();

    public override string ToString() => "********"; // Never expose the hash
}
