using CO.CDP.OrganisationApp.Logging;
using Flurl;
using Flurl.Http;
using System.Net;

namespace CO.CDP.OrganisationApp.ThirdPartyApiClients.CompaniesHouse;

public class CompaniesHouseApi(IConfiguration configuration,
    ILogger<CompaniesHouseApi> logger) : ICompaniesHouseApi
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
                .AppendPathSegment($"/company/{companyNumber.Trim()}/registered-office-address")
                .WithBasicAuth(userName, password)
                .GetAsync()
                .ReceiveJson<RegisteredAddress>();
        }
        catch (Exception exc)
        {
            Log(exc, companyNumber);
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
                .AppendPathSegment($"/company/{companyNumber.Trim()}")
                .WithBasicAuth(userName, password)
                .GetAsync()
                .ReceiveJson<CompanyProfile>();
        }
        catch (Exception exc)
        {
            Log(exc, companyNumber);
        }

        return profile;
    }

    private void Log(Exception exc, string companyNumber)
    {
        var logException = new CdpExceptionLogging($"Failed to call Companies House API for company number: {companyNumber}.", CdpErrorCodes.CompaniesHouseApiError, exc);

        logger.LogError(logException, "Failed to call Companies House API.");
    }

    private string GetCompaniesHouseUrl()
    {
        return configuration["CompaniesHouse:Url"] ?? "";
    }

    private string GetComapniesHouseUser()
    {
        return configuration["CompaniesHouse:User"] ?? "";
    }

    private string GetComapniesHousePassword()
    {
        return configuration["CompaniesHouse:Password"] ?? "";
    }
}
