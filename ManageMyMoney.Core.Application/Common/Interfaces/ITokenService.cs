using ManageMyMoney.Core.Domain.Common;

namespace ManageMyMoney.Core.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string email, IEnumerable<string>? roles = null);
    string GenerateRefreshToken();
    string GenerateRandomToken(int length = 32);
    OperationResult<Guid> ValidateAccessToken(string token);
}
