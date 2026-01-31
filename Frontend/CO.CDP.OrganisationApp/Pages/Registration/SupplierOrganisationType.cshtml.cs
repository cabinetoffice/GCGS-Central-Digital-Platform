using CO.CDP.Localization;
using CO.CDP.Mvc.Validation;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[ValidateRegistrationStep]
public class SupplierOrganisationTypeModel(ISession session) : RegistrationStepModel(session)
{
    public override string CurrentPage => SupplierOrganisationTypePage;

    [BindProperty]
    [NotEmpty(ErrorMessageResourceName = nameof(StaticTextResource.Supplier_OperationQuestion_SelectOption), ErrorMessageResourceType = typeof(StaticTextResource))]
    [ValidOperationTypeSelection]
    public List<OperationType>? SelectedOperationTypes { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public IActionResult OnGet()
    {
        if (RegistrationDetails.SupplierOrganisationOperationTypes.Count > 0)
        {
            SelectedOperationTypes = RegistrationDetails.SupplierOrganisationOperationTypes;
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        RegistrationDetails.SupplierOrganisationOperationTypes = SelectedOperationTypes ?? [];
        SessionContext.Set(Session.RegistrationDetailsKey, RegistrationDetails);

        if (RedirectToSummary == true)
        {
            return RedirectToPage("OrganisationDetailsSummary");
        }

        return RedirectToPage("OrganisationDetailsSummary");
    }
}
