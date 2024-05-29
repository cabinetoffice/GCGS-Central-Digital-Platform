using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
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

        return RedirectToPage("OrganisationOverview", new { organisation.Id });
    }

    private async Task<OrganisationWebApiClient.Organisation?> RegisterOrganisation(RegistrationDetails details)
    {
        var payload = NewOrganisationPayload(details);

        if (payload is not null)
        {
            try
            {
                return await organisationClient.CreateOrganisationAsync(payload);
            }
            catch (ApiException<OrganisationWebApiClient.ProblemDetails> aex)
                when (aex.StatusCode == StatusCodes.Status400BadRequest)
            {
                ModelState.AddModelError(string.Empty, ErrorMessagesList.OrganisationCreationFailed);
            }
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
            addresses: [new OrganisationAddress(
                type: AddressType.Registered,
                streetAddress: details.OrganisationAddressLine1,
                streetAddress2: details.OrganisationAddressLine2,
                locality: details.OrganisationCityOrTown,
                region: details.OrganisationRegion,
                countryName: details.OrganisationCountry,
                postalCode: details.OrganisationPostcode)],
            contactPoint: new OrganisationContactPoint(
                email: details.OrganisationEmailAddress,
                name: null,
                telephone: null,
                url: null),
            identifier: new OrganisationIdentifier(
                id: details.OrganisationIdentificationNumber,
                legalName: details.OrganisationName,
                scheme: details.OrganisationScheme),
            name: details.OrganisationName,
            types: details.OrganisationType.HasValue ? [(int)details.OrganisationType.Value] : [],
            personId: details.PersonId.Value
        );
    }

    private RegistrationDetails VerifySession()
    {
        var registrationDetails = session.Get<RegistrationDetails>(Session.RegistrationDetailsKey)
            ?? throw new Exception(ErrorMessagesList.SessionNotFound);

        return registrationDetails;
    }
}