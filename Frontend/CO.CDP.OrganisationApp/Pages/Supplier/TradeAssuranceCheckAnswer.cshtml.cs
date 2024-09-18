using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

public class TradeAssuranceCheckAnswerModel(
    IOrganisationClient organisationClient,
    ITempDataService tempDataService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public TradeAssurance? TradeAssurance { get; set; }

    public IActionResult OnGet()
    {
        TradeAssurance = tempDataService.PeekOrDefault<TradeAssurance>(TradeAssurance.TempDataKey);
        if (!Validate(TradeAssurance))
        {
            return RedirectToPage("TradeAssuranceBody", new { Id });
        }
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var ta = tempDataService.GetOrDefault<TradeAssurance>(TradeAssurance.TempDataKey);
        if (!Validate(ta))
        {
            return RedirectToPage("TradeAssuranceBody", new { Id });
        }

        await organisationClient.UpdateSupplierTradeAssurance(Id,
            new CO.CDP.Organisation.WebApiClient.TradeAssurance(
                id: ta.Id,
                awardedByPersonOrBodyName: ta.AwardedByPersonOrBodyName,
                referenceNumber: ta.ReferenceNumber,
                dateAwarded: ta.DateAwarded!.Value));

        tempDataService.Remove(TradeAssurance.TempDataKey);

        return RedirectToPage("TradeAssuranceSummary", new { Id });
    }

    private static bool Validate(TradeAssurance tradeAssurance)
    {
        return !string.IsNullOrEmpty(tradeAssurance.AwardedByPersonOrBodyName)
            && !string.IsNullOrEmpty(tradeAssurance.ReferenceNumber)
            && tradeAssurance.DateAwarded.HasValue;
    }
}