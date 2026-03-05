namespace Spizarnia.Domain.Entities;

public class MealPlan
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime WeekStartDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<Meal> Meals { get; set; } = [];
}

public class Meal
{
    public Guid Id { get; set; }
    public Guid MealPlanId { get; set; }
    public DayOfWeek Day { get; set; }
    public MealType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Ingredients { get; set; } = [];
    public int CaloriesEstimate { get; set; }

    public MealPlan MealPlan { get; set; } = null!;
}
