using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Users;

public class ChangeUserRole(
    IOrganisationClient organisationClient,
    ISession session) : LoggedInUserAwareModel(session)
{
    public const string ScopeAdmin = "ADMIN";
    public const string ScopeEditor = "EDITOR";
    public const string ScopeViewer = "VIEWER";

    [BindProperty(SupportsGet = true)]
    public string Handler { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid ItemId { get; set; }    // Could be a Person Id or an Invite Id - this page is used for both

    [BindProperty]
    public bool? IsAdmin { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Role required")]
    public string? Role { get; set; }

    public string UserFullName;

    public async Task<IActionResult> OnGetPerson()
    {
        //var person = await GetPerson(organisationClient);

        // TODO: Access checking - should only be able to do this if you have the admin role

        //if (person == null || person.Id == UserDetails.PersonId)
        //{
        //    return Redirect("/page-not-found");
        //}

        return Page();
    }

    public async Task<IActionResult> OnGetPersonInvite()
    {
        var personInvite = await GetPersonInvite(organisationClient);

        if (personInvite == null)
        {
            return Redirect("/page-not-found");
        }

        UserFullName = personInvite.FirstName + " " + personInvite.LastName;
        IsAdmin = personInvite.Scopes.Contains(ScopeAdmin);
        Role = GetRole(personInvite.Scopes);

        return Page();
    }

    public string? GetRole(ICollection<string> scopes)
    {
        if (scopes.Contains(ScopeEditor))
        {
            return ScopeEditor;
        }
        else if (scopes.Contains(ScopeViewer))
        {
            return ScopeViewer;
        }

        return null;
    }

    public async Task<IActionResult> OnPostPerson()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // TODO: Update person
        
        return RedirectToPage("UserSummary", new { Id });
    }

    public async Task<IActionResult> OnPostPersonInvite()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // TODO: Update person invite
        
        return RedirectToPage("UserSummary", new { Id });
    }

    public async Task<PersonInviteModel?> GetPersonInvite(IOrganisationClient organisationClient)
    {
        try
        {
            var personInvites = await organisationClient.GetOrganisationPersonInvitesAsync(Id);
            return personInvites.FirstOrDefault(p => p.Id == ItemId);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }
}