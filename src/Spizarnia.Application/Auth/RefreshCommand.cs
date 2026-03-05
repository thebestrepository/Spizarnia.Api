using MediatR;
using Spizarnia.Application.Common;
using Spizarnia.Domain.Interfaces;

namespace Spizarnia.Application.Auth;

public record RefreshCommand(string RefreshToken) : IRequest<Result<AuthResponse>>;

public class RefreshCommandHandler(IUserRepository users, IJwtService jwt) : IRequestHandler<RefreshCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(RefreshCommand cmd, CancellationToken ct)
    {
        if (!jwt.IsValidRefreshTokenFormat(cmd.RefreshToken))
            return Result<AuthResponse>.Failure("Invalid refresh token.");

        // Find user by refresh token stored in DB
        var user = await users.GetByRefreshTokenAsync(cmd.RefreshToken, ct);
        if (user is null || user.RefreshTokenExpiry < DateTime.UtcNow)
            return Result<AuthResponse>.Failure("Refresh token expired or invalid.");

        var (accessToken, refreshToken) = jwt.GenerateTokens(user);
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await users.UpdateAsync(user, ct);
        await users.SaveChangesAsync(ct);

        return Result<AuthResponse>.Success(new AuthResponse(accessToken, refreshToken, user.Id, user.Email, user.Name));
    }
}
