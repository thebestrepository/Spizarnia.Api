namespace Spizarnia.Domain.Interfaces;

public interface IWoltClient
{
    Task<WoltShipmentPromise> GetShipmentPromiseAsync(WoltShipmentRequest request, CancellationToken ct = default);
    Task<WoltDelivery> CreateDeliveryAsync(WoltDeliveryRequest request, CancellationToken ct = default);
    Task<WoltDeliveryStatus> GetDeliveryStatusAsync(string deliveryId, CancellationToken ct = default);
    Task CancelDeliveryAsync(string deliveryId, CancellationToken ct = default);
    Task<WoltVenueList> SearchVenuesAsync(double lat, double lon, string? query = null, CancellationToken ct = default);
    Task<WoltVenueMenu> GetVenueMenuAsync(string venueId, CancellationToken ct = default);
}

public record WoltShipmentRequest(
    double PickupLat, double PickupLon, string PickupAddress,
    double DropoffLat, double DropoffLon, string DropoffAddress);

public record WoltShipmentPromise(string PromiseId, decimal Price, int EtaMinutes);

public record WoltDeliveryRequest(
    string PromiseId,
    string CustomerName, string CustomerPhone, string CustomerEmail,
    string DropoffAddress, double DropoffLat, double DropoffLon,
    List<WoltOrderLineItem> Items, string MerchantOrderId);

public record WoltOrderLineItem(string Name, int Count, decimal Price);

public record WoltDelivery(string DeliveryId, string TrackingUrl, string Status);

public record WoltDeliveryStatus(string DeliveryId, string Status, string? TrackingUrl);

public record WoltVenueList(List<WoltVenueSummary> Venues);

public record WoltVenueSummary(string Id, string Name, string Address, double Lat, double Lon, bool IsOnline);

public record WoltVenueMenu(string VenueId, List<WoltMenuItem> Items);

public record WoltMenuItem(string Id, string Name, string Description, decimal Price, string? ImageUrl);
