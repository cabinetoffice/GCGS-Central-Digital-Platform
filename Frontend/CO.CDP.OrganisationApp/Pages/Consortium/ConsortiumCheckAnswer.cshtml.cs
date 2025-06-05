using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Consortium;

[FeatureGate(FeatureFlags.Consortium)]

public class ConsortiumCheckAnswerModel(
    ISession session,
    IOrganisationClient organisationClient) :  ConsortiumStepModel(session)
{
    public override string CurrentPage => ConsortiumEmailPage;
    public string? ConsortiumName => ConsortiumDetails.ConsortiumName;
    public OrganisationWebApiClient.Organisation? OrganisationDetails { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public IActionResult OnGet()
    {
        return Page();
    }

    private async Task<OrganisationWebApiClient.Organisation?> RegisterConsortiumAsync()
    {
        var payload = NewOrganisationPayload(UserDetails, ConsortiumDetails);

        if (payload == null)
        {
            ModelState.AddModelError(string.Empty, ErrorMessagesList.PayLoadIssueOrNullArgument);
            return null;
        }

        try
        {
            return await organisationClient.CreateOrganisationAsync(payload);
        }
        catch (OrganisationWebApiClient.ApiException<OrganisationWebApiClient.ProblemDetails> aex)
        {
            ApiExceptionMapper.MapApiExceptions(aex, ModelState);
        }

        return null;
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid) return Page();

        var organisation = await RegisterConsortiumAsync();

        if (!ModelState.IsValid || organisation == null) return Page();

        SessionContext.Remove(Session.ConsortiumKey);

        return RedirectToPage("ConsortiumOverview", new { organisation.Id });
    }

    private static OrganisationWebApiClient.NewOrganisation? NewOrganisationPayload(UserDetails user, ConsortiumDetails details)
    {
        if (!user.PersonId.HasValue)
        {
            return null;
        }

        return new OrganisationWebApiClient.NewOrganisation(
            additionalIdentifiers: null,
            addresses: [new OrganisationWebApiClient.OrganisationAddress(
                type: Constants.AddressType.Postal.AsApiClientAddressType(),
                streetAddress: details.PostalAddress!.AddressLine1,
                locality: details.PostalAddress.TownOrCity,
                region: null,
                country: details.PostalAddress.Country,
                countryName: details.PostalAddress.CountryName,
                postalCode: details.PostalAddress.Postcode)],
            contactPoint: new OrganisationWebApiClient.OrganisationContactPoint(
                email: details.ConsortiumEmail,
                name: null,
                telephone: null,
                url: null),
            identifier: new OrganisationWebApiClient.OrganisationIdentifier(
                id: Guid.NewGuid().ToString(),
                legalName: details.ConsortiumName,
                scheme: OrganisationSchemeType.Other),
            name: details.ConsortiumName,
            type: OrganisationWebApiClient.OrganisationType.InformalConsortium,
            roles: [PartyRole.Tenderer]
        );
    }
}
