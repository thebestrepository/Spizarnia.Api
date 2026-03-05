using MediatR;
using Spizarnia.Application.Common;
using Spizarnia.Domain.Entities;
using Spizarnia.Domain.Interfaces;

namespace Spizarnia.Application.MealPlans;

public record GenerateMealPlanCommand(Guid UserId) : IRequest<Result<MealPlanDto>>;

public class GenerateMealPlanCommandHandler(
    IUserRepository users,
    IMealPlanRepository mealPlans
) : IRequestHandler<GenerateMealPlanCommand, Result<MealPlanDto>>
{
    public async Task<Result<MealPlanDto>> Handle(GenerateMealPlanCommand cmd, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(cmd.UserId, ct);
        if (user is null) return Result<MealPlanDto>.Failure("User not found.");

        var monday = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek + 1);
        var plan = new MealPlan
        {
            Id = Guid.NewGuid(),
            UserId = cmd.UserId,
            WeekStartDate = monday,
            CreatedAt = DateTime.UtcNow,
            Meals = GenerateSampleMeals()
        };

        await mealPlans.AddAsync(plan, ct);
        await mealPlans.SaveChangesAsync(ct);

        return Result<MealPlanDto>.Success(MapToDto(plan));
    }

    // Placeholder: replace with actual Claude AI call for meal generation
    private static List<Meal> GenerateSampleMeals()
    {
        var meals = new List<Meal>();
        var days = Enum.GetValues<DayOfWeek>().Where(d => d != DayOfWeek.Saturday && d != DayOfWeek.Sunday);
        foreach (var day in days)
        {
            meals.Add(new Meal { Id = Guid.NewGuid(), Day = day, Type = MealType.Breakfast, Name = "Owsianka z owocami", Ingredients = ["płatki owsiane", "mleko", "banan", "borówki"], CaloriesEstimate = 380 });
            meals.Add(new Meal { Id = Guid.NewGuid(), Day = day, Type = MealType.Lunch, Name = "Kurczak z ryżem i warzywami", Ingredients = ["pierś z kurczaka", "ryż", "brokuły", "marchew"], CaloriesEstimate = 520 });
            meals.Add(new Meal { Id = Guid.NewGuid(), Day = day, Type = MealType.Dinner, Name = "Zupa pomidorowa", Ingredients = ["pomidory", "makaron", "bazylia", "oliwa"], CaloriesEstimate = 320 });
        }
        return meals;
    }

    internal static MealPlanDto MapToDto(MealPlan plan) => new(
        plan.Id, plan.UserId, plan.WeekStartDate, plan.CreatedAt,
        plan.Meals.Select(m => new MealDto(m.Id, m.Day.ToString(), m.Type.ToString(), m.Name, m.Description, m.Ingredients, m.CaloriesEstimate)).ToList());
}
