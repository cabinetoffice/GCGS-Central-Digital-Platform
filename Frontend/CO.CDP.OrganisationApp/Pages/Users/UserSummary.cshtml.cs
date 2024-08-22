using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using CO.CDP.OrganisationApp.Models;

namespace CO.CDP.OrganisationApp.Pages.Users;

[Authorize]
public class UserSummaryModel(
    IOrganisationClient organisationClient,
    ISession session) : PageModel
{
    private const string ScopeAdmin = "ADMIN";

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public Guid? SignedInPersonId { get; set; }

    [BindProperty] public ICollection<Organisation.WebApiClient.Person> Persons { get; set; } = [];

    [BindProperty] public ICollection<PersonInviteModel> PersonInvites { get; set; } = [];

    [BindProperty]
    [Required(ErrorMessage = "Select yes to add another user")]
    public bool? HasPerson { get; set; }

    public async Task<IActionResult> OnGet(bool? selected)
    {
        var userDetails = session.Get<UserDetails>(Session.UserDetailsKey);

        SignedInPersonId = userDetails?.PersonId;

        try
        {
            Persons = await organisationClient.GetOrganisationPersonsAsync(Id);

            if (!UserHasAdminScopeForOrganisation())
            {
                return Redirect("/page-not-found");
            }

            PersonInvites = await organisationClient.GetOrganisationPersonInvitesAsync(Id);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        HasPerson = selected;

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (HasPerson == true)
        {
            return Redirect("add-user");
        }

        return Redirect("/organisation/" + Id);
    }

    public bool UserHasAdminScopeForOrganisation()
    {
        bool userIsAdminForOrg = false;

        foreach (var person in Persons)
        {
            if (person.Id == SignedInPersonId && person.Scopes.Contains(ScopeAdmin))
            {
                userIsAdminForOrg = true;
            }
        }

        if (!userIsAdminForOrg)
        {
            return false;
        }

        return true;
    }
}