using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Spizarnia.Domain.Entities;
using System.Text.Json;

namespace Spizarnia.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<FamilyProfile> FamilyProfiles => Set<FamilyProfile>();
    public DbSet<MealPlan> MealPlans => Set<MealPlan>();
    public DbSet<Meal> Meals => Set<Meal>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        base.OnModelCreating(model);

        model.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.HasOne(u => u.FamilyProfile).WithOne(f => f.User).HasForeignKey<FamilyProfile>(f => f.UserId);
            e.HasMany(u => u.MealPlans).WithOne(m => m.User).HasForeignKey(m => m.UserId);
            e.HasMany(u => u.Orders).WithOne(o => o.User).HasForeignKey(o => o.UserId);
        });

        var listComparer = new ValueComparer<List<string>>(
            (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b)),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        var memberComparer = new ValueComparer<List<FamilyMember>>(
            (a, b) => JsonSerializer.Serialize(a, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(b, (JsonSerializerOptions?)null),
            c => JsonSerializer.Serialize(c, (JsonSerializerOptions?)null).GetHashCode(),
            c => JsonSerializer.Deserialize<List<FamilyMember>>(JsonSerializer.Serialize(c, (JsonSerializerOptions?)null), (JsonSerializerOptions?)null)!);

        model.Entity<FamilyProfile>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.Members).HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<FamilyMember>>(v, (JsonSerializerOptions?)null) ?? new()).Metadata.SetValueComparer(memberComparer);
            e.Property(f => f.DietaryRestrictions).HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new()).Metadata.SetValueComparer(listComparer);
            e.Property(f => f.Allergies).HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new()).Metadata.SetValueComparer(listComparer);
            e.Property(f => f.Goals).HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new()).Metadata.SetValueComparer(listComparer);
        });

        model.Entity<MealPlan>(e =>
        {
            e.HasKey(m => m.Id);
            e.HasMany(m => m.Meals).WithOne(m => m.MealPlan).HasForeignKey(m => m.MealPlanId);
        });

        model.Entity<Meal>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Ingredients).HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new()).Metadata.SetValueComparer(listComparer);
        });

        model.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);
            e.HasMany(o => o.Items).WithOne(i => i.Order).HasForeignKey(i => i.OrderId);
        });

        model.Entity<OrderItem>(e => e.HasKey(i => i.Id));
    }
}
