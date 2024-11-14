using CO.CDP.OrganisationApp.CharityCommission;
using CO.CDP.OrganisationApp.Logging;
using Flurl;
using Flurl.Http;
using System.Net;

namespace CO.CDP.OrganisationApp.ThirdPartyApiClients.CharityCommission;

public class CharityCommissionApi(IConfiguration configuration,
    ILogger<CharityCommissionApi> logger) : ICharityCommissionApi
{
    public async Task<CharityDetails?> GetCharityDetails(
        string registeredNumber)
    {
        var charityCommissionUrl = GetCharityCommissionUrl();
        var subscriptionKey = GetCharityCommissionSubscriptionKey();
        CharityDetails? charityRegDetails = null;

        try
        {
            charityRegDetails = await $"{charityCommissionUrl}"
                .AppendPathSegment($"/allcharitydetails/{registeredNumber.Trim()}/-0")
                .WithHeader("ocp-apim-subscription-key", subscriptionKey)
                .GetAsync()
                .ReceiveJson<CharityDetails>();
        }
        catch (FlurlHttpException ex) when ((ex.StatusCode == (int)HttpStatusCode.NotFound))
        {
        }
        catch (Exception exc)
        {
            Log(exc, registeredNumber);
        }

        return charityRegDetails;
    }

    private void Log(Exception exc, string registeredNumber)
    {
        var logException = new CdpExceptionLogging($"Failed to call Charity Commission API for registration number: {registeredNumber}.", CdpErrorCodes.CompaniesHouseApiError, exc);

        logger.LogError(logException, "Failed to call Companies House API.");
    }

    private string GetCharityCommissionUrl()
    {
        return configuration["CharityCommission:Url"] ?? "";
    }

    private string GetCharityCommissionSubscriptionKey()
    {
        return configuration["CharityCommission:SubscriptionKey"] ?? "";
    }
}
