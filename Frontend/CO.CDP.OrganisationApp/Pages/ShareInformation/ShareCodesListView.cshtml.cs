using CO.CDP.DataSharing.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Mime;
using static System.Net.Mime.MediaTypeNames;
using SharedConsentDetails = CO.CDP.OrganisationApp.Models.SharedConsentDetails;

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
    public List<SharedConsentDetails>? SharedConsentDetailsList { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            var sharedCodesList = await dataSharingClient.GetShareCodeListAsync(OrganisationId);

            SharedConsentDetailsList = new List<SharedConsentDetails>();

            foreach (var sharedCode in sharedCodesList)
            {
                SharedConsentDetailsList.Add(new SharedConsentDetails
                {
                    ShareCode = sharedCode.ShareCode,
                    SubmittedAt = sharedCode.SubmittedAt
                });
            }
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
        return Page();
    }
    public async Task<IActionResult> OnGetDownload(string shareCode)
    {
        if (string.IsNullOrEmpty(shareCode))
        {
            return Redirect("/page-not-found");
        }

        var fileResponse = await dataSharingClient.GetSharedDataPdfAsync(shareCode);

        if (fileResponse == null)
        {
            return Redirect("/page-not-found");
        }

        var contentDisposition = fileResponse.Headers["Content-Disposition"].FirstOrDefault();
        var filename = string.IsNullOrWhiteSpace(contentDisposition) ? $"{shareCode}.pdf" : new ContentDisposition(contentDisposition).FileName;
        var contentType = fileResponse.Headers["Content-Type"].FirstOrDefault() ?? Application.Pdf;

        return File(fileResponse.Stream, contentType, filename);
    }
}