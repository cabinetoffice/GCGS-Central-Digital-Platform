namespace CO.CDP.Organisation.WebApi;

public interface IIdentifierService
{
    string? GetRegistryUri(string scheme, string? identifierId);
}