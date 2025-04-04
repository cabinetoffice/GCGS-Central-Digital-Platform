using Microsoft.AspNetCore.Mvc;
using ProblemDetails = CO.CDP.Person.WebApiClient.ProblemDetails;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.Person.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Users;

public class ClaimOrganisationInviteModel(
    IPersonClient personClient,
    ISession session) : LoggedInUserAwareModel(session)
{
    public async Task<IActionResult> OnGet(Guid personInviteId)
    {
        var person = await personClient.LookupPersonAsync(urn: UserDetails.UserUrn, email: null);

        try
        {
            await ClaimPersonInvite(person.Id, personInviteId);

            return RedirectToPage("../Organisation/OrganisationSelection");
        }
        catch (ApiException<ProblemDetails> e)
        {
            var errorCode = e.Result.AdditionalProperties.TryGetValue("code", out var code) && code is string codeString ? codeString : default;

            switch (errorCode)
            {
                case ErrorCodes.PERSON_INVITE_EXPIRED:
                    return RedirectToPage("OrganisationInviteExpired");
                default:
                    throw;
            }
        }
    }

    private async Task ClaimPersonInvite(Guid personId, Guid personInviteId)
    {
        var command = new ClaimPersonInvite(personInviteId);
        try
        {
            await personClient.ClaimPersonInviteAsync(personId, command);
        }
        catch (ApiException<ProblemDetails> e)
        {
            var errorCode = e.Result.AdditionalProperties.TryGetValue("code", out var code) && code is string codeString ? codeString : default;

            // We catch the error and don't do anything if the invite has already been claimed or the person is already a member of the organisation.
            switch (errorCode)
            {
                case ErrorCodes.PERSON_INVITE_ALREADY_CLAIMED:
                case ErrorCodes.PERSON_ALREADY_ADDED_TO_ORGANISATION:
                    break;
                default:
                    throw;
            }
        }
    }
}