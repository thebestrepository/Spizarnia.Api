using FluentValidation;
using MediatR;
using Spizarnia.Application.Common;
using Spizarnia.Domain.Entities;
using Spizarnia.Domain.Interfaces;

namespace Spizarnia.Application.Auth;

public record RegisterCommand(string Email, string Name, string Password) : IRequest<Result<AuthResponse>>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).EmailAddress().NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Password).MinimumLength(8).NotEmpty();
    }
}

public class RegisterCommandHandler(IUserRepository users, IJwtService jwt) : IRequestHandler<RegisterCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(RegisterCommand cmd, CancellationToken ct)
    {
        var existing = await users.GetByEmailAsync(cmd.Email, ct);
        if (existing is not null)
            return Result<AuthResponse>.Failure("Email already registered.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = cmd.Email.ToLowerInvariant(),
            Name = cmd.Name,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(cmd.Password),
            CreatedAt = DateTime.UtcNow
        };

        var (accessToken, refreshToken) = jwt.GenerateTokens(user);
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        await users.AddAsync(user, ct);
        await users.SaveChangesAsync(ct);

        return Result<AuthResponse>.Success(new AuthResponse(accessToken, refreshToken, user.Id, user.Email, user.Name));
    }
}
