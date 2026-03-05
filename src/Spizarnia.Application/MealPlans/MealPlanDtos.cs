namespace Spizarnia.Application.MealPlans;

public record MealPlanDto(
    Guid Id, Guid UserId, DateTime WeekStartDate, DateTime CreatedAt,
    List<MealDto> Meals);

public record MealDto(
    Guid Id, string Day, string Type, string Name, string Description,
    List<string> Ingredients, int CaloriesEstimate);
