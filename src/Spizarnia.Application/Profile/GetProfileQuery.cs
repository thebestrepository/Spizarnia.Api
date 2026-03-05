using MediatR;
using Spizarnia.Application.Common;
using Spizarnia.Domain.Interfaces;

namespace Spizarnia.Application.Profile;

public record GetProfileQuery(Guid UserId) : IRequest<Result<ProfileDto>>;

public class GetProfileQueryHandler(IUserRepository users) : IRequestHandler<GetProfileQuery, Result<ProfileDto>>
{
    public async Task<Result<ProfileDto>> Handle(GetProfileQuery query, CancellationToken ct)
    {
        var user = await users.GetByIdAsync(query.UserId, ct);
        if (user is null) return Result<ProfileDto>.Failure("User not found.");

        var profile = user.FamilyProfile;
        return Result<ProfileDto>.Success(new ProfileDto(
            user.Id, user.Email, user.Name, user.Language,
            profile is null ? null : new FamilyProfileDto(
                profile.Id,
                profile.Members.Select(m => new FamilyMemberDto(m.Name, m.Age, m.Role)).ToList(),
                profile.DietaryRestrictions,
                profile.Allergies,
                profile.Goals)));
    }
}
