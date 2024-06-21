using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class TradeAssuranceRemoveConfirmationModel(
    IOrganisationClient organisationClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid TradeAssuranceId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Please confirm remove trade assurance option")]
    public bool? ConfirmRemove { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var ta = await GetTradeAssurance(organisationClient);
        if (ta == null) return Redirect("/page-not-found");

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (ConfirmRemove == true)
        {
            var ta = await GetTradeAssurance(organisationClient);
            if (ta == null) return Redirect("/page-not-found");

            await organisationClient.DeleteSupplierTradeAssurance(Id, TradeAssuranceId);
        }

        return RedirectToPage("TradeAssuranceSummary", new { Id });
    }

    private async Task<Organisation.WebApiClient.TradeAssurance?> GetTradeAssurance(IOrganisationClient organisationClient)
    {
        try
        {
            var supplierInfo = await organisationClient.GetOrganisationSupplierInformationAsync(Id);
            return supplierInfo.TradeAssurances.FirstOrDefault(ta => ta.Id == TradeAssuranceId);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }
}