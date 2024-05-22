using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Helpers;
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
    public required RegistrationDetails Details { get; set; }

    public string? Error { get; set; }

    public void OnGet()
    {
        Details = VerifySession();
    }

    public async Task<IActionResult> OnPost()
    {
        Details = VerifySession();

        try
        {
            var organisation = await ApiHelper.CallApiAsync(
                        () => RegisterOrganisation(Details),
                        "Failed to register organisation."
            );

            if (organisation != null)
            {
                Details.OrganisationId = organisation.Id;
                session.Set(Session.RegistrationDetailsKey, Details);
            }
            else
            {
                return Page();
            }

            return RedirectToPage("OrganisationOverview", new { organisation.Id });
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            return Page();
        }
    }

    private async Task<OrganisationWebApiClient.Organisation?> RegisterOrganisation(RegistrationDetails details)
    {
        var payload = NewOrganisationPayload(details);

        if (payload is not null)
        {
            return await organisationClient.CreateOrganisationAsync(payload);
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
            types: details.OrganisationType.HasValue ? [(int)details.OrganisationType.Value] : [],
            personId: details.PersonId.Value
        );
    }

    private RegistrationDetails VerifySession()
    {
        var registrationDetails = session.Get<RegistrationDetails>(Session.RegistrationDetailsKey)
            ?? throw new Exception("Session not found");

        return registrationDetails;
    }
}