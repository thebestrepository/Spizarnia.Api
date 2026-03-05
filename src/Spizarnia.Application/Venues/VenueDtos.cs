namespace Spizarnia.Application.Venues;

public record VenueSummaryDto(string Id, string Name, string Address, double Lat, double Lon, bool IsOnline);
public record VenueMenuDto(string VenueId, List<MenuItemDto> Items);
public record MenuItemDto(string Id, string Name, string Description, decimal Price, string? ImageUrl);
