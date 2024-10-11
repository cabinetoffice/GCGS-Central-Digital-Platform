namespace CO.CDP.Authentication;
public interface IClaimService
{
    string? GetUserUrn();

    Guid? GetOrganisationId();

    Task<bool> HaveAccessToOrganisation(Guid organisationId, string[] scopes, string[]? personScopes = null);
}