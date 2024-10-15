namespace CO.CDP.Authentication;
public interface IClaimService
{
    string? GetUserUrn();

    IEnumerable<string> GetUserRoles();

    Guid? GetOrganisationId();

    Task<bool> HaveAccessToOrganisation(Guid organisationId, string[] scopes, string[]? personScopes = null);
}