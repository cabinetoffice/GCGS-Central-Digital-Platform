using System.Collections.Generic;

namespace CO.CDP.OrganisationApp.Constants;

public static class OrganisationSchemeType
{
    public const string CompaniesHouse = "GB-COH";
    public const string CharityCommissionEnglandWales = "GB-CHC";
    public const string ScottishCharityRegulator = "GB-SC";
    public const string CharityCommissionNorthernIreland = "GB-NIC";
    public const string MutualsPublicRegister = "GB-MPR";
    public const string GuernseyRegistry = "GG-RCE";
    public const string JerseyFinancialServicesCommission = "JE-FSC";
    public const string IsleOfManCompaniesRegistry = "IM-CR";
    public const string NHSOrganisationsRegistry = "GB-NHS";
    public const string UKRegisterOfLearningProviders = "GB-UKPRN";
    public const string VAT = "VAT";
    public const string Other = "Other";
    public const string Ppon = "GB-PPON";

    private static readonly Dictionary<string, string> OrganisationScheme = new()
    {
        { CompaniesHouse, "Companies House Number" },
        { CharityCommissionEnglandWales, "Charity Commission for England & Wales Number" },
        { ScottishCharityRegulator, "Scottish Charity Regulator" },
        { CharityCommissionNorthernIreland, "Charity Commission for Northern Ireland Number" },
        { MutualsPublicRegister, "Mutuals Public Register Number" },
        { GuernseyRegistry, "Guernsey Registry Number" },
        { JerseyFinancialServicesCommission, "Jersey Financial Services Commission Registry Number" },
        { IsleOfManCompaniesRegistry, "Isle of Man Companies Registry Number" },
        { NHSOrganisationsRegistry, "National Health Service Organisations Registry Number" },
        { UKRegisterOfLearningProviders, "UK Register of Learning Provider Number" },
        { VAT, "VAT number" },
        { Other, "Other / None" },
        { Ppon, "Ppon" }
    };

    public static string? SchemeDescription(this string? scheme)
    {
        if (string.IsNullOrWhiteSpace(scheme))
            return null;

        OrganisationScheme.TryGetValue(scheme, out var value);
        return value;
    }
}
