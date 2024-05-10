using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.Person.WebApiClient;
using CO.CDP.Tenant.WebApiClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;
using PersonWebApiClient = CO.CDP.Person.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[Authorize]
public class OrganisationOverviewModel(
    IOrganisationClient organisationClient) : PageModel
{

    public OrganisationWebApiClient.Organisation? OrganisationDetails { get; set; }

    public async Task OnGet(Guid? id)
    {
        ArgumentNullException.ThrowIfNull(id);

        OrganisationDetails = await organisationClient.GetOrganisationAsync(id!.Value);
    }
}