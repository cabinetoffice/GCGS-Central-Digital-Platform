using CO.CDP.DataSharing.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.ShareInformation;

public class ShareCodeConfirmationModel(IDataSharingClient dataSharingClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid OrganisationId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid FormId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid SectionId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var sharRequest = new ShareRequest(FormId, OrganisationId);
            var getDataSharingTask = await dataSharingClient.CreateSharedDataAsync(sharRequest);

        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
        return Page();
    }
}
