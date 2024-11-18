using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Validation;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[ValidateRegistrationStep]
public class OrganisationEmailModel(ISession session) : RegistrationStepModel(session)
{
    public override string CurrentPage => OrganisationEmailPage;

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_Email_Heading))]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Organisation_Email_Required_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    [ValidEmailAddress(ErrorMessageResourceName = nameof(StaticTextResource.Global_Email_Invalid_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
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

        SessionContext.Set(Session.RegistrationDetailsKey, RegistrationDetails);

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
