using CO.CDP.Localization;
using CO.CDP.Mvc.Validation;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class SupplierWebsiteQuestionModel(IOrganisationClient organisationClient) : PageModel
{
    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Supplier_WebsiteQuestion_ErrorSelectOption), ErrorMessageResourceType = typeof(StaticTextResource))]
    public bool? HasWebsiteAddress { get; set; }

    [BindProperty]
    [RequiredIf(nameof(HasWebsiteAddress), true, ErrorMessageResourceName = nameof(StaticTextResource.Supplier_WebsiteAddress_ErrorEnterWebsiteAddress), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? WebsiteAddress { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public SupplierType? SupplierType { get; set; }

    public async Task<IActionResult> OnGet(Guid id)
    {
        try
        {
            var composed = await organisationClient.GetComposedOrganisation(id);
            SupplierType = composed.SupplierInfo.SupplierType;

            if (composed.SupplierInfo.CompletedWebsiteAddress)
            {
                HasWebsiteAddress = false;
                if (composed.Organisation.ContactPoint.Url != null)
                {
                    HasWebsiteAddress = true;
                    WebsiteAddress = composed.Organisation.ContactPoint.Url.ToString();
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
        if (!string.IsNullOrWhiteSpace(WebsiteAddress))
        {
            WebsiteAddress = WebsiteAddress.Trim();

            if (!Regex.IsMatch(WebsiteAddress, "^(http|https)://") && !WebsiteAddress.StartsWith("//"))
            {
                WebsiteAddress = $"http://{WebsiteAddress}";
            }

            if (!Uri.TryCreate(WebsiteAddress, UriKind.Absolute, out _))
            {
                ModelState.AddModelError(nameof(WebsiteAddress), StaticTextResource.Supplier_WebsiteQuestion_ErrorInvalidUrl);
            }
        }

        try
        {
            var composed = await organisationClient.GetComposedOrganisation(Id);
            SupplierType = composed.SupplierInfo.SupplierType;
            var organisation = composed.Organisation;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            List<Task> tasks = [];

            var cp = new OrganisationContactPoint(
                    name: organisation.ContactPoint.Name,
                    email: organisation.ContactPoint.Email,
                    telephone: organisation.ContactPoint.Telephone,
                    url: HasWebsiteAddress == true ? WebsiteAddress : null);

            tasks.Add(organisationClient.UpdateOrganisationContactPoint(Id, cp));

            tasks.Add(organisationClient.UpdateSupplierCompletedWebsiteAddress(Id));

            await Task.WhenAll(tasks);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return RedirectToPage("SupplierBasicInformation", new { Id });
    }
}