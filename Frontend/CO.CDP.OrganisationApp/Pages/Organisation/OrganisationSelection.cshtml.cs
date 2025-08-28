using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using PartyRole = CO.CDP.Organisation.WebApiClient.PartyRole;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

public class OrganisationSelectionModel(
    IUserInfoService userInfoService,
    IOrganisationClient organisationClient,
    ISession session,
    IFeatureManager featureManager) : LoggedInUserAwareModel(session)
{
    public IList<(UserOrganisationInfo Organisation, Review? Review)> UserOrganisations { get; set; } = [];

    [BindProperty]
    public bool IsBuyerViewEnabled { get; set; }

    public string? Error { get; set; }

    public async Task<IActionResult> OnGet()
    {
        IsBuyerViewEnabled = await featureManager.IsEnabledAsync(FeatureFlags.BuyerView);

        var userInfo = await userInfoService.GetUserInfo();

        UserOrganisations = await Task.WhenAll(userInfo.Organisations
            .Select(async organisation =>
            {
                Review? review = null;
                if (organisation.PendingRoles.Count > 0)
                {
                    var reviews = await organisationClient.GetOrganisationReviewsAsync(organisation.Id);
                    review = reviews?.FirstOrDefault();
                }

                return (organisation, review);
            }));

        return Page();
    }

    public string OrganisationUrl(UserOrganisationInfo organisation, bool buyerViewFeatureEnabled)
    {
        if (buyerViewFeatureEnabled && organisation.Roles.Contains((Tenant.WebApiClient.PartyRole)PartyRole.Buyer))
        {
            return $"/organisation/{organisation.Id}/buyer";
        }
        return $"/organisation/{organisation.Id}?origin=selection";
    }
}
