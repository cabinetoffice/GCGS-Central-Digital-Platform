using CO.CDP.DataSharing.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.ShareInformation;

public class ShareCodeDetailsModel(IDataSharingClient dataSharingClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid OrganisationId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid FormId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid SectionId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ShareCode { get; set; }

    public SharedConsentDetails? SharedConsentDetails { get; set; }

    public async Task<IActionResult> OnGet(string shareCode)
    {
        try
        {
            SharedConsentDetails = await dataSharingClient.GetShareCodeDetailsAsync(OrganisationId, shareCode);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
        return Page();
    }
}
