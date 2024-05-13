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

        var organisation = await RegisterOrganisation(Details);
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

    private async Task<OrganisationWebApiClient.Organisation?> RegisterOrganisation(RegistrationDetails details)
    {
        try
        {
            var payload = NewOrganisationPayload(details);
            if (payload is not null)
            {
                return await organisationClient.CreateOrganisationAsync(payload);
            }
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

        return null;
    }

    private static NewOrganisation? NewOrganisationPayload(RegistrationDetails details)
    {
        if (!details.PersonId.HasValue)
        {
            return null;
        }

        return new NewOrganisation(
            additionalIdentifiers: null,
            address: new OrganisationAddress(
                details.OrganisationAddressLine1,
                details.OrganisationAddressLine2,
                details.OrganisationCityOrTown,
                details.OrganisationCountry,
                details.OrganisationPostcode),
            contactPoint: new OrganisationContactPoint(
                details.OrganisationEmailAddress,
                null,
                null,
                null),
            identifier: new OrganisationIdentifier(
                null,
                null,
                details.OrganisationIdentificationNumber,
                details.OrganisationScheme,
                null),
            name: details.OrganisationName,
            types: [1], // TODO: Need to update - Hard-coded till we have buyer/supplier screen
            personId: details.PersonId.Value
        );
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