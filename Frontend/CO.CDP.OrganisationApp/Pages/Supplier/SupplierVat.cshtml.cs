using CO.CDP.Mvc.Validation;
using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

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
            var getOrganisationTask = organisationClient.GetOrganisationAsync(id);
            var getSupplierInfoTask = organisationClient.GetOrganisationSupplierInformationAsync(id);

            await Task.WhenAll(getOrganisationTask, getSupplierInfoTask);

            if (getSupplierInfoTask.Result.CompletedVat)
            {
                var vatIdentifier = getOrganisationTask.Result.AdditionalIdentifiers.FirstOrDefault(i => i.Scheme == "VAT");
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

            await organisationClient.UpdateOrganisationAsync(Id,
                new UpdatedOrganisation
                (
                    type: OrganisationUpdateType.AdditionalIdentifiers,
                    organisation: new OrganisationInfo(
                        additionalIdentifiers: [
                            new OrganisationIdentifier(
                                id: HasVatNumber == true ? VatNumber : "",
                                legalName: organisation.Name,
                                scheme: "VAT")
                        ])
                ));
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return RedirectToPage("SupplierBasicInformation", new { Id });
    }
}