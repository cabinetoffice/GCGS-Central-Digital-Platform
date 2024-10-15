using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Mvc;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[ValidateRegistrationStep]
public class OrganisationDetailsSummaryModel(
    ISession session,
    OrganisationWebApiClient.IOrganisationClient organisationClient) : RegistrationStepModel(session)
{
    public override string CurrentPage => OrganisationSummaryPage;

    public string? Error { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        var organisation = await RegisterOrganisationAsync();
        if (!ModelState.IsValid || organisation == null)
        {
            return Page();
        }

        List<Task> tasks = [];

        if (RegistrationDetails.BuyerOrganisationType != null)
        {
            tasks.Add(organisationClient.UpdateBuyerOrganisationType(organisation.Id, RegistrationDetails.BuyerOrganisationType));
        }

        if (RegistrationDetails.Devolved == true)
        {
            tasks.Add(organisationClient.UpdateBuyerDevolvedRegulations(organisation.Id, RegistrationDetails.Regulations));
        }

        await Task.WhenAll(tasks);

        SessionContext.Remove(Session.RegistrationDetailsKey);
        return RedirectToPage("../Organisation/OrganisationSelection");
    }

    private async Task<OrganisationWebApiClient.Organisation?> RegisterOrganisationAsync()
    {
        var payload = NewOrganisationPayload(UserDetails, RegistrationDetails);

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

    private static OrganisationWebApiClient.NewOrganisation? NewOrganisationPayload(UserDetails user, RegistrationDetails details)
    {
        if (!user.PersonId.HasValue)
        {
            return null;
        }

        return new OrganisationWebApiClient.NewOrganisation(
            additionalIdentifiers: null,
            addresses: [new OrganisationWebApiClient.OrganisationAddress(
                type: Constants.AddressType.Registered.AsApiClientAddressType(),
                streetAddress: details.OrganisationAddressLine1,
                locality: details.OrganisationCityOrTown,
                region: details.OrganisationRegion,
                country: details.OrganisationCountryCode,
                countryName: details.OrganisationCountryName,
                postalCode: details.OrganisationPostcode)],
            contactPoint: new OrganisationWebApiClient.OrganisationContactPoint(
                email: details.OrganisationEmailAddress,
                name: null,
                telephone: null,
                url: null),
            identifier: new OrganisationWebApiClient.OrganisationIdentifier(
                id: details.OrganisationIdentificationNumber,
                legalName: details.OrganisationName,
                scheme: details.OrganisationScheme),
            name: details.OrganisationName,
            roles: [details.OrganisationType!.Value.AsPartyRole()],
            personId: user.PersonId.Value
        );
    }  
}