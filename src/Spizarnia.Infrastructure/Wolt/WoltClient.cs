using Microsoft.Extensions.Options;
using Spizarnia.Domain.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Spizarnia.Infrastructure.Wolt;

public class WoltClient(HttpClient http, WoltAuthService auth, IOptions<WoltOptions> options) : IWoltClient
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    private async Task AuthorizeAsync(CancellationToken ct)
    {
        var token = await auth.GetAccessTokenAsync(ct);
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<WoltShipmentPromise> GetShipmentPromiseAsync(WoltShipmentRequest request, CancellationToken ct = default)
    {
        await AuthorizeAsync(ct);
        var body = JsonSerializer.Serialize(new
        {
            pickup = new { coordinates = new { lat = request.PickupLat, lon = request.PickupLon }, address = request.PickupAddress },
            dropoff = new { coordinates = new { lat = request.DropoffLat, lon = request.DropoffLon }, address = request.DropoffAddress }
        }, JsonOpts);

        var response = await http.PostAsync($"{options.Value.DriveBaseUrl}/v1/shipment-promises",
            new StringContent(body, Encoding.UTF8, "application/json"), ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<WoltShipmentPromiseResponse>(json, JsonOpts)!;
        return new WoltShipmentPromise(result.PromiseId, result.Price, result.EtaMinutes);
    }

    public async Task<WoltDelivery> CreateDeliveryAsync(WoltDeliveryRequest request, CancellationToken ct = default)
    {
        await AuthorizeAsync(ct);
        var body = JsonSerializer.Serialize(new
        {
            promise_id = request.PromiseId,
            customer = new { name = request.CustomerName, phone = request.CustomerPhone, email = request.CustomerEmail },
            dropoff = new
            {
                address = request.DropoffAddress,
                coordinates = new { lat = request.DropoffLat, lon = request.DropoffLon }
            },
            items = request.Items.Select(i => new { name = i.Name, count = i.Count, price = (int)(i.Price * 100) }),
            merchant_order_id = request.MerchantOrderId
        }, JsonOpts);

        var response = await http.PostAsync($"{options.Value.DriveBaseUrl}/v1/deliveries",
            new StringContent(body, Encoding.UTF8, "application/json"), ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<WoltDeliveryResponse>(json, JsonOpts)!;
        return new WoltDelivery(result.DeliveryId, result.TrackingUrl, result.Status);
    }

    public async Task<WoltDeliveryStatus> GetDeliveryStatusAsync(string deliveryId, CancellationToken ct = default)
    {
        await AuthorizeAsync(ct);
        var response = await http.GetAsync($"{options.Value.DriveBaseUrl}/v1/orders/{deliveryId}", ct);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<WoltDeliveryResponse>(json, JsonOpts)!;
        return new WoltDeliveryStatus(result.DeliveryId, result.Status, result.TrackingUrl);
    }

    public async Task CancelDeliveryAsync(string deliveryId, CancellationToken ct = default)
    {
        await AuthorizeAsync(ct);
        var response = await http.DeleteAsync($"{options.Value.DriveBaseUrl}/v1/orders/{deliveryId}", ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<WoltVenueList> SearchVenuesAsync(double lat, double lon, string? query = null, CancellationToken ct = default)
    {
        await AuthorizeAsync(ct);
        var url = $"{options.Value.DriveBaseUrl}/v1/pages/restaurants?lat={lat}&lon={lon}";
        if (!string.IsNullOrEmpty(query)) url += $"&q={Uri.EscapeDataString(query)}";

        var response = await http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var doc = JsonDocument.Parse(json);
        var venues = new List<WoltVenueSummary>();

        if (doc.RootElement.TryGetProperty("sections", out var sections))
        {
            foreach (var section in sections.EnumerateArray())
            {
                if (!section.TryGetProperty("items", out var items)) continue;
                foreach (var item in items.EnumerateArray())
                {
                    venues.Add(new WoltVenueSummary(
                        item.GetProperty("venue").GetProperty("id").GetString() ?? "",
                        item.GetProperty("venue").GetProperty("name").EnumerateArray().First().GetProperty("value").GetString() ?? "",
                        item.GetProperty("venue").GetProperty("address").GetString() ?? "",
                        item.GetProperty("venue").GetProperty("location").GetProperty("coordinates")[1].GetDouble(),
                        item.GetProperty("venue").GetProperty("location").GetProperty("coordinates")[0].GetDouble(),
                        item.GetProperty("venue").GetProperty("online").GetBoolean()
                    ));
                }
            }
        }

        return new WoltVenueList(venues);
    }

    public async Task<WoltVenueMenu> GetVenueMenuAsync(string venueId, CancellationToken ct = default)
    {
        await AuthorizeAsync(ct);
        var response = await http.GetAsync($"{options.Value.DriveBaseUrl}/v1/venues/slug/{venueId}/menu", ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var doc = JsonDocument.Parse(json);
        var items = new List<WoltMenuItem>();

        if (doc.RootElement.TryGetProperty("items", out var menuItems))
        {
            foreach (var item in menuItems.EnumerateArray())
            {
                items.Add(new WoltMenuItem(
                    item.GetProperty("id").GetString() ?? "",
                    item.GetProperty("name").EnumerateArray().First().GetProperty("value").GetString() ?? "",
                    item.TryGetProperty("description", out var desc) ? desc.EnumerateArray().FirstOrDefault().GetProperty("value").GetString() ?? "" : "",
                    item.GetProperty("baseprice").GetDecimal() / 100,
                    item.TryGetProperty("image", out var img) ? img.GetProperty("url").GetString() : null
                ));
            }
        }

        return new WoltVenueMenu(venueId, items);
    }

    private record WoltShipmentPromiseResponse(
        [property: JsonPropertyName("promise_id")] string PromiseId,
        [property: JsonPropertyName("price")] decimal Price,
        [property: JsonPropertyName("eta_minutes")] int EtaMinutes);

    private record WoltDeliveryResponse(
        [property: JsonPropertyName("id")] string DeliveryId,
        [property: JsonPropertyName("tracking_url")] string TrackingUrl,
        [property: JsonPropertyName("status")] string Status);
}
