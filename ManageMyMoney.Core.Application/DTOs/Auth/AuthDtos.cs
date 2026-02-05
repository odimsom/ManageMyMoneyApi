namespace ManageMyMoney.Core.Application.DTOs.Auth;

public record RegisterUserRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string FirstName { get; init; }
    public string? LastName { get; init; }
    public string PreferredCurrency { get; init; } = "USD";
}

public record LoginRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}

public record AuthResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public DateTime ExpiresAt { get; init; }
    public required UserDto User { get; init; }
}

public record RefreshTokenRequest
{
    public required string RefreshToken { get; init; }
}

public record ChangePasswordRequest
{
    public required string CurrentPassword { get; init; }
    public required string NewPassword { get; init; }
}

public record ForgotPasswordRequest
{
    public required string Email { get; init; }
}

public record ResetPasswordRequest
{
    public required string Token { get; init; }
    public required string NewPassword { get; init; }
}

public record VerifyEmailRequest
{
    public required string Token { get; init; }
}

public record UserDto
{
    public Guid Id { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public string? LastName { get; init; }
    public string? FullName { get; init; }
    public required string PreferredCurrency { get; init; }
    public bool IsEmailVerified { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record UpdateUserProfileRequest
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? TimeZone { get; init; }
    public string? PreferredCurrency { get; init; }
}
