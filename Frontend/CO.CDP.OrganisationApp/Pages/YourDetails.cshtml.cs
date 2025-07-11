using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.Person.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CO.CDP.Localization;
using Microsoft.AspNetCore.Html;

namespace CO.CDP.OrganisationApp.Pages;

public class YourDetailsModel(
    ISession session,
    IPersonClient personClient) : LoggedInUserAwareModel(session)
{
    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Users_FirstName_Label))]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Users_FirstName_Validation), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? FirstName { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Users_LastName_Label))]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Users_LastName_Validation), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? LastName { get; set; }

    public IHtmlContent? SystemError { get; set; }

    public IActionResult OnGet()
    {
        var details = UserDetails;
        if (details.PersonId.HasValue)
        {
            return RedirectToPage("Organisation/OrganisationSelection");
        }

        FirstName = details.FirstName;
        LastName = details.LastName;

        return Page();
    }

    public async Task<IActionResult> OnPost(string? redirectUri = null)
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

        if (Helper.ValidRelativeUri(redirectUri))
        {
            return Redirect(redirectUri!);
        }

        return RedirectToPage("Organisation/OrganisationSelection");
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
            var error = code switch
            {
                ErrorCodes.PERSON_GUID_ALREADY_EXISTS => ErrorMessagesList.DuplicatePersonName,
                ErrorCodes.PERSON_EMAIL_ALREADY_EXISTS => string.Format(ErrorMessagesList.FirstAccessDuplicatePersonEmail, HttpContext.TraceIdentifier),
                ErrorCodes.ARGUMENT_NULL => ErrorMessagesList.PayLoadIssueOrNullArgument,
                ErrorCodes.INVALID_OPERATION => ErrorMessagesList.PersonCreationFailed,
                ErrorCodes.UNPROCESSABLE_ENTITY => ErrorMessagesList.UnprocessableEntity,
                _ => ErrorMessagesList.UnexpectedError
            };

            if (code == ErrorCodes.PERSON_EMAIL_ALREADY_EXISTS)
            {
                SystemError = new HtmlString(error);
            }
            else
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }
    }

    private static string? ExtractErrorCode(ApiException<Person.WebApiClient.ProblemDetails> aex)
    {
        return aex.Result.AdditionalProperties.TryGetValue("code", out var code) && code is string codeString
            ? codeString
            : null;
    }

}
