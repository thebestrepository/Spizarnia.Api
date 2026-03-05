using Spizarnia.Domain.Entities;

namespace Spizarnia.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Order?> GetByWoltDeliveryIdAsync(string deliveryId, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetAllForUserAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    Task UpdateAsync(Order order, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
