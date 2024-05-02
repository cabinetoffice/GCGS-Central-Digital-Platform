using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[Authorize]
public class OrganisationDetailsSummaryModel(
    ISession session,
    IOrganisationClient organisationClient) : PageModel
{
    public RegistrationDetails? Details { get; set; }

    public void OnGet()
    {
        Details = VerifySession();
    }

    public async Task<IActionResult> OnPost()
    {
        Details = VerifySession();

        await organisationClient.CreateOrganisationAsync
            (new NewOrganisation(
                null,
                new OrganisationAddress(
                    Details.OrganisationAddressLine1,
                    Details.OrganisationAddressLine2,
                    Details.OrganisationCityOrTown,
                    Details.OrganisationCountry,
                    Details.OrganisationPostcode),
                new OrganisationContactPoint(
                    Details.OrganisationEmailAddress,
                    null,
                    null,
                    null),
                new OrganisationIdentifier(
                    null,
                    null,
                    Details.OrganisationIdentificationNumber,
                    Details.OrganisationScheme,
                    null),
                Details.OrganisationName,
                [1] // TODO: Need to Remove - Hard-coded till we have buyer/supplier screen
            ));

        return RedirectToPage("OrganisationAccount");
    }

    private RegistrationDetails VerifySession()
    {
        var registrationDetails = session.Get<RegistrationDetails>(Session.RegistrationDetailsKey);
        if (registrationDetails == null)
        {
            //show error page (Once we finalise)
            throw new Exception("Shoudn't be here");
        }
        return registrationDetails;
    }
}
