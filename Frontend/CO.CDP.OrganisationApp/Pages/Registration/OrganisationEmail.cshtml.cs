using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[AuthorisedSession]
[ValidateRegistrationStep]
public class OrganisationEmailModel(ISession session) : RegistrationStepModel
{
    public override string CurrentPage => OrganisationEmailPage;
    public override ISession SessionContext => session;

    [BindProperty]
    [DisplayName("Enter the organisation's contact email address")]
    [Required(ErrorMessage = "Enter the organisation's contact email address")]
    [EmailAddress(ErrorMessage = "Enter an email address in the correct format, like name@example.com")]
    public string? EmailAddress { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public void OnGet()
    {
        EmailAddress = RegistrationDetails.OrganisationEmailAddress;
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        RegistrationDetails.OrganisationEmailAddress = EmailAddress;

        session.Set(Session.RegistrationDetailsKey, RegistrationDetails);

        if (RedirectToSummary == true)
        {
            return RedirectToPage("OrganisationDetailsSummary");
        }
        else
        {
            return RedirectToPage("OrganisationRegisteredAddress", new { UkOrNonUk = "uk" });
        }
    }
}