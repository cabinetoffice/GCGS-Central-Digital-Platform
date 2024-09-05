using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;

namespace CO.CDP.OrganisationApp.Pages.Users;

public class UserCheckAnswersModel(
    IOrganisationClient organisationClient,
    ITempDataService tempDataService,
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

        var personInviteCommand = new InvitePersonToOrganisation(
            PersonInviteStateData.Email,
            PersonInviteStateData.FirstName,
            PersonInviteStateData.LastName,
            PersonInviteStateData.Scopes
        );

        await organisationClient.CreatePersonInviteAsync(Id, personInviteCommand);

        session.Remove(PersonInviteState.TempDataKey);

        tempDataService.Put(FlashMessageTypes.Success, $"You've sent an email invite to {PersonInviteStateData.FirstName} {PersonInviteStateData.LastName}");

        return RedirectToPage("UserSummary", new { Id });
    }

    public static bool Validate(PersonInviteState personInviteStateData)
    {
        return !string.IsNullOrEmpty(personInviteStateData.FirstName)
               && !string.IsNullOrEmpty(personInviteStateData.LastName)
               && !string.IsNullOrEmpty(personInviteStateData.Email)
               && !personInviteStateData.Scopes.IsNullOrEmpty();
    }
}