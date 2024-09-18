namespace CO.CDP.OrganisationApp;

public static class Helper
{
    const string VatSchemeName = "VAT";

    public static bool ValidRelativeUri(string? redirectUri)
        => !string.IsNullOrWhiteSpace(redirectUri) && Uri.TryCreate(redirectUri, UriKind.Relative, out var _);

    public static Organisation.WebApiClient.Identifier? GetVatIdentifier(Organisation.WebApiClient.Organisation organisation)
    {
        var vatIdentifier = organisation.AdditionalIdentifiers.FirstOrDefault(i => i.Scheme == VatSchemeName) ??
            ((organisation.Identifier.Scheme == VatSchemeName) ? organisation.Identifier : null);
        return vatIdentifier;
    }
}