using Flurl;
using Flurl.Http;

using System.Net;

namespace CO.CDP.OrganisationApp.ThirdPartyApiClients.CompaniesHouse;

public class CompaniesHouseApi(IConfiguration configuration,
    ILogger<CompaniesHouseApi> logger) : ICompaniesHouseApi
{
    public async Task<(RegisteredAddress?, HttpStatusCode)> GetRegisteredAddress(
        string companyNumber)
    {
        var companiesHouseUrl = GetCompaniesHouseUrl();
        var userName = GetComapniesHouseUser();
        var password = GetComapniesHousePassword();
        RegisteredAddress? companyRegDetails = null;
        HttpStatusCode httpStatus = HttpStatusCode.OK;

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
            httpStatus = Enum.Parse<HttpStatusCode>(ex.StatusCode?.ToString() ?? HttpStatusCode.InternalServerError.ToString());
        }
        catch (Exception exc)
        {
            httpStatus = HttpStatusCode.ServiceUnavailable;
            logger.LogError(exc, "Failed during call to registered office address.");
        }

        return (companyRegDetails, httpStatus);
    }

    public async Task<(CompanyProfile?, HttpStatusCode)> GetProfile(
        string companyNumber)
    {
        var companiesHouseUrl = GetCompaniesHouseUrl();
        var userName = GetComapniesHouseUser();
        var password = GetComapniesHousePassword();
        CompanyProfile? profile = null;
        HttpStatusCode httpStatus = HttpStatusCode.OK;

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
            httpStatus = Enum.Parse<HttpStatusCode>(ex.StatusCode?.ToString() ?? HttpStatusCode.InternalServerError.ToString());
        }
        catch (Exception exc)
        {
            httpStatus = HttpStatusCode.ServiceUnavailable;
            logger.LogError(exc, "Failed during call to get company profile.");
        }

        return (profile, httpStatus);
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
