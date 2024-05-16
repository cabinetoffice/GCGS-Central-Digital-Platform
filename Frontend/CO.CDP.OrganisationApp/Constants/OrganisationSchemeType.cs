namespace CO.CDP.OrganisationApp.Constants;

public static class OrganisationSchemeType
{
    public static Dictionary<string, string> OrganisationScheme => new()
        {
            { "CHN", "Companies House Number"},
            { "CCEW", "Charity Commission for England & Wales Number"},
            { "SCR", "Scottish Charity Regulator"},
            { "CCNI", "Charity Commission for Northren Ireland Number"},
            { "MPR", "Mutuals Public Register Number"},
            { "GRN", "Guernsey Registry Number"},
            { "JFSC", "Jersey Financial Services Commission Registry Number"},
            { "IMCR", "Isle of Man Companies Registry Number"},
            { "NHOR", "National health Service Organisations Registry Number"},
            { "UKPRN", "UK Register of Learning Provider Number"},
            { "VAT", "VAT number"},
            { "Other", "Other / None"}
        };
}