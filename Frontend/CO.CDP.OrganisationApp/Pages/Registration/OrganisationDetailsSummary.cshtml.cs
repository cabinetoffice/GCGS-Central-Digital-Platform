using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[AuthorisedSession]
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
        var organisation = await RegisterOrganisationAsync(UserDetails!, RegistrationDetails);
        if (!ModelState.IsValid || organisation == null)
        {
            return Page();
        }
        session.Remove(Session.RegistrationDetailsKey);
        return RedirectToPage("/OrganisationSelection");
    }

    private async Task<OrganisationWebApiClient.Organisation?> RegisterOrganisationAsync(UserDetails user, RegistrationDetails details)
    {
        var payload = NewOrganisationPayload(user, details);

        if (payload == null)
        {
            ModelState.AddModelError(string.Empty, ErrorMessagesList.PayLoadIssueOrNullAurgument);
            return null;
        }

        try
        {
            return await organisationClient.CreateOrganisationAsync(payload);
        }
        catch (ApiException<OrganisationWebApiClient.ProblemDetails> aex)
        {
            MapApiExceptions(aex);
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
            buyerInfo: new BuyerInformation(
                buyerType: details.BuyerOrganisationType,
                devolvedRegulations: details.Regulations.AsApiClientDevolvedRegulationList()),
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

    private void MapApiExceptions(ApiException<OrganisationWebApiClient.ProblemDetails> aex)
    {
        var code = ExtractErrorCode(aex);

        if (!string.IsNullOrEmpty(code))
        {
            ModelState.AddModelError(string.Empty, code switch
            {
                ErrorCodes.ORGANISATION_ALREADY_EXISTS => ErrorMessagesList.DuplicateOgranisationName,
                ErrorCodes.ARGUMENT_NULL => ErrorMessagesList.PayLoadIssueOrNullAurgument,
                ErrorCodes.INVALID_OPERATION => ErrorMessagesList.OrganisationCreationFailed,
                ErrorCodes.PERSON_DOES_NOT_EXIST => ErrorMessagesList.PersonNotFound,
                ErrorCodes.UNPROCESSABLE_ENTITY => ErrorMessagesList.UnprocessableEntity,
                _ => ErrorMessagesList.UnexpectedError
            });
        }
    }

    private static string? ExtractErrorCode(ApiException<OrganisationWebApiClient.ProblemDetails> aex)
    {
        return aex.Result.AdditionalProperties.TryGetValue("code", out var code) && code is string codeString
            ? codeString
            : null;
    }

}