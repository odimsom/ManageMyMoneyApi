using System.Text.RegularExpressions;
using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Domain.ValueObjects;

public sealed partial class PhoneNumber : IEquatable<PhoneNumber>
{
    public string Value { get; }
    public string? CountryCode { get; }

    private PhoneNumber(string value, string? countryCode)
    {
        Value = value;
        CountryCode = countryCode;
    }

    public static OperationResult<PhoneNumber> Create(string phoneNumber, string? countryCode = null)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return OperationResult.Failure<PhoneNumber>("Phone number is required");

        var cleaned = CleanPhoneNumber(phoneNumber);

        if (cleaned.Length < 7 || cleaned.Length > 15)
            return OperationResult.Failure<PhoneNumber>("Phone number must be between 7 and 15 digits");

        if (!PhoneRegex().IsMatch(cleaned))
            return OperationResult.Failure<PhoneNumber>("Invalid phone number format");

        return OperationResult.Success(new PhoneNumber(cleaned, countryCode?.Trim()));
    }

    private static string CleanPhoneNumber(string phone) =>
        new(phone.Where(char.IsDigit).ToArray());

    [GeneratedRegex(@"^\d{7,15}$", RegexOptions.Compiled)]
    private static partial Regex PhoneRegex();

    public string FormattedValue => CountryCode is not null 
        ? $"+{CountryCode} {Value}" 
        : Value;

    public bool Equals(PhoneNumber? other) => 
        other is not null && Value == other.Value && CountryCode == other.CountryCode;
    public override bool Equals(object? obj) => Equals(obj as PhoneNumber);
    public override int GetHashCode() => HashCode.Combine(Value, CountryCode);
    public override string ToString() => FormattedValue;
}
