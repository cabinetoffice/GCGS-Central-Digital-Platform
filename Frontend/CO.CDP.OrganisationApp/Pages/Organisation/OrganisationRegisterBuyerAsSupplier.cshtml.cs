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
    IFlashMessageService flashMessageService
    ) : PageModel
{
    public async Task<IActionResult> OnGet(Guid id)
    {
        var orgInfo = await organisationClient.GetOrganisationAsync(id);
        if (!orgInfo.Roles.Contains(PartyRole.Tenderer))
        {
            await organisationClient.AddOrganisationRoles(id, [PartyRole.Tenderer]);
            flashMessageService.SetFlashMessage(
                FlashMessageType.Success, 
                heading: "You have been registered as a supplier",
                description: $"You'll need to <a class=\"govuk-notification-banner__link\" href=\"/organisation/{id}/supplier-information\"\">complete your supplier information</a> to create sharecode."
            );
        }

        return RedirectToPage("OrganisationOverview", new { id });
    }
}
