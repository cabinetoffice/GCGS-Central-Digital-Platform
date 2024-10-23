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
            await organisationClient.AddOrganisationRoles(id, [PartyRole.Tenderer]);
            tempDataService.Put(FlashMessageTypes.Success, new FlashMessage(
                "You have been registered as a supplier",
                $"You'll need to <a class=\"govuk-notification-banner__link\" href=\"/organisation/{id}/supplier-information\"\">add supplier information</a> to be able to create sharecode."
            ));
        }

        return RedirectToPage("OrganisationOverview", new { id });
    }
}
