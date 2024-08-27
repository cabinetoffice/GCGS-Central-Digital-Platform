using CO.CDP.EntityVerificationClient;
using CO.CDP.Mvc.Validation;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class SupplierVatQuestionModel(IOrganisationClient organisationClient,
    IPponClient pponClient, IHttpContextAccessor httpContextAccessor) : PageModel
{
    const string VatSchemeName = "VAT";

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
                var vatIdentifier = composed.Organisation.AdditionalIdentifiers.FirstOrDefault(i => i.Scheme == VatSchemeName);
                if (vatIdentifier != null)
                {
                    HasVatNumber = !string.IsNullOrWhiteSpace(vatIdentifier.Id);
                    VatNumber = vatIdentifier.Id;
                }
            }
        }
        catch (Organisation.WebApiClient.ApiException ex) when (ex.StatusCode == 404)
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

        Organisation.WebApiClient.Organisation? organisation;
        try
        {
            organisation = await organisationClient.GetOrganisationAsync(Id);
        }
        catch (Organisation.WebApiClient.ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        ICollection<OrganisationIdentifier> identifiers = [
                                new OrganisationIdentifier(
                                    id: HasVatNumber == true ? VatNumber : null,
                                    legalName: organisation.Name,
                                    scheme: VatSchemeName)];

        if (HasVatNumber.GetValueOrDefault())
        {
            try
            {
                await LookupOrganisationAsync();
            }
            catch (Exception orgApiException) when (orgApiException is Organisation.WebApiClient.ApiException && ((Organisation.WebApiClient.ApiException)orgApiException).StatusCode == 404)
            {
                try
                {
                    await LookupEntityVerificationAsync();
                }
                catch (Exception evApiException) when (evApiException is EntityVerificationClient.ApiException && ((EntityVerificationClient.ApiException)evApiException).StatusCode == 404)
                {
                    await organisationClient.UpdateOrganisationAdditionalIdentifiers(Id, identifiers);
                    return RedirectToPage("SupplierBasicInformation", new { Id });
                }
                catch
                {
                    return RedirectToPage("/Registration/OrganisationRegistrationUnavailable", SetRoute());
                }
            }
        }
        else
        {
            return RedirectToPage("SupplierBasicInformation", new { Id });
        }

        return RedirectToPage("/Registration/OrganisationAlreadyRegistered", SetRoute());
    }

    private RouteValueDictionary SetRoute()
    {
        return new RouteValueDictionary
        {
            { "backLink", httpContextAccessor!.HttpContext!.Request.Path }
        };
    }

    private async Task<Organisation.WebApiClient.Organisation> LookupOrganisationAsync()
    {
        return await organisationClient.LookupOrganisationAsync(string.Empty,
                    $"{VatSchemeName}:{VatNumber}");
    }

    private async Task<ICollection<EntityVerificationClient.Identifier>> LookupEntityVerificationAsync()
    {
        return await pponClient.GetIdentifiersAsync($"{VatSchemeName}:{VatNumber}");
    }
}