using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

public class TradeAssuranceSummaryModel(
    IOrganisationClient organisationClient,
    ITempDataService tempDataService) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Please select an option")]
    public bool? HasTradeAssurance { get; set; }

    [BindProperty]
    public ICollection<CO.CDP.Organisation.WebApiClient.TradeAssurance> TradeAssurances { get; set; } = [];

    [BindProperty]
    public bool CompletedTradeAssurance { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public async Task<IActionResult> OnGet(bool? selected)
    {
        try
        {
            var supplierInfo = await organisationClient.GetOrganisationSupplierInformationAsync(Id);
            TradeAssurances = supplierInfo.TradeAssurances;
            CompletedTradeAssurance = supplierInfo.CompletedTradeAssurance;
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        HasTradeAssurance = selected;
        return Page();
    }

    public async Task<IActionResult> OnGetChange(Guid assuranceId)
    {
        try
        {
            var supplierInfo = await organisationClient.GetOrganisationSupplierInformationAsync(Id);
            var tradeAssurance = supplierInfo.TradeAssurances.FirstOrDefault(ta => ta.Id == assuranceId);
            if (tradeAssurance == null)
            {
                return Redirect("/page-not-found");
            }

            tempDataService.Put(TradeAssurance.TempDataKey, new TradeAssurance
            {
                Id = assuranceId,
                AwardedByPersonOrBodyName = tradeAssurance.AwardedByPersonOrBodyName,
                ReferenceNumber = tradeAssurance.ReferenceNumber,
                DateAwarded = tradeAssurance.DateAwarded
            });
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return RedirectToPage("TradeAssuranceBody", new { Id });
    }

    public async Task<IActionResult> OnPost()
    {
        var supplierInfo = await organisationClient.GetOrganisationSupplierInformationAsync(Id);
        TradeAssurances = supplierInfo.TradeAssurances;
        CompletedTradeAssurance = supplierInfo.CompletedTradeAssurance;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // When no trade assurance selected
        if (supplierInfo.CompletedTradeAssurance == false && HasTradeAssurance == false)
        {
            await organisationClient.UpdateSupplierTradeAssurance(Id);
        }

        if (HasTradeAssurance == true)
        {
            tempDataService.Put(TradeAssurance.TempDataKey, new TradeAssurance());
        }

        return RedirectToPage(HasTradeAssurance == true ? "TradeAssuranceBody" : "SupplierBasicInformation", new { Id });
    }
}