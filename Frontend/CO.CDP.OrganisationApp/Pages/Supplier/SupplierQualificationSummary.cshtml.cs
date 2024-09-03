using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

public class SupplierQualificationSummaryModel(
    IOrganisationClient organisationClient,
    ITempDataService tempDataService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Please select an option")]
    public bool? HasQualification { get; set; }

    [BindProperty]
    public ICollection<Organisation.WebApiClient.Qualification> Qualifications { get; set; } = [];

    [BindProperty]
    public bool CompletedQualification { get; set; }


    public async Task<IActionResult> OnGet(bool? selected)
    {
        try
        {
            var supplierInfo = await organisationClient.GetOrganisationSupplierInformationAsync(Id);
            Qualifications = supplierInfo.Qualifications;
            CompletedQualification = supplierInfo.CompletedQualification;
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        HasQualification = selected;
        return Page();
    }

    public async Task<IActionResult> OnGetChange(Guid qualificationId)
    {
        try
        {
            var supplierInfo = await organisationClient.GetOrganisationSupplierInformationAsync(Id);
            var qualification = supplierInfo.Qualifications.FirstOrDefault(qa => qa.Id == qualificationId);
            if (qualification == null)
            {
                return Redirect("/page-not-found");
            }

            tempDataService.Put(Qualification.TempDataKey, new Qualification
            {
                Id = qualificationId,
                AwardedByPersonOrBodyName = qualification.AwardedByPersonOrBodyName,
                Name = qualification.Name,
                DateAwarded = qualification.DateAwarded
            });
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return RedirectToPage("SupplierQualificationAwardingBody", new { Id });
    }

    public async Task<IActionResult> OnPost()
    {
        var supplierInfo = await organisationClient.GetOrganisationSupplierInformationAsync(Id);
        Qualifications = supplierInfo.Qualifications;
        CompletedQualification = supplierInfo.CompletedQualification;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (supplierInfo.CompletedQualification == false && HasQualification == false)
        {
            await organisationClient.UpdateSupplierQualification(Id);
        }

        if (HasQualification == true)
        {
            tempDataService.Put(Qualification.TempDataKey, new Qualification());
        }

        return RedirectToPage(HasQualification == true ? "SupplierQualificationAwardingBody" : "SupplierBasicInformation", new { Id });
    }
}