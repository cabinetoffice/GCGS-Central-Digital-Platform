using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;

namespace CO.CDP.OrganisationApp.Pages.Users;

[Authorize]
public class UserCheckAnswersModel(
    IOrganisationClient organisationClient,
    ISession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public PersonInviteState? PersonInviteStateData;

    public IActionResult OnGet()
    {
        PersonInviteStateData = session.Get<PersonInviteState>(PersonInviteState.TempDataKey) ?? null;

        if (PersonInviteStateData == null || !Validate(PersonInviteStateData))
        {
            return RedirectToPage("AddUser", new { Id });
        }
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        PersonInviteStateData = session.Get<PersonInviteState>(PersonInviteState.TempDataKey) ?? null;

        if (PersonInviteStateData == null || !Validate(PersonInviteStateData))
        {
            return RedirectToPage("AddUser", new { Id });
        }

        // TODO: Create person invite

        session.Remove(PersonInviteState.TempDataKey);

        return RedirectToPage("UserSummary", new { Id });
    }

    private static bool Validate(PersonInviteState personInviteStateData)
    {
        return !string.IsNullOrEmpty(personInviteStateData.FirstName)
               && !string.IsNullOrEmpty(personInviteStateData.LastName)
               && !string.IsNullOrEmpty(personInviteStateData.Email)
               && !personInviteStateData.Scopes.IsNullOrEmpty();
    }
}