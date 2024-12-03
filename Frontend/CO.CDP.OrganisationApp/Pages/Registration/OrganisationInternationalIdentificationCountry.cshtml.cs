using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace CO.CDP.OrganisationApp.Pages.Registration;

[ValidateRegistrationStep]
public class OrganisationInternationalIdentificationCountryModel(ISession session) : RegistrationStepModel(session)
{
    public override string CurrentPage => OrganisationInternationalIdentificationCountryPage;

    public string NextPage => OrganisationInternationalIdentifierPage;

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    [BindProperty]

    [DisplayName(nameof(StaticTextResource.OrganisationRegistration_InternationalIdentifier_Country_Heading))]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.OrganisationRegistration_InternationalIdentifier_Country_Required_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? Country { get; set; } = string.Empty;

    public void OnGet()
    {
        Country = RegistrationDetails.OrganisationIdentificationCountry;
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        RegistrationDetails.OrganisationIdentificationCountry = Country;

        SessionContext.Set(Session.RegistrationDetailsKey, RegistrationDetails);

        if (RedirectToSummary == true)
        {
            
            return RedirectToPage(NextPage+ "?frm-summary");
        }
        else
        {
            return RedirectToPage("OrganisationInternationalIdentification");
        }
    }
}