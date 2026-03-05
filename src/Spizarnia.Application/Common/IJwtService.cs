using Spizarnia.Domain.Entities;

namespace Spizarnia.Application.Common;

public interface IJwtService
{
    (string AccessToken, string RefreshToken) GenerateTokens(User user);
    bool IsValidRefreshTokenFormat(string refreshToken);
}
