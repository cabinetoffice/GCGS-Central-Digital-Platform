using CO.CDP.DataSharing.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.ShareInformation;

public class ShareCodeConfirmationModel(
    IDataSharingClient dataSharingClient,
    ITempDataService tempDataService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid OrganisationId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid FormId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid SectionId { get; set; }

    [BindProperty]
    public string? ShareCode { get; set; }
    private string ShareDataStateKey => $"ShareData_{OrganisationId}_{FormId}_{SectionId}_Page";

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var sharingDataDetails = await dataSharingClient.CreateSharedDataAsync(new ShareRequest(FormId, OrganisationId));
            ShareCode = sharingDataDetails.ShareCode;
            tempDataService.Put(ShareDataStateKey, true);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
        return Page();
    }
}
