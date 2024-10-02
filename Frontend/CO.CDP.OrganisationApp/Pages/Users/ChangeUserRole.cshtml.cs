using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.OrganisationApp.Pages.Users;

[Authorize(Policy = OrgScopeRequirement.Admin)]
public class ChangeUserRoleModel(
    IOrganisationClient organisationClient,
    ISession session) : LoggedInUserAwareModel(session)
{
    [BindProperty(SupportsGet = true)]
    public string? Handler { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid ItemId { get; set; }    // Could be a Person Id or an Invite Id - this page is used for both

    [BindProperty]
    public bool? IsAdmin { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Role required")]
    public string? Role { get; set; }

    public string? UserFullName;

    public async Task<IActionResult> OnGetPerson()
    {
        var organisationPerson = await GetOrganisationPerson(organisationClient);

        if (organisationPerson == null || organisationPerson.Id == UserDetails.PersonId)
        {
            return Redirect("/page-not-found");
        }

        UserFullName = organisationPerson.FirstName + " " + organisationPerson.LastName;
        IsAdmin = organisationPerson.Scopes.Contains(OrganisationPersonScopes.Admin);
        Role = GetRole(organisationPerson.Scopes);

        return Page();
    }

    public async Task<IActionResult> OnPostPerson()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var person = await GetOrganisationPerson(organisationClient);

        if (person == null)
        {
            return Redirect("/page-not-found");
        }

        var personInviteUpdateCommand = new UpdatePersonToOrganisation(
            ProcessScopes(person.Scopes)
        );

        try
        {
            await organisationClient.UpdateOrganisationPersonAsync(Id, ItemId, personInviteUpdateCommand);
        }
        catch (CO.CDP.Organisation.WebApiClient.ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return RedirectToPage("UserSummary", new { Id });
    }

    public async Task<CO.CDP.Organisation.WebApiClient.Person?> GetOrganisationPerson(IOrganisationClient organisationClient)
    {
        try
        {
            var persons = await organisationClient.GetOrganisationPersonsAsync(Id);
            return persons.FirstOrDefault(p => p.Id == ItemId);
        }
        catch (CO.CDP.Organisation.WebApiClient.ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public async Task<IActionResult> OnGetPersonInvite()
    {
        var personInvite = await GetPersonInvite(organisationClient);

        if (personInvite == null)
        {
            return Redirect("/page-not-found");
        }

        UserFullName = personInvite.FirstName + " " + personInvite.LastName;
        IsAdmin = personInvite.Scopes.Contains(OrganisationPersonScopes.Admin);
        Role = GetRole(personInvite.Scopes);

        return Page();
    }

    public string? GetRole(ICollection<string> scopes)
    {
        if (scopes.Contains(OrganisationPersonScopes.Editor))
        {
            return OrganisationPersonScopes.Editor;
        }
        else if (scopes.Contains(OrganisationPersonScopes.Viewer))
        {
            return OrganisationPersonScopes.Viewer;
        }

        return null;
    }

    public async Task<IActionResult> OnPostPersonInvite()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var personInvite = await GetPersonInvite(organisationClient);

        if (personInvite == null)
        {
            return Redirect("/page-not-found");
        }

        var personInviteUpdateCommand = new UpdateInvitedPersonToOrganisation(
            ProcessScopes(personInvite.Scopes)
        );

        try
        {
            await organisationClient.UpdatePersonInviteAsync(Id, ItemId, personInviteUpdateCommand);
        }
        catch (CO.CDP.Organisation.WebApiClient.ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return RedirectToPage("UserSummary", new { Id });
    }

    public ICollection<string>? ProcessScopes(ICollection<string> scopes)
    {
        if (scopes != null && scopes.Contains(OrganisationPersonScopes.UserAdmin)) scopes.Remove(OrganisationPersonScopes.UserAdmin);
        if (scopes != null && scopes.Contains(OrganisationPersonScopes.Admin)) scopes.Remove(OrganisationPersonScopes.Admin);
        if (scopes != null && scopes.Contains(OrganisationPersonScopes.Editor)) scopes.Remove(OrganisationPersonScopes.Editor);
        if (scopes != null && scopes.Contains(OrganisationPersonScopes.Viewer)) scopes.Remove(OrganisationPersonScopes.Viewer);

        if (IsAdmin == true)
        {
            scopes?.Add(OrganisationPersonScopes.UserAdmin);
        }

        switch (Role)
        {
            case OrganisationPersonScopes.Admin:
                scopes?.Add(OrganisationPersonScopes.Admin);
                break;
            case OrganisationPersonScopes.Editor:
                scopes?.Add(OrganisationPersonScopes.Editor);
                break;
            default:
                scopes?.Add(OrganisationPersonScopes.Viewer);
                break;
        }

        return scopes;
    }

    public async Task<PersonInviteModel?> GetPersonInvite(IOrganisationClient organisationClient)
    {
        try
        {
            var personInvites = await organisationClient.GetOrganisationPersonInvitesAsync(Id);
            return personInvites.FirstOrDefault(p => p.Id == ItemId);
        }
        catch (CO.CDP.Organisation.WebApiClient.ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }
}