using FluentValidation;
using MediatR;
using Spizarnia.Application.Common;
using Spizarnia.Domain.Interfaces;

namespace Spizarnia.Application.Auth;

public record LoginCommand(string Email, string Password) : IRequest<Result<AuthResponse>>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).EmailAddress().NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginCommandHandler(IUserRepository users, IJwtService jwt) : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(LoginCommand cmd, CancellationToken ct)
    {
        var user = await users.GetByEmailAsync(cmd.Email.ToLowerInvariant(), ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(cmd.Password, user.PasswordHash))
            return Result<AuthResponse>.Failure("Invalid email or password.");

        var (accessToken, refreshToken) = jwt.GenerateTokens(user);
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await users.UpdateAsync(user, ct);
        await users.SaveChangesAsync(ct);

        return Result<AuthResponse>.Success(new AuthResponse(accessToken, refreshToken, user.Id, user.Email, user.Name));
    }
}
