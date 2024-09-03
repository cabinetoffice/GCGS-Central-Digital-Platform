using CO.CDP.Mvc.Validation;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

public class SupplierOperationQuestionModel(IOrganisationClient organisationClient) : PageModel
{
    private readonly IOrganisationClient organisationClient = organisationClient;

    [BindProperty]
    [NotEmpty(ErrorMessage = "Select at least one option to proceed.")]
    [ValidOperationTypeSelection]
    public required List<OperationType>? SelectedOperationTypes { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public async Task<IActionResult> OnGet(Guid id)
    {
        try
        {
            var composed = await organisationClient.GetComposedOrganisation(id);

            if (composed.SupplierInfo.CompletedOperationType)
            {
                SelectedOperationTypes = composed.SupplierInfo.OperationTypes.ToList();
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

            await organisationClient.UpdateOperationType(Id, SelectedOperationTypes);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return RedirectToPage("SupplierBasicInformation", new { Id });
    }
}