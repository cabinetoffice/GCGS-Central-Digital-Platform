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

    [BindProperty(SupportsGet = true)]
    public string? ShareCode { get; set; }

    public async Task<IActionResult> OnGetDownload()
    {
        if (string.IsNullOrEmpty(ShareCode))
        {
            return NotFound("ShareCode is required to download the file.");
        }

        try
        {
            var fileResponse = await dataSharingClient.GetSharedDataPdfAsync(ShareCode);

            if (fileResponse == null || fileResponse.Stream.Length == 0)
            {
                return NotFound("Unable to generate the PDF.");
            }

            return File(fileResponse.Stream, "application/pdf", $"{ShareCode}.pdf");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while generating the PDF: {ex.Message}");
        }
    }

}