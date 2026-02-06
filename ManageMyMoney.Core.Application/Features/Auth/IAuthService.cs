using ManageMyMoney.Core.Application.DTOs.Auth;
using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Application.Features.Auth;

public interface IAuthService
{
    Task<OperationResult<AuthResponse>> RegisterAsync(RegisterUserRequest request);
    Task<OperationResult<AuthResponse>> LoginAsync(LoginRequest request);
    Task<OperationResult<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    Task<OperationResult> LogoutAsync(string refreshToken);
    Task<OperationResult> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<OperationResult> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<OperationResult> ResetPasswordAsync(ResetPasswordRequest request);
    Task<OperationResult> VerifyEmailAsync(VerifyEmailRequest request);
    Task<OperationResult> ResendVerificationEmailAsync(Guid userId, string verificationUrl);
    Task<OperationResult<UserDto>> GetCurrentUserAsync(Guid userId);
    Task<OperationResult<UserDto>> UpdateProfileAsync(Guid userId, UpdateUserProfileRequest request);
    Task<OperationResult> DeactivateAccountAsync(Guid userId);
}
