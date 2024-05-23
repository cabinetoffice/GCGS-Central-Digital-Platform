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
        var registrationDetails = VerifySession();

        FirstName = registrationDetails.FirstName;
        LastName = registrationDetails.LastName;

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var registrationDetails = VerifySession();

        var person = await RegisterPerson(registrationDetails);

        if (person != null)
        {
            registrationDetails.PersonId = person.Id;
            registrationDetails.FirstName = FirstName;
            registrationDetails.LastName = LastName;

            session.Set(Session.RegistrationDetailsKey, registrationDetails);
        }
        else
        {
            return Page();
        }

        return RedirectToPage("OrganisationSelection");
    }

    private async Task<Person.WebApiClient.Person?> RegisterPerson(RegistrationDetails registrationDetails)
    {
        Person.WebApiClient.Person? person = null;

        try
        {
            person = await personClient.CreatePersonAsync(new NewPerson(
                userUrn: registrationDetails.UserUrn,
                email: registrationDetails.Email,
                phone: registrationDetails.Phone,
                firstName: FirstName,
                lastName: LastName
            ));
        }
        catch (ApiException<Person.WebApiClient.ProblemDetails> aex)
        {
            var statusCode = aex.StatusCode;
            var problemDetails = aex.Result;

            switch (statusCode)
            {
                case StatusCodes.Status400BadRequest:
                    ModelState.AddModelError(string.Empty, ErrorMessagesList.DuplicatePersonName);
                    break;

                case StatusCodes.Status404NotFound:
                    ModelState.AddModelError(string.Empty, ErrorMessagesList.PersonNotFound);
                    return null;

                default:
                    Error = ErrorMessagesList.UnexpectedError;
                    break;
            }
        }
        catch (Exception ex)
        {
            Error = ErrorMessagesList.UnexpectedError;
        }
        return person;
    }

    private RegistrationDetails VerifySession()
    {
        var registrationDetails = session.Get<RegistrationDetails>(Session.RegistrationDetailsKey)
            ?? throw new Exception(ErrorMessagesList.SessionNotFound);

        return registrationDetails;
    }
}
