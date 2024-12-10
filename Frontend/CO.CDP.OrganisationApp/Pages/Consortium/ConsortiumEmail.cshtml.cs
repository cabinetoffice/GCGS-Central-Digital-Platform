using CO.CDP.Localization;
using CO.CDP.Mvc.Validation;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Consortium;

[FeatureGate(FeatureFlags.Consortium)]
[ValidateConsortiumStep]
public class ConsortiumEmailModel(
    ISession session,
    IOrganisationClient organisationClient) : ConsortiumStepModel(session)
{
    public override string CurrentPage => ConsortiumEmailPage;

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Consortium_ConsortiumEmail_Heading))]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Consortium_ConsortiumEmail_Required_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    [ValidEmailAddress(ErrorMessageResourceName = nameof(StaticTextResource.Global_Email_Invalid_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? EmailAddress { get; set; }

    public string? ConsortiumName => ConsortiumDetails.ConstortiumName;

    public IActionResult OnGet()
    {
        EmailAddress = ConsortiumDetails.ConstortiumEmail;

        return Page();
    }
    public async Task<IActionResult> OnPost()
    {
        ConsortiumDetails.ConstortiumEmail = EmailAddress;

        SessionContext.Set(Session.ConsortiumKey, ConsortiumDetails);

        var organisation = await RegisterConsortiumAsync();

        if (!ModelState.IsValid || organisation == null) return Page();

        SessionContext.Remove(Session.ConsortiumKey);

        return RedirectToPage("ConsortiumOverview", new { organisation.Id });
    }

    private async Task<OrganisationWebApiClient.Organisation?> RegisterConsortiumAsync()
    {
        var payload = NewOrganisationPayload(UserDetails, ConsortiumDetails);

        if (payload == null)
        {
            ModelState.AddModelError(string.Empty, ErrorMessagesList.PayLoadIssueOrNullAurgument);
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
                email: details.ConstortiumEmail,
                name: null,
                telephone: null,
                url: null),
            identifier: new OrganisationWebApiClient.OrganisationIdentifier(
                id: Guid.NewGuid().ToString(),
                legalName: OrganisationSchemeType.Other,
                scheme: string.Empty),
            name: details.ConstortiumName,
            type: OrganisationWebApiClient.OrganisationType.InformalConsortium,
            roles: []
        );
    }
}
