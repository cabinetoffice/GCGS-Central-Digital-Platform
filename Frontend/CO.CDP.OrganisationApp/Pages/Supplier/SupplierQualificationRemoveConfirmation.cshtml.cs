using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

public class SupplierQualificationRemoveConfirmationModel(
IOrganisationClient organisationClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid QualificationId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Please confirm remove qualification option")]
    public bool? ConfirmRemove { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var qa = await GetQualification(organisationClient);
        if (qa == null)
            return Redirect("/page-not-found");

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (ConfirmRemove == true)
        {
            var qa = await GetQualification(organisationClient);
            if (qa == null)
                return Redirect("/page-not-found");

            await organisationClient.DeleteSupplierQualification(Id, QualificationId);
        }

        return RedirectToPage("SupplierQualificationSummary", new { Id });
    }

    private async Task<CO.CDP.Organisation.WebApiClient.Qualification?> GetQualification(IOrganisationClient organisationClient)
    {
        try
        {
            var supplierInfo = await organisationClient.GetOrganisationSupplierInformationAsync(Id);
            return supplierInfo.Qualifications.FirstOrDefault(qa => qa.Id == QualificationId);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }
}