using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class OrganisationRegisterBuyerAsSupplierModel(
    IOrganisationClient organisationClient,
    ITempDataService tempDataService
    ) : PageModel
{
    public async Task<IActionResult> OnGet(Guid id)
    {
        var orgInfo = await organisationClient.GetOrganisationAsync(id);
        if (!orgInfo.Roles.Contains(PartyRole.Tenderer))
        {
            orgInfo.Roles.Add(PartyRole.Tenderer);
            await organisationClient.UpdateOrganisationRoles(id, orgInfo.Roles);
            tempDataService.Put(FlashMessageTypes.Success, new FlashMessage(
                "You have been registered as a supplier",
                $"You'll need to <a class=\"govuk-notification-banner__link\" href=\"/organisation/{id}/register-buyer-as-supplier\"\">add supplier information</a> to be able to create sharecode."
            ));
        }

        return RedirectToPage("OrganisationOverview", new { id });
    }
}
