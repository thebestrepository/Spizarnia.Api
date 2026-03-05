using MediatR;
using Spizarnia.Application.Common;
using Spizarnia.Domain.Entities;
using Spizarnia.Domain.Interfaces;

namespace Spizarnia.Application.Orders;

public record GetOrdersQuery(Guid UserId) : IRequest<Result<List<OrderDto>>>;
public record GetOrderByIdQuery(Guid UserId, Guid OrderId) : IRequest<Result<OrderDto>>;
public record CancelOrderCommand(Guid UserId, Guid OrderId) : IRequest<Result>;

public class GetOrdersQueryHandler(IOrderRepository orders) : IRequestHandler<GetOrdersQuery, Result<List<OrderDto>>>
{
    public async Task<Result<List<OrderDto>>> Handle(GetOrdersQuery q, CancellationToken ct)
    {
        var list = await orders.GetAllForUserAsync(q.UserId, ct);
        return Result<List<OrderDto>>.Success(list.Select(CreateOrderCommandHandler.MapToDto).ToList());
    }
}

public class GetOrderByIdQueryHandler(IOrderRepository orders) : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery q, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(q.OrderId, ct);
        if (order is null || order.UserId != q.UserId) return Result<OrderDto>.Failure("Order not found.");
        return Result<OrderDto>.Success(CreateOrderCommandHandler.MapToDto(order));
    }
}

public class CancelOrderCommandHandler(IOrderRepository orders, IWoltClient wolt) : IRequestHandler<CancelOrderCommand, Result>
{
    public async Task<Result> Handle(CancelOrderCommand cmd, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(cmd.OrderId, ct);
        if (order is null || order.UserId != cmd.UserId) return Result.Failure("Order not found.");
        if (order.Status is OrderStatus.Delivered or OrderStatus.Canceled)
            return Result.Failure("Order cannot be canceled.");

        if (order.WoltDeliveryId is not null)
        {
            try { await wolt.CancelDeliveryAsync(order.WoltDeliveryId, ct); }
            catch { /* already canceled or unavailable */ }
        }

        order.Status = OrderStatus.Canceled;
        order.UpdatedAt = DateTime.UtcNow;
        await orders.UpdateAsync(order, ct);
        await orders.SaveChangesAsync(ct);
        return Result.Success();
    }
}
