using MediatR;
using Spizarnia.Application.Common;
using Spizarnia.Domain.Interfaces;

namespace Spizarnia.Application.MealPlans;

public record GetCurrentMealPlanQuery(Guid UserId) : IRequest<Result<MealPlanDto>>;
public record GetMealPlanByIdQuery(Guid UserId, Guid PlanId) : IRequest<Result<MealPlanDto>>;
public record UpdateMealPlanCommand(Guid UserId, Guid PlanId, List<MealDto> Meals) : IRequest<Result<MealPlanDto>>;

public class GetCurrentMealPlanQueryHandler(IMealPlanRepository plans) : IRequestHandler<GetCurrentMealPlanQuery, Result<MealPlanDto>>
{
    public async Task<Result<MealPlanDto>> Handle(GetCurrentMealPlanQuery q, CancellationToken ct)
    {
        var plan = await plans.GetCurrentForUserAsync(q.UserId, ct);
        if (plan is null) return Result<MealPlanDto>.Failure("No meal plan for current week.");
        return Result<MealPlanDto>.Success(GenerateMealPlanCommandHandler.MapToDto(plan));
    }
}

public class GetMealPlanByIdQueryHandler(IMealPlanRepository plans) : IRequestHandler<GetMealPlanByIdQuery, Result<MealPlanDto>>
{
    public async Task<Result<MealPlanDto>> Handle(GetMealPlanByIdQuery q, CancellationToken ct)
    {
        var plan = await plans.GetByIdAsync(q.PlanId, ct);
        if (plan is null || plan.UserId != q.UserId) return Result<MealPlanDto>.Failure("Meal plan not found.");
        return Result<MealPlanDto>.Success(GenerateMealPlanCommandHandler.MapToDto(plan));
    }
}

public class UpdateMealPlanCommandHandler(IMealPlanRepository plans) : IRequestHandler<UpdateMealPlanCommand, Result<MealPlanDto>>
{
    public async Task<Result<MealPlanDto>> Handle(UpdateMealPlanCommand cmd, CancellationToken ct)
    {
        var plan = await plans.GetByIdAsync(cmd.PlanId, ct);
        if (plan is null || plan.UserId != cmd.UserId) return Result<MealPlanDto>.Failure("Meal plan not found.");

        plan.Meals.Clear();
        foreach (var m in cmd.Meals)
        {
            plan.Meals.Add(new Domain.Entities.Meal
            {
                Id = m.Id == Guid.Empty ? Guid.NewGuid() : m.Id,
                MealPlanId = plan.Id,
                Day = Enum.Parse<DayOfWeek>(m.Day),
                Type = Enum.Parse<Domain.Entities.MealType>(m.Type),
                Name = m.Name,
                Description = m.Description,
                Ingredients = m.Ingredients,
                CaloriesEstimate = m.CaloriesEstimate
            });
        }

        await plans.UpdateAsync(plan, ct);
        await plans.SaveChangesAsync(ct);
        return Result<MealPlanDto>.Success(GenerateMealPlanCommandHandler.MapToDto(plan));
    }
}
