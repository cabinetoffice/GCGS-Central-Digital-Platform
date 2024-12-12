using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[ValidateRegistrationStep]
public class OrganisationTypeModel(
    ISession session) : RegistrationStepModel(session)
{
    public override string CurrentPage => OrganisationTypePage;

    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.OrganisationRegistration_OrganisationType_BuyerOrSupplierValidation), ErrorMessageResourceType = typeof(StaticTextResource))]
    public OrganisationType? OrganisationType { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public IActionResult OnGet()
    {
        OrganisationType = RegistrationDetails.OrganisationType;

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        RegistrationDetails.OrganisationType = OrganisationType;
        SessionContext.Set(Session.RegistrationDetailsKey, RegistrationDetails);

        if (RedirectToSummary == true)
        {
            return RedirectToPage("OrganisationDetailsSummary");
        }
        else
        {
            return RedirectToPage("CompanyHouseNumberQuestion");
        }
    }
}
