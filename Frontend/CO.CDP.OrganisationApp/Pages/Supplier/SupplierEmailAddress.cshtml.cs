using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.Mvc.Validation;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using CO.CDP.OrganisationApp.Models;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class SupplierEmailAddressModel(IOrganisationClient organisationClient) : PageModel
{
    [BindProperty]
    [ModelBinder<SanitisedStringModelBinder>]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Supplier_Email_Required_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    [ValidEmailAddress(ErrorMessageResourceName = nameof(StaticTextResource.Global_Email_Invalid_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? EmailAddress { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public SupplierType? SupplierType { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            var composed = await organisationClient.GetComposedOrganisation(Id);
            SupplierType = composed.SupplierInfo.SupplierType;

            if (composed.SupplierInfo.CompletedEmailAddress)
            {
                if (composed.Organisation.ContactPoint.Email != null)
                {
                    EmailAddress = composed.Organisation.ContactPoint.Email;
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

        try
        {
            var composed = await organisationClient.GetComposedOrganisation(Id);
            SupplierType = composed.SupplierInfo.SupplierType;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            List<Task> tasks = [];

            var cp = new OrganisationContactPoint(
                    name: composed.Organisation.ContactPoint.Name,
                    email: EmailAddress,
                    telephone: composed.Organisation.ContactPoint.Telephone,
                    url: composed.Organisation.ContactPoint.Url?.ToString());

            tasks.Add(organisationClient.UpdateOrganisationContactPoint(Id, cp));

            tasks.Add(organisationClient.UpdateSupplierCompletedEmailAddress(Id));

            await Task.WhenAll(tasks);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return RedirectToPage("SupplierBasicInformation", new { Id });
    }
}