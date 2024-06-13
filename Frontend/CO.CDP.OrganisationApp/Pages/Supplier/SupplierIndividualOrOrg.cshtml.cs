using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[AuthorisedSession]
public class SupplierIndividualOrOrgModel(
    ISession session,
    IOrganisationClient organisationClient) : LoggedInUserAwareModel
{
    public override ISession SessionContext => session;

    [BindProperty]
    [Required(ErrorMessage = "Select the journey you want to take")]
    public SupplierType? SupplierType { get; set; }

    [BindProperty]
    public string? Name { get; set; }

    [BindProperty]
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
            await organisationClient.UpdateSupplierInformationAsync(Id,
                new UpdateSupplierInformation
                (
                    type: SupplierInformationUpdateType.SupplierType,
                    supplierInformation: new SupplierInfo(SupplierType)
                ));
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return RedirectToPage("SupplierBasicInformation", new { Id });

    }
}