using CO.CDP.EntityVerificationClient;
using System.Collections.Generic;
using CO.CDP.Localization;

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
        { CompaniesHouse, StaticTextResource.OrganisationSchemeType_CompaniesHouse },
        { CharityCommissionEnglandWales, StaticTextResource.OrganisationSchemeType_CharityCommissionEnglandWales },
        { ScottishCharityRegulator, StaticTextResource.OrganisationSchemeType_ScottishCharityRegulator },
        { CharityCommissionNorthernIreland, StaticTextResource.OrganisationSchemeType_CharityCommissionNorthernIreland },
        { MutualsPublicRegister, StaticTextResource.OrganisationSchemeType_MutualsPublicRegister },
        { GuernseyRegistry, StaticTextResource.OrganisationSchemeType_GuernseyRegistry },
        { JerseyFinancialServicesCommission, StaticTextResource.OrganisationSchemeType_JerseyFinancialServicesCommission },
        { IsleOfManCompaniesRegistry, StaticTextResource.OrganisationSchemeType_IsleOfManCompaniesRegistry },
        { NHSOrganisationsRegistry, StaticTextResource.OrganisationSchemeType_NHSOrganisationsRegistry },
        { UKRegisterOfLearningProviders, StaticTextResource.OrganisationSchemeType_UKRegisterOfLearningProviders },
        { VAT, StaticTextResource.OrganisationSchemeType_VAT },
        { Other, StaticTextResource.OrganisationSchemeType_OtherNone },
        { Ppon, StaticTextResource.OrganisationSchemeType_Ppon }
    };

    public static string? SchemeDescription(this string? scheme, ICollection<IdentifierRegistries>? registriesDetails = null)
    {
        if (string.IsNullOrWhiteSpace(scheme))
            return null;

        if (OrganisationScheme.TryGetValue(scheme, out var value) && !string.IsNullOrEmpty(value))
        {
            return value;
        }

        if (registriesDetails != null)
        {
            var registryMatch = registriesDetails.FirstOrDefault(r => r.Scheme?.Equals(scheme, StringComparison.OrdinalIgnoreCase) == true);
            if (registryMatch != null)
            {

                if (!string.IsNullOrWhiteSpace(registryMatch.Countrycode) && Constants.Country.NonUKCountries.TryGetValue(registryMatch.Countrycode, out var countryName))
                {
                    return $"{countryName} - {registryMatch.RegisterName}";
                }

                return $"{registryMatch.RegisterName}";
            }
            var schemeCountryCode = scheme.Contains("-") ? scheme.Split('-')[0] : scheme;
            if (!string.IsNullOrWhiteSpace(schemeCountryCode) && Constants.Country.NonUKCountries.TryGetValue(schemeCountryCode, out var countryNameforOthers))
            {
                return $"{countryNameforOthers} - {StaticTextResource.OrganisationSchemeType_Other}";
            }
        }

        return StaticTextResource.OrganisationSchemeType_Other;
    }
}
