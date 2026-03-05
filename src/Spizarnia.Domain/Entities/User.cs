namespace Spizarnia.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Language { get; set; } = "pl";
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public FamilyProfile? FamilyProfile { get; set; }
    public ICollection<MealPlan> MealPlans { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
}
