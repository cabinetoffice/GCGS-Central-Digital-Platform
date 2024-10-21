using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Linq;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

public class OrganisationRegisterBuyerAsSupplierModel(IOrganisationClient organisationClient) : PageModel
{
    public async Task<IActionResult> OnGet(Guid id)
    {
        var orgInfo = await organisationClient.GetOrganisationAsync(id);
        if (!orgInfo.Roles.Contains(PartyRole.Tenderer))
        {
            orgInfo.Roles.Add(PartyRole.Tenderer);
            await organisationClient.UpdateOrganisationRoles(id, orgInfo.Roles);
        }

        return RedirectToPage("./OrganisationOverview", new { id });
    }
}
