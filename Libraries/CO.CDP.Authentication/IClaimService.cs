namespace CO.CDP.Authentication;
public interface IClaimService
{
    string? GetUserUrn();

    int? GetOrganisationId();

    bool HaveAccessToOrganisation(Guid oragnisationId);
}