using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.OrganisationApp.Pages.MoU;

[Authorize(Policy = OrgScopeRequirement.Admin)]
public class MoUSummaryModel(IOrganisationClient organisationClient, ISession session) : LoggedInUserAwareModel(session)
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public MouSignatureLatest? Mou { get; set; }
    public async Task<IActionResult> OnGet()
    {
        try
        {
            await organisationClient.GetOrganisationAsync(Id);

            Mou = await organisationClient.GetOrganisationLatestMouSignatureAsync(Id);

            return Page();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }

    public async Task<IActionResult> OnPost()
    {
        try
        {
            await organisationClient.GetOrganisationAsync(Id);

            Mou = await organisationClient.GetOrganisationLatestMouSignatureAsync(Id);

            var absolutePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "mou-pdfs",
                Mou.Mou.FilePath
            );

            if (!System.IO.File.Exists(absolutePath))
            {
                return RedirectToPage("/page-not-found");
            }

            var contentType = "application/pdf";
            var fileName = "fts-joint-controller-agreement.pdf";

            var fileStream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read);

            return File(fileStream, contentType, fileName);

        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }
}
