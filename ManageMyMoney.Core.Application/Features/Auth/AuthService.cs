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
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IEmailService emailService,
        IFileStorageService fileStorageService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _emailService = emailService;
        _fileStorageService = fileStorageService;
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

        // Create and save verification token
        var verificationToken = _tokenService.GenerateRandomToken();
        var tokenResult = EmailVerificationToken.Create(verificationToken, user.Id, 48); // 48 hours expiration
        if (tokenResult.IsFailure)
            return OperationResult.Failure<AuthResponse>(tokenResult.Error);

        var emailVerificationToken = tokenResult.Value!;
        var saveTokenResult = await _userRepository.AddEmailVerificationTokenAsync(emailVerificationToken);
        if (saveTokenResult.IsFailure)
            _logger.LogWarning("Failed to save email verification token for {Email}: {Error}", request.Email, saveTokenResult.Error);

        // Send verification email with timeout (non-blocking after 3 seconds)
        var verificationUrl = $"{request.VerificationUrl}?token={verificationToken}";
        
        // Try to send email with 3-second timeout, then continue regardless
        var emailTask = Task.Run(async () =>
        {
            try
            {
                _logger.LogInformation("🚀 Starting email send task for {Email}", request.Email);
                var result = await _emailService.SendEmailVerificationAsync(
                    request.Email,
                    request.FirstName,
                    verificationToken,
                    verificationUrl,
                    60); // 60 minutes expiration for email link display
                
                if (result.IsSuccess)
                {
                    _logger.LogInformation("✅ Email verification sent successfully to {Email}", request.Email);
                }
                else
                {
                    _logger.LogWarning("⚠️ Email verification failed for {Email}: {Error}", request.Email, result.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception sending verification email to {Email}", request.Email);
            }
        });

        // Wait up to 3 seconds for email, then continue
        _ = Task.WhenAny(emailTask, Task.Delay(3000));

        _logger.LogInformation("User registered successfully: {Email}. Verification required.", request.Email);

        return OperationResult.Success(new AuthResponse
        {
            AccessToken = string.Empty,
            RefreshToken = string.Empty,
            ExpiresAt = DateTime.UtcNow,
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

        if (!user.IsEmailVerified)
            return OperationResult.Failure<AuthResponse>("Please verify your email address before logging in. Check your inbox for the verification link.");

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
        return await Task.FromResult(OperationResult.Failure<AuthResponse>("Refresh token validation not implemented"));
    }

    public Task<OperationResult> LogoutAsync(string refreshToken)
    {
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
            return OperationResult.Success();
        }

        var user = userResult.Value!;
        var resetTokenValue = _tokenService.GenerateRandomToken();

        var tokenResult = PasswordResetToken.Create(resetTokenValue, user.Id, 24);
        if (tokenResult.IsFailure)
            return OperationResult.Failure(tokenResult.Error);

        var resetToken = tokenResult.Value!;
        
        var saveTokenResult = await _userRepository.AddPasswordResetTokenAsync(resetToken);
        if (saveTokenResult.IsFailure)
            return OperationResult.Failure("Failed to create reset token");

        var resetUrl = $"https://app.managemymoney.com/reset-password?token={resetTokenValue}";

        // Send password reset email in background (non-blocking)
        _ = Task.Run(async () =>
        {
            try
            {
                await _emailService.SendPasswordResetEmailAsync(
                    request.Email,
                    user.FirstName,
                    resetUrl,
                    60); // 60 minutes expiration for email link display
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email} (non-blocking)", request.Email);
            }
        });

        return OperationResult.Success();
    }

    public async Task<OperationResult> ResetPasswordAsync(ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return OperationResult.Failure("Reset token is required");

        if (string.IsNullOrWhiteSpace(request.NewPassword))
            return OperationResult.Failure("New password is required");

        // Validate password requirements
        var passwordValidation = Domain.ValueObjects.Password.ValidateRawPassword(request.NewPassword);
        if (passwordValidation.IsFailure)
            return passwordValidation;

        // Get and validate reset token
        var tokenResult = await _userRepository.GetValidPasswordResetTokenAsync(request.Token);
        if (tokenResult.IsFailure)
            return OperationResult.Failure("Invalid or expired reset token");

        var resetToken = tokenResult.Value!;

        // Get user
        var userResult = await _userRepository.GetByIdAsync(resetToken.UserId);
        if (userResult.IsFailure)
            return OperationResult.Failure("User not found");

        var user = userResult.Value!;

        // Mark token as used
        var markTokenResult = resetToken.MarkAsUsed();
        if (markTokenResult.IsFailure)
            return markTokenResult;

        await _userRepository.UpdatePasswordResetTokenAsync(resetToken);

        // Update password
        var newHashedPassword = _passwordHasher.HashPassword(request.NewPassword);
        var updateResult = user.UpdatePassword(newHashedPassword);
        if (updateResult.IsFailure)
            return updateResult;

        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Password reset completed for user: {UserId}", user.Id);

        return OperationResult.Success();
    }

    public async Task<OperationResult> VerifyEmailAsync(VerifyEmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return OperationResult.Failure("Verification token is required");

        // Get and validate verification token
        var tokenResult = await _userRepository.GetValidEmailVerificationTokenAsync(request.Token);
        if (tokenResult.IsFailure)
            return OperationResult.Failure("Invalid or expired verification token");

        var verificationToken = tokenResult.Value!;

        // Get user
        var userResult = await _userRepository.GetByIdAsync(verificationToken.UserId);
        if (userResult.IsFailure)
            return OperationResult.Failure("User not found");

        var user = userResult.Value!;

        // Check if email is already verified
        if (user.IsEmailVerified)
            return OperationResult.Failure("Email is already verified");

        // Mark token as used
        var markTokenResult = verificationToken.MarkAsUsed();
        if (markTokenResult.IsFailure)
            return markTokenResult;

        await _userRepository.UpdateEmailVerificationTokenAsync(verificationToken);

        // Verify email
        var verifyResult = user.VerifyEmail();
        if (verifyResult.IsFailure)
            return verifyResult;

        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Email verified successfully for user: {UserId}", user.Id);

        return OperationResult.Success();
    }

    public async Task<OperationResult> ResendVerificationEmailAsync(Guid userId, string verificationUrl)
    {
        if (userId == Guid.Empty)
            return OperationResult.Failure("User ID is required");

        // Get user
        var userResult = await _userRepository.GetByIdAsync(userId);
        if (userResult.IsFailure)
            return OperationResult.Failure("User not found");

        var user = userResult.Value!;

        // Check if email is already verified
        if (user.IsEmailVerified)
            return OperationResult.Failure("Email is already verified");

        // Generate new verification token
        var verificationToken = _tokenService.GenerateRandomToken();
        var tokenResult = EmailVerificationToken.Create(verificationToken, user.Id, 48); // 48 hours expiration
        if (tokenResult.IsFailure)
            return OperationResult.Failure(tokenResult.Error);

        var emailVerificationToken = tokenResult.Value!;
        
        // Save token to database
        var saveTokenResult = await _userRepository.AddEmailVerificationTokenAsync(emailVerificationToken);
        if (saveTokenResult.IsFailure)
            return OperationResult.Failure("Failed to create verification token");

        // Send verification email (non-blocking with timeout)
        var verificationUrlWithToken = $"{verificationUrl}?token={verificationToken}";
        
        var emailTask = Task.Run(async () =>
        {
            try
            {
                _logger.LogInformation("🔄 Resending email verification for {Email}", user.Email.Value);
                var result = await _emailService.SendEmailVerificationAsync(
                    user.Email.Value,
                    user.FirstName,
                    verificationToken,
                    verificationUrlWithToken,
                    60); // 60 minutes for email link display
                
                if (result.IsSuccess)
                {
                    _logger.LogInformation("✅ Verification email resent successfully to {Email}", user.Email.Value);
                }
                else
                {
                    _logger.LogWarning("⚠️ Failed to resend verification email to {Email}: {Error}", user.Email.Value, result.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception resending verification email to {Email}", user.Email.Value);
            }
        });

        // Wait up to 3 seconds for email, then continue
        _ = Task.WhenAny(emailTask, Task.Delay(3000));

        _logger.LogInformation("Verification email resend initiated for user: {UserId}", userId);

        return OperationResult.Success();
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

        if (!string.IsNullOrWhiteSpace(request.PreferredCurrency))
        {
            // Assuming this is added to User entity as well or handled here
            // For now just updating if provided
        }

        if (request.AvatarUrl != null)
        {
            user.UpdateAvatar(request.AvatarUrl);
        }

        await _userRepository.UpdateAsync(user);

        return OperationResult.Success(MapToUserDto(user));
    }

    public async Task<OperationResult<UserDto>> UploadAvatarAsync(Guid userId, Stream fileStream, string fileName, string contentType)
    {
        var userResult = await _userRepository.GetByIdAsync(userId);
        if (userResult.IsFailure)
            return OperationResult.Failure<UserDto>(userResult.Error);

        var user = userResult.Value!;

        // Delete old avatar if it exists
        if (!string.IsNullOrEmpty(user.AvatarUrl))
        {
            await _fileStorageService.DeleteFileAsync(user.AvatarUrl);
        }

        var uploadResult = await _fileStorageService.UploadFileAsync(fileStream, fileName, contentType);
        if (uploadResult.IsFailure)
            return OperationResult.Failure<UserDto>(uploadResult.Error);

        user.UpdateAvatar(uploadResult.Value);
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
            AvatarUrl = user.AvatarUrl,
            PreferredCurrency = user.PreferredCurrency,
            IsEmailVerified = user.IsEmailVerified,
            CreatedAt = user.CreatedAt
        };
    }
}
