using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.OrganisationApp.Pages.Users;

[Authorize(Policy = OrgScopeRequirement.Admin)]
public class UserRemoveConfirmationModel(
IOrganisationClient organisationClient,
ISession session
) : LoggedInUserAwareModel(session)
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    // UserId can be applied to either a Person or PersonInvite
    [BindProperty(SupportsGet = true)]
    public Guid UserId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select yes to confirm remove user")]
    public bool? ConfirmRemove { get; set; }

    [BindProperty]
    public string? UserFullName { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var person = await GetPerson(organisationClient);

        if (person == null || person.Id == UserDetails.PersonId)
        {
            return Redirect("/page-not-found");
        }

        UserFullName = person.FirstName + " " + person.LastName;

        return Page();
    }

    public async Task<IActionResult> OnGetPersonInvite()
    {
        var personInvite = await GetPersonInvite(organisationClient);

        if (personInvite == null)
            return Redirect("/page-not-found");

        UserFullName = personInvite.FirstName + " " + personInvite.LastName;

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (ConfirmRemove == true)
        {
            var person = await GetPerson(organisationClient);

            if (person == null || person.Id == UserDetails.PersonId)
            {
                return Redirect("/page-not-found");
            }

            UserFullName = person.FirstName + " " + person.LastName;

            await organisationClient.RemovePersonFromOrganisationAsync(Id, new RemovePersonFromOrganisation(person.Id));
        }

        return RedirectToPage("UserSummary", new { Id });
    }

    public async Task<IActionResult> OnPostPersonInvite()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (ConfirmRemove == true)
        {
            var personInvite = await GetPersonInvite(organisationClient);
            if (personInvite == null)
                return Redirect("/page-not-found");

            await organisationClient.RemovePersonInviteFromOrganisationAsync(Id, UserId);
        }

        return RedirectToPage("UserSummary", new { Id });
    }

    public async Task<Organisation.WebApiClient.PersonInviteModel?> GetPersonInvite(IOrganisationClient organisationClient)
    {
        try
        {
            var personInvites = await organisationClient.GetOrganisationPersonInvitesAsync(Id);
            return personInvites.FirstOrDefault(pi => pi.Id == UserId);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    public async Task<Organisation.WebApiClient.Person?> GetPerson(IOrganisationClient organisationClient)
    {
        try
        {
            var persons = await organisationClient.GetOrganisationPersonsAsync(Id);
            return persons.FirstOrDefault(p => p.Id == UserId);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }
}