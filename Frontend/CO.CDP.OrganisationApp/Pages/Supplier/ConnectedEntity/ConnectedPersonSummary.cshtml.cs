using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

public class ConnectedPersonSummaryModel(
    IOrganisationClient organisationClient,
    ISession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty] public ICollection<ConnectedEntityLookup> ConnectedEntities { get; set; } = [];

    [BindProperty]
    [Required(ErrorMessage = "Select yes to add another connected person")]
    public bool? HasConnectedEntity { get; set; }

    public async Task<IActionResult> OnGet(bool? selected)
    {
        try
        {
            ConnectedEntities = await organisationClient.GetConnectedEntities(Id);
            session.Remove(Session.ConnectedPersonKey);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        HasConnectedEntity = selected;

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        ConnectedEntities = await organisationClient.GetConnectedEntities(Id);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        return RedirectToPage(HasConnectedEntity == true ? "/Supplier/ConnectedEntity/ConnectedEntityDeclaration" : "/Supplier/SupplierInformationSummary", new { Id });
    }
}