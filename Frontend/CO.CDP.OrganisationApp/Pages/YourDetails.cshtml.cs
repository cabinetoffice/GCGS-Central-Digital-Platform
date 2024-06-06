using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.Person.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages;

[AuthorisedSession]
public class YourDetailsModel(
    ISession session,
    IPersonClient personClient) : LoggedInUserAwareModel
{
    public override ISession SessionContext => session;

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
        if (UserDetails.PersonId.HasValue)
        {
            return RedirectToPage("OrganisationSelection");
        }

        FirstName = UserDetails.FirstName;
        LastName = UserDetails.LastName;

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var person = await RegisterPerson(UserDetails);

        if (person != null)
        {
            UserDetails.PersonId = person.Id;
            UserDetails.FirstName = FirstName;
            UserDetails.LastName = LastName;

            session.Set(Session.UserDetailsKey, UserDetails);
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