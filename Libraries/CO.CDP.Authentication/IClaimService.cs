namespace CO.CDP.Authentication;
public interface IClaimService
{
    string? GetUserUrn();

    string? GetOrganisationId();
}