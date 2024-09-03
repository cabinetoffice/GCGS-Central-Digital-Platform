using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.Person.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages;

public class YourDetailsModel(
    ISession session,
    IPersonClient personClient) : LoggedInUserAwareModel(session)
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
        var details = UserDetails;
        if (details.PersonId.HasValue)
        {
            return RedirectToPage("OrganisationSelection");
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

        var details = UserDetails;
        var person = await RegisterPersonAsync(details);

        if (person != null)
        {
            details.PersonId = person.Id;
            details.FirstName = FirstName;
            details.LastName = LastName;

            SessionContext.Set(Session.UserDetailsKey, details);
        }
        else
        {
            return Page();
        }

        return RedirectToPage("OrganisationSelection");
    }

    private async Task<Person.WebApiClient.Person?> RegisterPersonAsync(UserDetails details)
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
        {
            MapApiException(aex);
        }

        return null;
    }

    private void MapApiException(ApiException<Person.WebApiClient.ProblemDetails> aex)
    {
        var code = ExtractErrorCode(aex);

        if (!string.IsNullOrEmpty(code))
        {
            ModelState.AddModelError(string.Empty, code switch
            {
                ErrorCodes.PERSON_ALREADY_EXISTS => ErrorMessagesList.DuplicatePersonName,
                ErrorCodes.ARGUMENT_NULL => ErrorMessagesList.PayLoadIssueOrNullAurgument,
                ErrorCodes.INVALID_OPERATION => ErrorMessagesList.PersonCreationFailed,
                ErrorCodes.UNPROCESSABLE_ENTITY => ErrorMessagesList.UnprocessableEntity,
                _ => ErrorMessagesList.UnexpectedError
            });
        }
    }

    private static string? ExtractErrorCode(ApiException<Person.WebApiClient.ProblemDetails> aex)
    {
        return aex.Result.AdditionalProperties.TryGetValue("code", out var code) && code is string codeString
            ? codeString
            : null;
    }

}