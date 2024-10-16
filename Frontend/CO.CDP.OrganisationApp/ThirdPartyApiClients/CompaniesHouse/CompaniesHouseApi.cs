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
        catch (FlurlHttpException ex) when ((ex.StatusCode == (int)HttpStatusCode.NotFound) || (ex.StatusCode == (int)HttpStatusCode.InternalServerError))
        {
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
        catch (FlurlHttpException ex) when ((ex.StatusCode == (int)HttpStatusCode.NotFound) || (ex.StatusCode == (int)HttpStatusCode.InternalServerError))
        {
        }
        catch (Exception exc)
        {
            Log(exc, companyNumber);
        }

        return profile;
    }

    private void Log(Exception exc, string companyNumber)
    {
        var logException = new ExceptionLogging($"Failed to call Companies House API for company number: {companyNumber}.", ErrorCodes.CompaniesHouseApiError, exc);

        logger.LogError(logException, "Failed to call Companies House API for company number: {companyNumber}.", companyNumber);
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
