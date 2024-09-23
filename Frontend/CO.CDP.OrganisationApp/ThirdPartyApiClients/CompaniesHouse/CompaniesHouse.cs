using Flurl;
using Flurl.Http;

namespace CO.CDP.OrganisationApp.ThirdPartyApiClients.CompaniesHouse;

public class CompaniesHouseApi(IConfiguration configuration, ILogger<CompaniesHouseApi> logger) : ICompaniesHouseApi
{
    public async Task<RegisteredAddress?> GetRegisteredAddress(
        string companyNumber)
    {
        var companiesHouseUrl = GetCompaniesHouseUrl();
        var userName = GetComapniesHouseUser();
        var password = GetComapniesHousePassword();
        RegisteredAddress? companyRegDetails = null;

        try
        {
            companyRegDetails = await $"{companiesHouseUrl}"
                .AppendPathSegment($"/company/{companyNumber}/registered-office-address")
                .WithBasicAuth(userName, password)
                .GetAsync()
                .ReceiveJson<RegisteredAddress>();
        }
        catch (FlurlHttpException exc)
        {
            logger.LogError(exc, "Failed during call to registered office address.");
        }

        return companyRegDetails;
    }

    public async Task<CompanyProfile?> GetProfile(
        string companyNumber)
    {
        var companiesHouseUrl = GetCompaniesHouseUrl();
        var userName = GetComapniesHouseUser();
        var password = GetComapniesHousePassword();
        CompanyProfile? profile = null;

        try
        {
            profile = await $"{companiesHouseUrl}"
                .AppendPathSegment($"/company/{companyNumber}")
                .WithBasicAuth(userName, password)
                .GetAsync()
                .ReceiveJson<CompanyProfile>();
        }
        catch (FlurlHttpException exc)
        {
            logger.LogError(exc, "Failed during call to get company profile.");
        }

        return profile;
    }

    private string? GetCompaniesHouseUrl()
    {
        return configuration["CompaniesHouse:Url"];
    }

    private string? GetComapniesHouseUser()
    {
        return configuration["CompaniesHouse:User"];
    }

    private string? GetComapniesHousePassword()
    {
        return configuration["CompaniesHouse:Password"];
    }
}
