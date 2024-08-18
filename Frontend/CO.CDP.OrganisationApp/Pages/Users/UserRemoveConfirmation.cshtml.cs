using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Users;

public class UserRemoveConfirmationModel(
IOrganisationClient organisationClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid PersonId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select yes to confirm remove user")]
    public bool? ConfirmRemove { get; set; }

    [BindProperty]
    public string? UserFullName { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var person = await GetPerson(organisationClient);
        if (person == null)
            return Redirect("/page-not-found");

        UserFullName = person.FirstName + " " + person.LastName;

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
            if (person == null)
                return Redirect("/page-not-found");

            UserFullName = person.FirstName + " " + person.LastName;

            await organisationClient.RemovePersonFromOrganisationAsync(Id, PersonId);
        }

        return RedirectToPage("UserSummary", new { Id });
    }

    private async Task<Organisation.WebApiClient.Person?> GetPerson(IOrganisationClient organisationClient)
    {
        try
        {
            var persons = await organisationClient.GetOrganisationPersonsAsync(Id);
            return persons.FirstOrDefault(p => p.Id == PersonId);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }
}