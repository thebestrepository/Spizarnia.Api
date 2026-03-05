using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Spizarnia.Infrastructure.Wolt;

public class WoltAuthService(HttpClient http, IOptions<WoltOptions> options, IMemoryCache cache)
{
    private const string CacheKey = "wolt_access_token";

    public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
    {
        if (cache.TryGetValue(CacheKey, out string? token) && token is not null)
            return token;

        var opts = options.Value;
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = opts.ClientId,
            ["client_secret"] = opts.ClientSecret
        };

        var response = await http.PostAsync(opts.TokenEndpoint, new FormUrlEncodedContent(form), ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<TokenResponse>(json)
            ?? throw new InvalidOperationException("Failed to deserialize Wolt token response");

        cache.Set(CacheKey, result.AccessToken, TimeSpan.FromSeconds(result.ExpiresIn - 60));
        return result.AccessToken;
    }

    private record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
}
