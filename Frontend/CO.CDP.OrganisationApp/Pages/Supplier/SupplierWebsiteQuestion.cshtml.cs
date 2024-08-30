using CO.CDP.Mvc.Validation;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

public class SupplierWebsiteQuestionModel(IOrganisationClient organisationClient) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Please select an option")]
    public bool? HasWebsiteAddress { get; set; }

    [BindProperty]
    [RequiredIf(nameof(HasWebsiteAddress), true, ErrorMessage = "Please enter the Website address.")]
    [Url]
    public string? WebsiteAddress { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public async Task<IActionResult> OnGet(Guid id)
    {
        try
        {
            var composed = await organisationClient.GetComposedOrganisation(id);
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
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var organisation = await organisationClient.GetOrganisationAsync(Id);

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