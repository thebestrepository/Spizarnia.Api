namespace Spizarnia.Application.Auth;

public record AuthResponse(string AccessToken, string RefreshToken, Guid UserId, string Email, string Name);
