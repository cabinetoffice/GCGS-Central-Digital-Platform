using CO.CDP.Localization;
using CO.CDP.Mvc.Validation;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class SupplierVatQuestionModel(IOrganisationClient organisationClient) : PageModel
{
    const string VatSchemeName = "VAT";

    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Global_SelectAnOption), ErrorMessageResourceType = typeof(StaticTextResource))]
    public bool? HasVatNumber { get; set; }

    [BindProperty]
    [RequiredIf(nameof(HasVatNumber), true, ErrorMessageResourceName = nameof(StaticTextResource.Supplier_VatNumber_Error), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? VatNumber { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public SupplierType? SupplierType { get; set; }

    public async Task<IActionResult> OnGet(Guid id)
    {
        try
        {
            var composed = await organisationClient.GetComposedOrganisation(id);
            SupplierType = composed.SupplierInfo.SupplierType;

            if (composed.SupplierInfo.CompletedVat)
            {
                HasVatNumber = false;
                var vatIdentifier = Helper.GetVatIdentifier(composed.Organisation);

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
        CDP.Organisation.WebApiClient.Organisation? organisation;

        try
        {
            var composed = await organisationClient.GetComposedOrganisation(Id);
            SupplierType = composed.SupplierInfo.SupplierType;
            organisation = composed.Organisation;

            if (!ModelState.IsValid)
            {
                return Page();
            }
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        ICollection<OrganisationIdentifier> identifiers = [
                                new OrganisationIdentifier(
                                    id: HasVatNumber == true ? VatNumber?.Trim() : null,
                                    legalName: organisation.Name,
                                    scheme: VatSchemeName)];
        var existingVatIdentifier = Helper.GetVatIdentifier(organisation);

        if (HasVatNumber.GetValueOrDefault())
        {
            if ((existingVatIdentifier?.Id ?? string.Empty) != VatNumber)
            {
                await organisationClient.UpdateSupplierCompletedVat(Id);
                await organisationClient.UpdateOrganisationAdditionalIdentifiers(Id, identifiers);
            }
        }
        else
        {
            await organisationClient.UpdateSupplierCompletedVat(Id);

            if (!string.IsNullOrEmpty(existingVatIdentifier?.Id))
            {
                await organisationClient.UpdateOrganisationRemoveIdentifier(
                    organisation.Id,
                    new OrganisationIdentifier(string.Empty, organisation.Name, VatSchemeName));
            }

        }

        return RedirectToPage("SupplierBasicInformation", new { Id });
    }
}