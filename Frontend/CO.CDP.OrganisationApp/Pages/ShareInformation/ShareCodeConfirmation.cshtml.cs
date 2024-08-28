using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.ShareInformation;

public class ShareCodeConfirmationModel() : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid OrganisationId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid FormId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid SectionId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ShareCode { get; set; }

}