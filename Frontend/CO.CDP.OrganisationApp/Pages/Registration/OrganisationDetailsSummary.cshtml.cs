using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[Authorize]
[ValidateRegistrationStep]
public class OrganisationDetailsSummaryModel(
    ISession session,
    IOrganisationClient organisationClient) : RegistrationStepModel
{
    public override string CurrentPage => OrganisationSummaryPage;
    public override ISession SessionContext => session;

    public string? Error { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        var organisation = await RegisterOrganisation(UserDetails!, RegistrationDetails);
        if (organisation == null)
        {
            return Page();
        }
        session.Remove(Session.RegistrationDetailsKey);
        return RedirectToPage("OrganisationOverview", new { organisation.Id });
    }

    private async Task<OrganisationWebApiClient.Organisation?> RegisterOrganisation(UserDetails user, RegistrationDetails details)
    {
        var payload = NewOrganisationPayload(user, details);

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

    private static NewOrganisation? NewOrganisationPayload(UserDetails user, RegistrationDetails details)
    {
        if (!user.PersonId.HasValue)
        {
            return null;
        }

        return new NewOrganisation(
            additionalIdentifiers: null,
            addresses: [new OrganisationAddress(
                type: Constants.AddressType.Registered.AsApiClientAddressType(),
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
            roles: [details.OrganisationType!.Value.AsPartyRole()],
            personId: user.PersonId.Value
        );
    }
}