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
        var person = await personClient.LookupPersonAsync(UserDetails.UserUrn);

        await ClaimPersonInvite(person.Id, personInviteId);

        return RedirectToPage("../Organisation/OrganisationSelection");
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

            if (errorCode != ErrorCodes.PERSON_INVITE_ALREADY_CLAIMED)
            {
                throw;
            }
        }
    }
}