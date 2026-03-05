namespace Spizarnia.Application.Orders;

public record OrderDto(
    Guid Id, Guid UserId, string? WoltDeliveryId, string Status,
    string DeliveryAddress, decimal TotalPrice, DateTime CreatedAt,
    List<OrderItemDto> Items);

public record OrderItemDto(Guid Id, string ProductId, string Name, int Quantity, decimal Price);
