using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;
using CO.CDP.Tenant.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

public class OrganisationSelectionModel(
    ITenantClient tenantClient,
    IOrganisationClient organisationClient,
    ISession session) : LoggedInUserAwareModel(session)
{
    public List<OrganisationWebApiClient.Organisation> Organisations { get; set; } = [];

    public string? Error { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var usersTenant = await tenantClient.LookupTenantAsync();

        // TODO: Tenant call doesn't return details of review comments - need to extend it to include a review_status column as a minimum as part of a refactor of reviews
        // For the moment, hack this data in just for the intial proof of concept
        var organisationTasks = usersTenant.Tenants
            .SelectMany(t => t.Organisations)
            .Select(async o => await organisationClient.GetOrganisationExtendedAsync(o.Id));

        Organisations.AddRange(await Task.WhenAll(organisationTasks));

        // Old way to restore to later
        //usersTenant.Tenants.ToList()
        //    .ForEach(t => t.Organisations.ToList()
        //        .ForEach(o => UserOrganisations.Add(o)));

        return Page();
    }
}