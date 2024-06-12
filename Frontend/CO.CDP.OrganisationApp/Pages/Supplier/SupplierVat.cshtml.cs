using CO.CDP.Mvc.Validation;
using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages;

[AuthorisedSession]
public class SupplierVatModel(
    ISession session,
    IOrganisationClient organisationClient) : LoggedInUserAwareModel
{
    public override ISession SessionContext => session;

    [BindProperty]
    [Required(ErrorMessage = "Please select an option")]
    public bool? HasVatNumber { get; set; }

    [BindProperty]
    [RequiredIf(nameof(HasVatNumber), true, ErrorMessage = "Please enter the VAT number.")]
    public string? VatNumber { get; set; }

    [BindProperty]
    public Guid Id { get; set; }

    public async Task<IActionResult> OnGet(Guid id)
    {
        try
        {
            var details = await organisationClient.GetOrganisationSupplierInformationAsync(id);
            if (details.CompletedVat)
            {
                HasVatNumber = !string.IsNullOrWhiteSpace(details.VatNumber);
                VatNumber = details.VatNumber;
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
            await organisationClient.UpdateSupplierInformationAsync(Id,
                new UpdateSupplierInformation
                (
                    type: SupplierInformationUpdateType.Vat,
                    supplierInformation: new SupplierInfo(
                        supplierType: null,
                        hasVatNumber: HasVatNumber,
                        vatNumber: VatNumber)
                ));
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return RedirectToPage("SupplierBasicInformation", new { Id });
    }
}