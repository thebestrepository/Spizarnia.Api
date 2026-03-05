namespace Spizarnia.Domain.Entities;

public class FamilyProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<FamilyMember> Members { get; set; } = [];
    public List<string> DietaryRestrictions { get; set; } = [];
    public List<string> Allergies { get; set; } = [];
    public List<string> Goals { get; set; } = [];

    public User User { get; set; } = null!;
}

public class FamilyMember
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Role { get; set; } = string.Empty; // adult, child
}
