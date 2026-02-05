using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Domain.ValueObjects;

public sealed class Password : IEquatable<Password>
{
    public string HashedValue { get; }

    private Password(string hashedValue) => HashedValue = hashedValue;

    public static OperationResult<Password> Create(string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword))
            return OperationResult.Failure<Password>("Password hash is required");

        return OperationResult.Success(new Password(hashedPassword));
    }

    public static OperationResult ValidateRawPassword(string rawPassword)
    {
        if (string.IsNullOrWhiteSpace(rawPassword))
            return OperationResult.Failure("Password is required");

        if (rawPassword.Length < 8)
            return OperationResult.Failure("Password must be at least 8 characters");

        if (rawPassword.Length > 128)
            return OperationResult.Failure("Password cannot exceed 128 characters");

        if (!rawPassword.Any(char.IsUpper))
            return OperationResult.Failure("Password must contain at least one uppercase letter");

        if (!rawPassword.Any(char.IsLower))
            return OperationResult.Failure("Password must contain at least one lowercase letter");

        if (!rawPassword.Any(char.IsDigit))
            return OperationResult.Failure("Password must contain at least one digit");

        if (!rawPassword.Any(c => !char.IsLetterOrDigit(c)))
            return OperationResult.Failure("Password must contain at least one special character");

        return OperationResult.Success();
    }

    public bool Equals(Password? other) => other is not null && HashedValue == other.HashedValue;
    public override bool Equals(object? obj) => Equals(obj as Password);
    public override int GetHashCode() => HashedValue.GetHashCode();
}
