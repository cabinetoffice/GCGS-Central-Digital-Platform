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

    [BindProperty]
    [Required(ErrorMessage = "Select yes to add another user")]
    public bool? HasPerson { get; set; }

    public async Task<IActionResult> OnGet(bool? selected)
    {
        // Check how to check roles

        // Check for admin

        // If not admin then 404

        try
        {
            Persons = await organisationClient.GetOrganisationPersonsAsync(Id);
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