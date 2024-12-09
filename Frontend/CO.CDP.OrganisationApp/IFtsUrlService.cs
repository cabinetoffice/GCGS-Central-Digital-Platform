namespace CO.CDP.OrganisationApp;

public interface IFtsUrlService
{
    string BuildUrl(string endpoint, Guid? organisationId = null, string? redirectUrl = null);
}