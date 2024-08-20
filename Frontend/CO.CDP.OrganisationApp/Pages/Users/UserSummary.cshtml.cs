using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Users;

[Authorize]
public class UserSummaryModel(
    IOrganisationClient organisationClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty] public ICollection<Organisation.WebApiClient.Person> Persons { get; set; } = [];

    [BindProperty] public ICollection<PersonInviteModel> PersonInvites { get; set; } = [];

    [BindProperty]
    [Required(ErrorMessage = "Select yes to add another user")]
    public bool? HasPerson { get; set; }

    public async Task<IActionResult> OnGet(bool? selected)
    {
        // TODO: Page should only be accessible to users with ADMIN scope - Agreed to otherwise display 404

        try
        {
            // These are the ones that have been fully added
            Persons = await organisationClient.GetOrganisationPersonsAsync(Id);

            // These are the invites
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
}