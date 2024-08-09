namespace CO.CDP.OrganisationApp.Constants;

public static class OrganisationSchemeType
{
    public static Dictionary<string, string> OrganisationScheme => new()
        {
            { "GB-COH", "Companies House Number"},
            { "GB-CHC", "Charity Commission for England & Wales Number"},
            { "GB-SC", "Scottish Charity Regulator"},
            { "GB-NIC", "Charity Commission for Northren Ireland Number"},
            { "GB-MPR", "Mutuals Public Register Number"},
            { "GG-RCE", "Guernsey Registry Number"},
            { "JE-FSC", "Jersey Financial Services Commission Registry Number"},
            { "IM-CR", "Isle of Man Companies Registry Number"},
            { "GB-NHS", "National health Service Organisations Registry Number"},
            { "GB-UKPRN", "UK Register of Learning Provider Number"},
            { "VAT", "VAT number"},
            { "Other", "Other / None"},
            { "CDP-PPON", "Ppon" }
        };

    public static string? SchemeDescription(this string? scheme)
    {
        return !string.IsNullOrWhiteSpace(scheme) && OrganisationScheme.TryGetValue(scheme, out string? value) ? value : null;
    }
}