using CO.CDP.DataSharing.WebApiClient;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Mime;
using static System.Net.Mime.MediaTypeNames;

namespace CO.CDP.OrganisationApp.Pages.ShareInformation;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class ShareCodeConfirmationModel(
    IDataSharingClient dataSharingClient,
    IOrganisationClient organisationClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid OrganisationId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid FormId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid SectionId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ShareCode { get; set; }

    public bool IsInformalConsortium { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var organisationDetails = await organisationClient.GetOrganisationAsync(OrganisationId);
        IsInformalConsortium = (organisationDetails?.Type == CDP.Organisation.WebApiClient.OrganisationType.InformalConsortium);

        return Page();
    }

    public async Task<IActionResult> OnGetDownload()
    {
        if (string.IsNullOrEmpty(ShareCode))
        {
            return Redirect("/page-not-found");
        }

        try
        {
            FileResponse fileResponse = await dataSharingClient.GetSharedDataFileAsync(ShareCode);

            var contentDisposition = fileResponse.Headers["Content-Disposition"].FirstOrDefault();
            var filename = string.IsNullOrWhiteSpace(contentDisposition) ? $"{ShareCode}.pdf" : new ContentDisposition(contentDisposition).FileName;
            var contentType = fileResponse.Headers["Content-Type"].FirstOrDefault() ?? Application.Pdf;

            return File(fileResponse.Stream, contentType, filename);
        }
        catch (Exception ex)
         when ((ex is CO.CDP.Organisation.WebApiClient.ApiException oex && oex.StatusCode == 404)
            || (ex is DataSharing.WebApiClient.ApiException wex && wex.StatusCode == 404))
        {
            return Redirect("/page-not-found");
        }
    }
}