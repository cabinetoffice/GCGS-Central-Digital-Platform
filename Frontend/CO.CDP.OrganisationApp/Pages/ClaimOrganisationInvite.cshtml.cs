using Microsoft.AspNetCore.Mvc;
using ProblemDetails = CO.CDP.Person.WebApiClient.ProblemDetails;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.Person.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages;

public class ClaimOrganisationInviteModel(
    IPersonClient personClient,
    ISession session) : LoggedInUserAwareModel(session)
{
    public async Task<IActionResult> OnGet(Guid personInviteId)
    {
        var person = await personClient.LookupPersonAsync(UserDetails.UserUrn);

        await ClaimPersonInvite(person.Id, personInviteId);

        SessionContext.Remove("PersonInviteId");

        return RedirectToPage("OrganisationSelection");
    }

    public async Task ClaimPersonInvite(Guid personId, Guid personInviteId)
    {
        var command = new ClaimPersonInvite(personInviteId);
        try
        {
            await personClient.ClaimPersonInviteAsync(personId, command);
        }
        catch (ApiException<ProblemDetails> e)
        {
            var code = ExtractErrorCode(e);
            if (code != ErrorCodes.PERSON_INVITE_ALREADY_CLAIMED)
            {
                throw;
            }
        }
    }

    private static string? ExtractErrorCode(ApiException<ProblemDetails> aex)
    {
        return aex.Result.AdditionalProperties.TryGetValue("code", out var code) && code is string codeString
            ? codeString
            : null;
    }
}