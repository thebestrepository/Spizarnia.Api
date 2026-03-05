using FluentValidation;
using MediatR;
using Spizarnia.Application.Common;
using Spizarnia.Domain.Entities;
using Spizarnia.Domain.Interfaces;

namespace Spizarnia.Application.Orders;

public record CreateOrderCommand(
    Guid UserId,
    string DeliveryAddress, string DeliveryCity, double DeliveryLat, double DeliveryLon,
    List<OrderItemRequest> Items
) : IRequest<Result<OrderDto>>;

public record OrderItemRequest(string ProductId, string Name, int Quantity, decimal Price);

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.DeliveryAddress).NotEmpty();
        RuleFor(x => x.DeliveryCity).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Name).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.Price).GreaterThan(0);
        });
    }
}

public class CreateOrderCommandHandler(
    IOrderRepository orders,
    IUserRepository users,
    IWoltClient wolt
) : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(cmd.UserId, ct);
        if (user is null) return Result<OrderDto>.Failure("User not found.");

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = cmd.UserId,
            DeliveryAddress = cmd.DeliveryAddress,
            DeliveryCity = cmd.DeliveryCity,
            DeliveryLat = cmd.DeliveryLat,
            DeliveryLon = cmd.DeliveryLon,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Items = cmd.Items.Select(i => new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = i.ProductId,
                Name = i.Name,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList(),
            TotalPrice = cmd.Items.Sum(i => i.Price * i.Quantity)
        };

        try
        {
            var delivery = await wolt.CreateDeliveryAsync(new WoltDeliveryRequest(
                PromiseId: "",
                CustomerName: user.Name,
                CustomerPhone: "",
                CustomerEmail: user.Email,
                DropoffAddress: cmd.DeliveryAddress,
                DropoffLat: cmd.DeliveryLat,
                DropoffLon: cmd.DeliveryLon,
                Items: cmd.Items.Select(i => new WoltOrderLineItem(i.Name, i.Quantity, i.Price)).ToList(),
                MerchantOrderId: order.Id.ToString()
            ), ct);

            order.WoltDeliveryId = delivery.DeliveryId;
            order.Status = OrderStatus.Created;
        }
        catch
        {
            order.Status = OrderStatus.Failed;
        }

        await orders.AddAsync(order, ct);
        await orders.SaveChangesAsync(ct);

        return Result<OrderDto>.Success(MapToDto(order));
    }

    internal static OrderDto MapToDto(Order o) => new(
        o.Id, o.UserId, o.WoltDeliveryId, o.Status.ToString(),
        o.DeliveryAddress, o.TotalPrice, o.CreatedAt,
        o.Items.Select(i => new OrderItemDto(i.Id, i.ProductId, i.Name, i.Quantity, i.Price)).ToList());
}
