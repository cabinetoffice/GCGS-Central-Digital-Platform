using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using OrganisationApiException = CO.CDP.Organisation.WebApiClient.ApiException;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.MoU;

public class ReviewAndSignMemorandomModel(IOrganisationClient organisationClient, ISession session) : LoggedInUserAwareModel(session)
{

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.MoU_ReviewAndSign_Confirm_Title))]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.MoU_ReviewAndSign_SelectCheckbox_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public bool SignTheAgreement { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.MoU_ReviewAndSign_Job_Title))]
    [Required(ErrorMessage = nameof(StaticTextResource.MoU_ReviewAndSign_Job_Title_ErrorMessage))]
    public string? JobTitleValue { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.MoU_ReviewAndSign_Name))]
    [Required(ErrorMessage = nameof(StaticTextResource.MoU_ReviewAndSign_Name_ErrorMessage))]
    public string? Name { get; set; }

    public OrganisationWebApiClient.Organisation? OrganisationDetails { get; set; }
    public Guid? SignedInPersonId { get; set; }

    public Mou? MouLatest { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        try
        {
            SignedInPersonId = UserDetails.PersonId;

            if (await TryFetchLatestMou())
            {
                if (SignTheAgreement)
                {
                    var signMouRequest = new SignMouRequest
                    (
                        createdById: (Guid)SignedInPersonId!,
                        jobTitle: JobTitleValue,
                        mouId: MouLatest!.Id,
                        name: Name
                    );

                    OrganisationDetails = await organisationClient.GetOrganisationAsync(Id);

                    if (OrganisationDetails != null)
                    {
                        await organisationClient.SignOrganisationMouAsync(OrganisationDetails.Id, signMouRequest);
                    }
                }
                else
                {
                    return Page();
                }
            }
        }
        catch
        {
            return RedirectToPage("/page-not-found");
        }

        return RedirectToPage("ReviewAndSignMemorandomComplete", new { Id });
    }

    public async Task<IActionResult> OnGetDownload(string mouid = "")
    {
        try
        {
            var filePath = string.IsNullOrEmpty(mouid)
                ? (await TryFetchLatestMou() ? MouLatest?.FilePath : null)
                : (await organisationClient.GetOrganisationMouSignatureAsync(Id, Guid.Parse(mouid))).Mou.FilePath;

            if (string.IsNullOrEmpty(filePath))
                return RedirectToPage("/page-not-found");

            var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "mou-pdfs", filePath);

            if (!System.IO.File.Exists(absolutePath))
                return RedirectToPage("/page-not-found");

            var contentType = "application/pdf";
            var fileName = "fts-joint-controller-agreement.pdf";

            return File(new FileStream(absolutePath, FileMode.Open, FileAccess.Read, FileShare.Read), contentType, fileName);
        }
        catch (OrganisationApiException ex) when (ex.StatusCode == 404)
        {
            return RedirectToPage("/page-not-found");
        }
        catch
        {
            return RedirectToPage("/page-not-found");
        }
    }

    private async Task<bool> TryFetchLatestMou()
    {
        try
        {
            MouLatest = await organisationClient.GetLatestMouAsync();
            return true;
        }
        catch (OrganisationApiException ex) when (ex.StatusCode == 404)
        {
            MouLatest = null;
            return false;
        }
    }

}
