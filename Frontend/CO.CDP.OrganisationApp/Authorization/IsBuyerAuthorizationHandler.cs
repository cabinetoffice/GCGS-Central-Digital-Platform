using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Authorization;
using CO.CDP.OrganisationApp.WebApiClients;

namespace CO.CDP.OrganisationApp.Authorization;

public class IsBuyerAuthorizationHandler(
    IOrganisationClient organisationClient,
    ILogger<IsBuyerAuthorizationHandler> logger)
    : AuthorizationHandler<IsBuyerRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        IsBuyerRequirement requirement)
    {
        if (context.Resource is not Guid organisationId || organisationId == Guid.Empty)
        {
            logger.LogWarning("OrganisationId not found or invalid in resource parameter.");
            return;
        }

        var organisation = await organisationClient.GetOrganisationAsync(organisationId);
        if (organisation == null || !organisation.IsBuyer() || organisation.IsPendingBuyer())
        {
            logger.LogInformation($"Organisation {organisationId} is not a valid buyer or is pending.");
            return;
        }

        MouSignatureLatest? mouSignatureLatest;
        try
        {
            mouSignatureLatest = await organisationClient.GetOrganisationLatestMouSignatureAsync(organisation.Id);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            logger.LogInformation($"No MOU signature found for organisation {organisationId}.");
            return;
        }

        var hasBuyerSignedMou = mouSignatureLatest != null && mouSignatureLatest.IsLatest;
        if (!hasBuyerSignedMou)
        {
            logger.LogInformation($"Organisation {organisationId} has not signed the latest MOU.");
            return;
        }

        context.Succeed(requirement);
    }
}