namespace CO.CDP.DataSharing.WebApi;

public interface IIdentifierSchemes
{
    IDictionary<string, string> SchemesToEndpointUris { get; }

    Uri? GetRegistryUri(string? scheme, string? identifierId);
}