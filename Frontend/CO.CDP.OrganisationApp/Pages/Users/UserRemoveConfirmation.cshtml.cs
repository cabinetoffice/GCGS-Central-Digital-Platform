using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CO.CDP.OrganisationApp.Models;

namespace CO.CDP.OrganisationApp.Pages.Users;

public class UserRemoveConfirmationModel(
IOrganisationClient organisationClient,
ISession session
) : PageModel
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

        var userDetails = session.Get<UserDetails>(Session.UserDetailsKey);

        if (person == null || userDetails == null || person.Id == userDetails.PersonId)
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

            var userDetails = session.Get<UserDetails>(Session.UserDetailsKey);

            if (person == null || userDetails == null || person.Id == userDetails.PersonId)
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