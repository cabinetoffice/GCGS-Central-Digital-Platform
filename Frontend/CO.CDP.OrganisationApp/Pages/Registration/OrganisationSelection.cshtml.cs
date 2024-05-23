using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[Authorize]
public class OrganisationSelectionModel(
    IOrganisationClient organisationClient,
    ISession session) : PageModel
{
    public IEnumerable<Organisation.WebApiClient.Organisation> Organisations { get; set; } = [];

    public string? Error { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var details = VerifySession();

        try
        {
            Organisations = await organisationClient.ListOrganisationsAsync(details.UserUrn);
        }
        catch (ApiException<Organisation.WebApiClient.ProblemDetails> aex)
        {
            if (aex.StatusCode == StatusCodes.Status404NotFound)
            {
                ModelState.AddModelError(string.Empty, ErrorMessagesList.OrganisationNotFound);
            }
            else
            {
                Error = ErrorMessagesList.UnexpectedError;
            }
        }
        catch (Exception ex)
        {
            Error = ErrorMessagesList.UnexpectedError;
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        return RedirectToPage("OrganisationType");
    }

    private RegistrationDetails VerifySession()
    {
        var details = session.Get<RegistrationDetails>(Session.RegistrationDetailsKey)
            ?? throw new Exception(ErrorMessagesList.SessionNotFound);

        return details;
    }
}