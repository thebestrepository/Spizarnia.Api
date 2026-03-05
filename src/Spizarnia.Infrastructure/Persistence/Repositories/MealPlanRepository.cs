using Microsoft.EntityFrameworkCore;
using Spizarnia.Domain.Entities;
using Spizarnia.Domain.Interfaces;

namespace Spizarnia.Infrastructure.Persistence.Repositories;

public class MealPlanRepository(AppDbContext db) : IMealPlanRepository
{
    public Task<MealPlan?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.MealPlans.Include(m => m.Meals).FirstOrDefaultAsync(m => m.Id == id, ct);

    public Task<MealPlan?> GetCurrentForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var monday = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek + 1);
        return db.MealPlans.Include(m => m.Meals)
            .Where(m => m.UserId == userId && m.WeekStartDate == monday)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<MealPlan>> GetAllForUserAsync(Guid userId, CancellationToken ct = default) =>
        await db.MealPlans.Include(m => m.Meals)
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.WeekStartDate)
            .ToListAsync(ct);

    public async Task AddAsync(MealPlan plan, CancellationToken ct = default) =>
        await db.MealPlans.AddAsync(plan, ct);

    public Task UpdateAsync(MealPlan plan, CancellationToken ct = default)
    {
        db.MealPlans.Update(plan);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}
