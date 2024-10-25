using CO.CDP.Tenant.WebApiClient;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.OrganisationApp.Pages;

public class OrganisationSelectionModel(
    ITenantClient tenantClient,
    ISession session) : LoggedInUserAwareModel(session)
{
    public IList<UserOrganisation> UserOrganisations { get; set; } = [];

    public string? Error { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var usersTenant = await tenantClient.LookupTenantAsync();

        usersTenant.Tenants.ToList()
            .ForEach(t => t.Organisations.ToList()
                .ForEach(o => UserOrganisations.Add(o)));

        return Page();
    }
}