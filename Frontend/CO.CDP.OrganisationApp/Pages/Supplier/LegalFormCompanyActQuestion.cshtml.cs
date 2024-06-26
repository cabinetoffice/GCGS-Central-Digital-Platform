using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class LegalFormCompanyActQuestionModel(
    IOrganisationClient organisationClient,
    ITempDataService tempDataService) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Please select an option")]
    public bool? RegisteredOnCompanyHouse { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public async Task<IActionResult> OnGet(bool? selected)
    {
        try
        {
            var getSupplierInfoTask = await organisationClient.GetOrganisationSupplierInformationAsync(Id);

            if (getSupplierInfoTask.CompletedLegalForm)
            {
                var legalForm = getSupplierInfoTask.LegalForm;
                RegisteredOnCompanyHouse = legalForm.RegisteredUnderAct2006;
            }
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        if (selected.HasValue)
        {
            RegisteredOnCompanyHouse = selected.Value;
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var supplierInfo = await organisationClient.GetOrganisationSupplierInformationAsync(Id);
            var lf = new LegalForm();

            lf.RegisteredUnderAct2006 = RegisteredOnCompanyHouse;

            if (supplierInfo.CompletedLegalForm)
            {
                var legalForm = supplierInfo.LegalForm;
                lf.RegisteredLegalForm = legalForm.RegisteredLegalForm;
                lf.LawRegistered = legalForm.LawRegistered;
                lf.RegistrationDate = legalForm.RegistrationDate;
                lf.SupplierInformationOrganisationId = Id;
            }

            tempDataService.Put(LegalForm.TempDataKey, lf);

            if (RegisteredOnCompanyHouse == true)
            {
                return RedirectToPage("LegalFormSelectOrganisation", new { Id });
            }
            else
            {
                return RedirectToPage("LegalFormOtherOrganisation", new { Id });
            }
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }
}

public class LegalForm
{
    public const string TempDataKey = "LegalFormTempData";
    public Guid? SupplierInformationOrganisationId { get; set; }
    public bool? RegisteredUnderAct2006 { get; set; }
    public string? RegisteredLegalForm { get; set; }
    public string? LawRegistered { get; set; }
    public DateTimeOffset? RegistrationDate { get; set; }
}