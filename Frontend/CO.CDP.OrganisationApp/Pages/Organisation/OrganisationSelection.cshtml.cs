using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

public class OrganisationSelectionModel(
    IUserInfoService userInfoService,
    IOrganisationClient organisationClient,
    ISession session) : LoggedInUserAwareModel(session)
{
    public IList<(UserOrganisationInfo Organisation, Review? Review)> UserOrganisations { get; set; } = [];

    public string? Error { get; set; }

    public async Task<IActionResult> OnGet()
    {
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
}
