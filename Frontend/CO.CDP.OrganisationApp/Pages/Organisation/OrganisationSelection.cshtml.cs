using CO.CDP.Tenant.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

public class OrganisationSelectionModel(
    ITenantClient tenantClient,
    IOrganisationClient organisationClient,
    ISession session) : LoggedInUserAwareModel(session)
{
    public IList<(UserOrganisation Organisation, Review? Review)> UserOrganisations { get; set; } = [];

    public string? Error { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var usersTenant = await tenantClient.LookupTenantAsync();

        UserOrganisations = await Task.WhenAll(usersTenant.Tenants
            .SelectMany(t => t.Organisations)
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
