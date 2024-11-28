using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace CO.CDP.OrganisationApp.Pages.Registration;

[ValidateRegistrationStep]
public class OrganisationInternationalIdentificationCountryModel(ISession session) : RegistrationStepModel(session)
{
    public override string CurrentPage => OrganisationInternationalIdentificationCountryPage;

    [BindProperty]
    [DisplayName("Select which country your organisation is registered")]
    [Required(ErrorMessage = "Select a country")]
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

        return RedirectToPage("OrganisationInternationalIdentification");
    }
}