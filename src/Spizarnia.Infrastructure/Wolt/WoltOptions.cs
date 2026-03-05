namespace Spizarnia.Infrastructure.Wolt;

public class WoltOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string TokenEndpoint { get; set; } = "https://integrations-authentication-service.wolt.com/oauth2/token";
    public string DriveBaseUrl { get; set; } = "https://restaurant-api.wolt.com";
}
