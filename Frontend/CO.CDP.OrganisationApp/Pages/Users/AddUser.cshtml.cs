using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Users;

[Authorize]
public class AddUserModel(
    IOrganisationClient organisationClient) : PageModel
{
    public Guid Id { get; set; }

    [BindProperty]
    public Guid? PersonId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Firstname required")]
    public bool? FirstName { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Lastname required")]
    public bool? LastName { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Email required")]
    public bool? Email { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Role required")]
    public bool? Role { get; set; }

    public async Task<IActionResult> OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var persons = await organisationClient.GetOrganisationPersonsAsync(Id);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        return RedirectToPage("UserSummary");
    }
}