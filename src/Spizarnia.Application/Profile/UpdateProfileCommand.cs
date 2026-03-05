using MediatR;
using Spizarnia.Application.Common;
using Spizarnia.Domain.Entities;
using Spizarnia.Domain.Interfaces;

namespace Spizarnia.Application.Profile;

public record UpdateProfileCommand(
    Guid UserId, string? Name, string? Language,
    List<string>? DietaryRestrictions, List<string>? Allergies, List<string>? Goals
) : IRequest<Result<ProfileDto>>;

public record UpdateFamilyCommand(
    Guid UserId, List<FamilyMemberDto> Members
) : IRequest<Result<ProfileDto>>;

public class UpdateProfileCommandHandler(IUserRepository users) : IRequestHandler<UpdateProfileCommand, Result<ProfileDto>>
{
    public async Task<Result<ProfileDto>> Handle(UpdateProfileCommand cmd, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(cmd.UserId, ct);
        if (user is null) return Result<ProfileDto>.Failure("User not found.");

        if (cmd.Name is not null) user.Name = cmd.Name;
        if (cmd.Language is not null) user.Language = cmd.Language;

        if (user.FamilyProfile is null)
        {
            user.FamilyProfile = new Domain.Entities.FamilyProfile { Id = Guid.NewGuid(), UserId = user.Id };
        }

        if (cmd.DietaryRestrictions is not null) user.FamilyProfile.DietaryRestrictions = cmd.DietaryRestrictions;
        if (cmd.Allergies is not null) user.FamilyProfile.Allergies = cmd.Allergies;
        if (cmd.Goals is not null) user.FamilyProfile.Goals = cmd.Goals;

        await users.UpdateAsync(user, ct);
        await users.SaveChangesAsync(ct);

        var profile = user.FamilyProfile;
        return Result<ProfileDto>.Success(new ProfileDto(
            user.Id, user.Email, user.Name, user.Language,
            new FamilyProfileDto(
                profile.Id,
                profile.Members.Select(m => new FamilyMemberDto(m.Name, m.Age, m.Role)).ToList(),
                profile.DietaryRestrictions,
                profile.Allergies,
                profile.Goals)));
    }
}

public class UpdateFamilyCommandHandler(IUserRepository users) : IRequestHandler<UpdateFamilyCommand, Result<ProfileDto>>
{
    public async Task<Result<ProfileDto>> Handle(UpdateFamilyCommand cmd, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(cmd.UserId, ct);
        if (user is null) return Result<ProfileDto>.Failure("User not found.");

        if (user.FamilyProfile is null)
            user.FamilyProfile = new Domain.Entities.FamilyProfile { Id = Guid.NewGuid(), UserId = user.Id };

        user.FamilyProfile.Members = cmd.Members.Select(m => new FamilyMember
        {
            Name = m.Name, Age = m.Age, Role = m.Role
        }).ToList();

        await users.UpdateAsync(user, ct);
        await users.SaveChangesAsync(ct);

        var profile = user.FamilyProfile;
        return Result<ProfileDto>.Success(new ProfileDto(
            user.Id, user.Email, user.Name, user.Language,
            new FamilyProfileDto(
                profile.Id,
                profile.Members.Select(m => new FamilyMemberDto(m.Name, m.Age, m.Role)).ToList(),
                profile.DietaryRestrictions,
                profile.Allergies,
                profile.Goals)));
    }
}
