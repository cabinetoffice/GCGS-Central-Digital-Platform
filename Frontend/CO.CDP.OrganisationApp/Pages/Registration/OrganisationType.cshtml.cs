using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[Authorize]
[ValidateRegistrationStep]
public class OrganisationTypeModel(
    ISession session) : RegistrationStepModel
{
    public override string CurrentPage => OrganisationTypePage;
    public override ISession SessionContext => session;

    [BindProperty]
    [Required(ErrorMessage = "Select the organisation type")]
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
        session.Set(Session.RegistrationDetailsKey, RegistrationDetails);

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