using System.Text.RegularExpressions;
using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Domain.ValueObjects;

public sealed partial class Email : IEquatable<Email>
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static OperationResult<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return OperationResult.Failure<Email>("Email is required");

        email = email.Trim().ToLowerInvariant();

        if (email.Length > 256)
            return OperationResult.Failure<Email>("Email cannot exceed 256 characters");

        if (!EmailRegex().IsMatch(email))
            return OperationResult.Failure<Email>("Invalid email format");

        return OperationResult.Success(new Email(email));
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();

    public bool Equals(Email? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => Equals(obj as Email);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
