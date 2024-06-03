using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.Person.WebApiClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[Authorize]
public class YourDetailsModel(
    ISession session,
    IPersonClient personClient) : PageModel
{
    [BindProperty]
    [DisplayName("First name")]
    [Required(ErrorMessage = "Enter your first name")]
    public string? FirstName { get; set; }

    [BindProperty]
    [DisplayName("Last name")]
    [Required(ErrorMessage = "Enter your last name")]
    public string? LastName { get; set; }

    public string? Error { get; set; }

    public IActionResult OnGet()
    {
        var details = session.Get<UserDetails>(Session.UserDetailsKey);
        if (details == null)
        {
            return Redirect("/one-login/user-info");
        }

        FirstName = details.FirstName;
        LastName = details.LastName;

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var details = session.Get<UserDetails>(Session.UserDetailsKey);
        if (details == null)
        {
            return Redirect("/one-login/user-info");
        }

        var person = await RegisterPerson(details);

        if (person != null)
        {
            details.PersonId = person.Id;
            details.FirstName = FirstName;
            details.LastName = LastName;

            session.Set(Session.UserDetailsKey, details);
        }
        else
        {
            return Page();
        }

        return RedirectToPage("OrganisationSelection");
    }

    private async Task<Person.WebApiClient.Person?> RegisterPerson(UserDetails details)
    {
        try
        {
            return await personClient.CreatePersonAsync(new NewPerson(
                userUrn: details.UserUrn,
                email: details.Email,
                phone: details.Phone,
                firstName: FirstName,
                lastName: LastName
            ));
        }
        catch (ApiException<Person.WebApiClient.ProblemDetails> aex)
               when (aex.StatusCode == StatusCodes.Status400BadRequest)
        {
            ModelState.AddModelError(string.Empty, ErrorMessagesList.PayLoadIssue);
        }

        return null;
    }
}