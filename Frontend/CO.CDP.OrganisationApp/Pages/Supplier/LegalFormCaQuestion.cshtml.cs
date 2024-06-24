using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class LegalFormCaQuestionModel(
    IOrganisationClient organisationClient,
    ITempDataService tempDataService) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Please select an option")]
    public bool? RegisteredOnCh { get; set; }


    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public async Task<IActionResult> OnGet(Guid id)
    {
        try
        {
            var getSupplierInfoTask = await organisationClient.GetOrganisationSupplierInformationAsync(id);            

            if (getSupplierInfoTask.CompletedLegalForm)
            {
                var legalForm = getSupplierInfoTask.LegalForm;
                RegisteredOnCh = legalForm.RegisteredUnderAct2006;                
            }
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
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

            lf.RegisteredUnderAct2006 = RegisteredOnCh;

            if (supplierInfo.CompletedLegalForm)
            {
                var legalForm = supplierInfo.LegalForm;                
                lf.RegisteredLegalForm = legalForm.RegisteredLegalForm;
                lf.LawRegistered = legalForm.LawRegistered;
                lf.RegistrationDate = legalForm.RegistrationDate;                
                lf.SupplierInformationOrganisationId = Id;
            }

            tempDataService.Put(LegalForm.TempDataKey, lf);

            if (RegisteredOnCh == true)
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