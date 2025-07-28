using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.OrganisationApp.Authorization;

public class BuyerMouAuthorizationHandler(
    ISession session, IServiceScopeFactory serviceScopeFactory,
    IOrganisationClient organisationClient,
    ILogger<BuyerMouAuthorizationHandler> logger)
    : AuthorizationHandler<BuyerMouRequirement>
{
    private static bool IsValidOrganisationId(Guid? organisationId) => organisationId.HasValue && organisationId.Value != Guid.Empty;

    private static bool IsValidBuyer(Organisation.WebApiClient.Organisation? organisation) => organisation != null && organisation.IsBuyer() && !organisation.IsPendingBuyer();

    private static bool HasSignedLatestMou(MouSignatureLatest? mouSignatureLatest) => mouSignatureLatest is { IsLatest: true };

    private static Models.UserDetails? TryGetUserDetails(ISession session) => session.Get<Models.UserDetails>(Session.UserDetailsKey);

    private static Guid? TryGetOrganisationId(IServiceScopeFactory serviceScopeFactory, ILogger logger)
    {
        try
        {
            using var serviceScope = serviceScopeFactory.CreateScope();
            var userInfoService = serviceScope.ServiceProvider.GetRequiredService<IUserInfoService>();
            userInfoService.GetUserInfo().Wait();
            return userInfoService.GetOrganisationId();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get organisation ID from user info service.");
            return null;
        }
    }

    private static async Task<Organisation.WebApiClient.Organisation?> TryGetOrganisationAsync(IOrganisationClient organisationClient, Guid organisationId, ILogger logger)
    {
        try
        {
            return await organisationClient.GetOrganisationAsync(organisationId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get organisation {OrganisationId}.", organisationId);
            return null;
        }
    }

    private static async Task<MouSignatureLatest?> TryGetLatestMouSignatureAsync(IOrganisationClient organisationClient, Guid organisationId, ILogger logger)
    {
        try
        {
            return await organisationClient.GetOrganisationLatestMouSignatureAsync(organisationId);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            logger.LogInformation("No MOU signature found for organisation {OrganisationId}.", organisationId);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get MOU signature for organisation {OrganisationId}.", organisationId);
            return null;
        }
    }

    private static bool HasBuyerRole(IUserInfoService userInfoService)
    {
        try
        {
            var userInfo = userInfoService.GetUserInfo().Result;
            var organisationId = userInfoService.GetOrganisationId();

            if (organisationId == null) return false;

            var organisation = userInfo.Organisations.FirstOrDefault(o => o.Id == organisationId);
            return organisation?.Roles.Contains(Tenant.WebApiClient.PartyRole.Buyer) == true;
        }
        catch
        {
            return false;
        }
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        BuyerMouRequirement requirement)
    {
        var userDetails = TryGetUserDetails(session);
        if (userDetails?.PersonId is null)
        {
            context.Fail();
            return;
        }

        var organisationId = TryGetOrganisationId(serviceScopeFactory, logger);
        if (!IsValidOrganisationId(organisationId))
        {
            logger.LogWarning("OrganisationId not found or invalid.");
            context.Fail();
            return;
        }

        using var serviceScope = serviceScopeFactory.CreateScope();
        var userInfoService = serviceScope.ServiceProvider.GetRequiredService<IUserInfoService>();

        if (!HasBuyerRole(userInfoService))
        {
            logger.LogInformation("Organisation {OrganisationId} does not have buyer party role.", organisationId);
            context.Fail();
            return;
        }

        var organisation = await TryGetOrganisationAsync(organisationClient, organisationId!.Value, logger);
        if (!IsValidBuyer(organisation))
        {
            logger.LogInformation("Organisation {OrganisationId} is not a valid buyer or is pending.", organisationId);
            context.Fail();
            return;
        }

        var mouSignatureLatest = organisation != null
            ? await TryGetLatestMouSignatureAsync(organisationClient, organisation.Id, logger)
            : null;
        if (!HasSignedLatestMou(mouSignatureLatest))
        {
            logger.LogInformation("Organisation {OrganisationId} has not signed the latest MOU.", organisationId);
            context.Fail();
            return;
        }

        context.Succeed(requirement);
    }
}