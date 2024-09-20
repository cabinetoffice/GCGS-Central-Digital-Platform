using CO.CDP.DataSharing.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Mime;
using static System.Net.Mime.MediaTypeNames;

namespace CO.CDP.OrganisationApp.Pages.ShareInformation;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class ShareCodeConfirmationModel(IDataSharingClient dataSharingClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid OrganisationId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid FormId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid SectionId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ShareCode { get; set; }

    public async Task<IActionResult> OnGetDownload()
    {
        if (string.IsNullOrEmpty(ShareCode))
        {
            return Redirect("/page-not-found");
        }

        var fileResponse = await dataSharingClient.GetSharedDataPdfAsync(ShareCode);

        if (fileResponse == null)
        {
            return Redirect("/page-not-found");
        }

        var contentDisposition = fileResponse.Headers["Content-Disposition"].FirstOrDefault();
        var filename = string.IsNullOrWhiteSpace(contentDisposition) ? $"{ShareCode}.pdf" : new ContentDisposition(contentDisposition).FileName;
        var contentType = fileResponse.Headers["Content-Type"].FirstOrDefault() ?? Application.Pdf;

        return File(fileResponse.Stream, contentType, filename);
    }
}