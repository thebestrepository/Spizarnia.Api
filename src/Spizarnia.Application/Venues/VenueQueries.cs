using MediatR;
using Spizarnia.Application.Common;
using Spizarnia.Domain.Interfaces;

namespace Spizarnia.Application.Venues;

public record SearchVenuesQuery(double Lat, double Lon, string? Query) : IRequest<Result<List<VenueSummaryDto>>>;
public record GetVenueMenuQuery(string VenueId) : IRequest<Result<VenueMenuDto>>;

public class SearchVenuesQueryHandler(IWoltClient wolt) : IRequestHandler<SearchVenuesQuery, Result<List<VenueSummaryDto>>>
{
    public async Task<Result<List<VenueSummaryDto>>> Handle(SearchVenuesQuery q, CancellationToken ct)
    {
        var result = await wolt.SearchVenuesAsync(q.Lat, q.Lon, q.Query, ct);
        return Result<List<VenueSummaryDto>>.Success(
            result.Venues.Select(v => new VenueSummaryDto(v.Id, v.Name, v.Address, v.Lat, v.Lon, v.IsOnline)).ToList());
    }
}

public class GetVenueMenuQueryHandler(IWoltClient wolt) : IRequestHandler<GetVenueMenuQuery, Result<VenueMenuDto>>
{
    public async Task<Result<VenueMenuDto>> Handle(GetVenueMenuQuery q, CancellationToken ct)
    {
        var result = await wolt.GetVenueMenuAsync(q.VenueId, ct);
        return Result<VenueMenuDto>.Success(new VenueMenuDto(
            result.VenueId,
            result.Items.Select(i => new MenuItemDto(i.Id, i.Name, i.Description, i.Price, i.ImageUrl)).ToList()));
    }
}
