using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[Authorize]
public class OrganisationDetailsSummaryModel(
    ISession session,
    IOrganisationClient organisationClient) : PageModel
{
    public RegistrationDetails? Details { get; set; }

    [BindProperty]
    public string? Error { get; set; }

    public void OnGet()
    {
        Details = VerifySession();
    }

    public async Task<IActionResult> OnPost()
    {
        Details = VerifySession();

        var organisation = await RegisterOrganisation();
        if (organisation != null)
        {
            Details.OrganisationId = organisation.Id;
            session.Set(Session.RegistrationDetailsKey, Details);
        }
        else
        {
            return Page();
        }

        return RedirectToPage("OrganisationAccount");
    }

    private async Task<OrganisationWebApiClient.Organisation?> RegisterOrganisation()
    {
        OrganisationWebApiClient.Organisation? organisation = null;

        try
        {
            organisation = await organisationClient.CreateOrganisationAsync
                (new NewOrganisation(
                    null,
                    new OrganisationAddress(
                        Details!.OrganisationAddressLine1,
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
                    Details.OrganisationType.HasValue ? [(int)Details.OrganisationType.Value] : []
                ));
        }
        catch (ApiException aex)
            when (aex is ApiException<OrganisationWebApiClient.ProblemDetails> pd)
        {
            Error = pd.Result.Detail;
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }

        return organisation;
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