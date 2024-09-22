using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Mvc;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[ValidateRegistrationStep]
public class OrganisationDetailsSummaryModel(
    ISession session,
    IOrganisationClient organisationClient) : RegistrationStepModel(session)
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
                locality: details.OrganisationCityOrTown,
                region: details.OrganisationRegion,
                country: details.OrganisationCountryCode,
                countryName: details.OrganisationCountryName,
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
                ErrorCodes.UNKNOWN_ORGANISATION => ErrorMessagesList.UnknownOrganisation,
                ErrorCodes.BUYER_INFO_NOT_EXISTS => ErrorMessagesList.BuyerInfoNotExists,
                ErrorCodes.UNKNOWN_BUYER_INFORMATION_UPDATE_TYPE => ErrorMessagesList.UnknownBuyerInformationUpdateType,
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