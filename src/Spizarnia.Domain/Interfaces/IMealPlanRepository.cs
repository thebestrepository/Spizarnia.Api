using Spizarnia.Domain.Entities;

namespace Spizarnia.Domain.Interfaces;

public interface IMealPlanRepository
{
    Task<MealPlan?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<MealPlan?> GetCurrentForUserAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<MealPlan>> GetAllForUserAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(MealPlan plan, CancellationToken ct = default);
    Task UpdateAsync(MealPlan plan, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
