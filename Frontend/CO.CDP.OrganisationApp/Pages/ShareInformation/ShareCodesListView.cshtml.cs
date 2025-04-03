using CO.CDP.DataSharing.WebApiClient;
using CO.CDP.Forms.WebApiClient;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Pages.Supplier;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Mime;
using static System.Net.Mime.MediaTypeNames;
using SharedConsentDetails = CO.CDP.OrganisationApp.Models.SharedConsentDetails;

namespace CO.CDP.OrganisationApp.Pages.ShareInformation;

[Authorize(Policy = OrgScopeRequirement.Viewer)]
public class ShareCodesListViewModel(
    IDataSharingClient dataSharingClient,
    IOrganisationClient organisationClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid OrganisationId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid FormId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid SectionId { get; set; }

    [BindProperty]
    public List<SharedConsentDetails>? SharedConsentDetailsList { get; set; }

    public bool IsInformalConsortium { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            var sharedCodesList = await dataSharingClient.GetShareCodeListAsync(OrganisationId);
            var organisationDetails = await organisationClient.GetOrganisationAsync(OrganisationId);
            IsInformalConsortium = (organisationDetails.Type == CDP.Organisation.WebApiClient.OrganisationType.InformalConsortium);

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
        catch (DataSharing.WebApiClient.ApiException ex) when (ex.StatusCode == 404)
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

        FileResponse fileResponse;

        try
        {
            fileResponse = await dataSharingClient.GetSharedDataFileAsync(shareCode);
        }
        catch (DataSharing.WebApiClient.ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        var contentDisposition = fileResponse.Headers["Content-Disposition"].FirstOrDefault();
        var filename = string.IsNullOrWhiteSpace(contentDisposition) ? $"{shareCode}.pdf" : new ContentDisposition(contentDisposition).FileName;
        var contentType = fileResponse.Headers["Content-Type"].FirstOrDefault() ?? Application.Pdf;

        return File(fileResponse.Stream, contentType, filename);
    }
}