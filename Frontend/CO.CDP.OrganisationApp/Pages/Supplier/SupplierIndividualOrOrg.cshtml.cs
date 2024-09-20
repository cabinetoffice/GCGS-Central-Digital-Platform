using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class SupplierIndividualOrOrgModel(IOrganisationClient organisationClient) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Select the journey you want to take")]
    public SupplierType? SupplierType { get; set; }

    [BindProperty]
    public string? Name { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }


    public async Task<IActionResult> OnGet(Guid id)
    {
        SupplierInformation? supplierInfo;
        try
        {
            supplierInfo = await organisationClient.GetOrganisationSupplierInformationAsync(id);

            SupplierType = supplierInfo.SupplierType;
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
            await organisationClient.UpdateSupplierType(Id, SupplierType!.Value);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return RedirectToPage("SupplierBasicInformation", new { Id });

    }
}