using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Users;

[AuthenticatedSessionNotRequired]
public class ResendInviteModel(ISession session, IOrganisationClient organisationClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid OrganisationId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid PersonInviteId { get; set; }

    public PersonInviteState? PersonInviteStateData;

    public async Task<IActionResult> OnGet(Guid id)
    {
        var personInvite = await GetPersonInvite();

        if (personInvite != null)
        {
            PersonInviteStateData = new PersonInviteState()
            {
                FirstName = personInvite.FirstName,
                LastName = personInvite.LastName,
                Email = personInvite.Email,
                Scopes = personInvite.Scopes.ToList()
            };

            session.Set(PersonInviteState.TempDataKey, PersonInviteStateData);
        }

        return RedirectToPage("UserCheckAnswers", new { id = OrganisationId });
    }

    public async Task<PersonInviteModel?> GetPersonInvite()
    {
        try
        {
            var personInvites = await organisationClient.GetOrganisationPersonInvitesAsync(OrganisationId);
            return personInvites.FirstOrDefault(p => p.Id == PersonInviteId);
        }
        catch (CO.CDP.Organisation.WebApiClient.ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }
}