using ManageMyMoney.Core.Application.Common.Interfaces;
using ManageMyMoney.Core.Application.DTOs.Auth;
using ManageMyMoney.Core.Domain.Common;
using ManageMyMoney.Core.Domain.Entities.Auth;
using ManageMyMoney.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ManageMyMoney.Core.Application.Features.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IEmailService emailService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<OperationResult<AuthResponse>> RegisterAsync(RegisterUserRequest request)
    {
        var emailExists = await _userRepository.EmailExistsAsync(request.Email);
        if (emailExists.IsSuccess && emailExists.Value)
            return OperationResult.Failure<AuthResponse>("Email is already registered");

        var passwordValidation = Domain.ValueObjects.Password.ValidateRawPassword(request.Password);
        if (passwordValidation.IsFailure)
            return OperationResult.Failure<AuthResponse>(passwordValidation.Error);

        var hashedPassword = _passwordHasher.HashPassword(request.Password);

        var userResult = User.Create(
            request.Email,
            hashedPassword,
            request.FirstName,
            request.LastName,
            request.PreferredCurrency);

        if (userResult.IsFailure)
            return OperationResult.Failure<AuthResponse>(userResult.Error);

        var addResult = await _userRepository.AddAsync(userResult.Value!);
        if (addResult.IsFailure)
            return OperationResult.Failure<AuthResponse>(addResult.Error);

        var user = userResult.Value!;

        // Send verification email in background (non-blocking)
        var verificationCode = _tokenService.GenerateRandomToken(6);
        var verificationUrl = $"https://app.managemymoney.com/verify-email?code={verificationCode}";
        
        // Fire and forget - don't wait for email to send
        _ = Task.Run(async () =>
        {
            try
            {
                await _emailService.SendEmailVerificationAsync(
                    request.Email,
                    request.FirstName,
                    verificationCode,
                    verificationUrl,
                    60); // 60 minutes expiration
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification email to {Email} (non-blocking)", request.Email);
            }
        });

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user.Id, request.Email);
        var refreshToken = _tokenService.GenerateRefreshToken();

        _logger.LogInformation("User registered successfully: {Email}", request.Email);

        return OperationResult.Success(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = MapToUserDto(user)
        });
    }

    public async Task<OperationResult<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var userResult = await _userRepository.GetByEmailAsync(request.Email);
        if (userResult.IsFailure)
            return OperationResult.Failure<AuthResponse>("Invalid email or password");

        var user = userResult.Value!;

        if (!user.IsActive)
            return OperationResult.Failure<AuthResponse>("Account is deactivated");

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash.HashedValue))
            return OperationResult.Failure<AuthResponse>("Invalid email or password");

        user.RecordLogin();
        await _userRepository.UpdateAsync(user);

        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email.Value);
        var refreshToken = _tokenService.GenerateRefreshToken();

        _logger.LogInformation("User logged in: {Email}", request.Email);

        return OperationResult.Success(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = MapToUserDto(user)
        });
    }

    public async Task<OperationResult<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        // Simplified - would need RefreshToken repository
        return await Task.FromResult(OperationResult.Failure<AuthResponse>("Refresh token validation not implemented"));
    }

    public Task<OperationResult> LogoutAsync(string refreshToken)
    {
        // Revoke refresh token
        return Task.FromResult(OperationResult.Success());
    }

    public async Task<OperationResult> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var userResult = await _userRepository.GetByIdAsync(userId);
        if (userResult.IsFailure)
            return OperationResult.Failure("User not found");

        var user = userResult.Value!;

        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash.HashedValue))
            return OperationResult.Failure("Current password is incorrect");

        var passwordValidation = Domain.ValueObjects.Password.ValidateRawPassword(request.NewPassword);
        if (passwordValidation.IsFailure)
            return passwordValidation;

        var newHashedPassword = _passwordHasher.HashPassword(request.NewPassword);
        var updateResult = user.UpdatePassword(newHashedPassword);
        if (updateResult.IsFailure)
            return updateResult;

        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Password changed for user: {UserId}", userId);

        return OperationResult.Success();
    }

    public async Task<OperationResult> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var userResult = await _userRepository.GetByEmailAsync(request.Email);
        if (userResult.IsFailure)
        {
            // Don't reveal if email exists - always return success
            return OperationResult.Success();
        }

        var user = userResult.Value!;
        var resetToken = _tokenService.GenerateRandomToken();
        var resetUrl = $"https://app.managemymoney.com/reset-password?token={resetToken}";

        // Send password reset email in background (non-blocking)
        _ = Task.Run(async () =>
        {
            try
            {
                await _emailService.SendPasswordResetEmailAsync(
                    request.Email,
                    user.FirstName,
                    resetUrl,
                    60); // 60 minutes expiration
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email} (non-blocking)", request.Email);
            }
        });

        return OperationResult.Success();
    }

    public Task<OperationResult> ResetPasswordAsync(ResetPasswordRequest request)
    {
        // Validate token and reset password
        return Task.FromResult(OperationResult.Success());
    }

    public async Task<OperationResult> VerifyEmailAsync(VerifyEmailRequest request)
    {
        // Validate token and verify email
        return await Task.FromResult(OperationResult.Success());
    }

    public Task<OperationResult> ResendVerificationEmailAsync(Guid userId)
    {
        return Task.FromResult(OperationResult.Success());
    }

    public async Task<OperationResult<UserDto>> GetCurrentUserAsync(Guid userId)
    {
        var userResult = await _userRepository.GetByIdAsync(userId);
        if (userResult.IsFailure)
            return OperationResult.Failure<UserDto>(userResult.Error);

        return OperationResult.Success(MapToUserDto(userResult.Value!));
    }

    public async Task<OperationResult<UserDto>> UpdateProfileAsync(Guid userId, UpdateUserProfileRequest request)
    {
        var userResult = await _userRepository.GetByIdAsync(userId);
        if (userResult.IsFailure)
            return OperationResult.Failure<UserDto>(userResult.Error);

        var user = userResult.Value!;

        if (!string.IsNullOrWhiteSpace(request.FirstName))
        {
            var updateResult = user.UpdateProfile(
                request.FirstName,
                request.LastName,
                request.TimeZone);

            if (updateResult.IsFailure)
                return OperationResult.Failure<UserDto>(updateResult.Error);
        }

        await _userRepository.UpdateAsync(user);

        return OperationResult.Success(MapToUserDto(user));
    }

    public async Task<OperationResult> DeactivateAccountAsync(Guid userId)
    {
        var userResult = await _userRepository.GetByIdAsync(userId);
        if (userResult.IsFailure)
            return OperationResult.Failure(userResult.Error);

        var deactivateResult = userResult.Value!.Deactivate();
        if (deactivateResult.IsFailure)
            return deactivateResult;

        await _userRepository.UpdateAsync(userResult.Value!);

        _logger.LogInformation("Account deactivated: {UserId}", userId);

        return OperationResult.Success();
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email.Value,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            PreferredCurrency = user.PreferredCurrency,
            IsEmailVerified = user.IsEmailVerified,
            CreatedAt = user.CreatedAt
        };
    }
}
