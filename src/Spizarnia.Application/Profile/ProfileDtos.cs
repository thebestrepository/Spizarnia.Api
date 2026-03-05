namespace Spizarnia.Application.Profile;

public record ProfileDto(
    Guid UserId, string Email, string Name, string Language,
    FamilyProfileDto? FamilyProfile);

public record FamilyProfileDto(
    Guid Id,
    List<FamilyMemberDto> Members,
    List<string> DietaryRestrictions,
    List<string> Allergies,
    List<string> Goals);

public record FamilyMemberDto(string Name, int Age, string Role);
