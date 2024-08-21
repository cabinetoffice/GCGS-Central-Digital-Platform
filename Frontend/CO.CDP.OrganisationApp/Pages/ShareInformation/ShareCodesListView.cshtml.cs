using CO.CDP.DataSharing.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.ShareInformation;

public class ShareCodesListViewModel(IDataSharingClient dataSharingClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid OrganisationId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid FormId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid SectionId { get; set; }

    [BindProperty]
    public SharedConsentDetails? sharedConsentDetails { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            var sharedCodesList = await dataSharingClient.GetShareCodeListAsync(OrganisationId);
            foreach (var sharedCode in sharedCodesList)
            {
                sharedConsentDetails.ShareCode = sharedCode.ShareCode;
                sharedConsentDetails.SubmittedAt = sharedCode.SubmittedAt;
            }
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
        return Page();
    }
}
