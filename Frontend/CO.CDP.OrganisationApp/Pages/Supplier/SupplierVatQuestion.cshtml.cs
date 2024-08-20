using CO.CDP.Mvc.Validation;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class SupplierVatQuestionModel(IOrganisationClient organisationClient) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Please select an option")]
    public bool? HasVatNumber { get; set; }

    [BindProperty]
    [RequiredIf(nameof(HasVatNumber), true, ErrorMessage = "Please enter the VAT number.")]
    public string? VatNumber { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public async Task<IActionResult> OnGet(Guid id)
    {
        try
        {
            var composed = await organisationClient.GetComposedOrganisation(id);

            if (composed.SupplierInfo.CompletedVat)
            {
                HasVatNumber = false;
                var vatIdentifier = composed.Organisation.AdditionalIdentifiers.FirstOrDefault(i => i.Scheme == "VAT");
                if (vatIdentifier != null)
                {
                    HasVatNumber = !string.IsNullOrWhiteSpace(vatIdentifier.Id);
                    VatNumber = vatIdentifier.Id;
                }
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
            var organisation = await organisationClient.GetOrganisationAsync(Id);

            ICollection<OrganisationIdentifier> identifiers = [
                                new OrganisationIdentifier(
                                    id: HasVatNumber == true ? VatNumber : null,
                                    legalName: organisation.Name,
                                    scheme: "VAT")];

            await organisationClient.UpdateOrganisationAdditionalIdentifiers(Id, identifiers);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return RedirectToPage("SupplierBasicInformation", new { Id });
    }
}