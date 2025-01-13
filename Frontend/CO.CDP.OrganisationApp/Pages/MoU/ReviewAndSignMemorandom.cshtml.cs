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

    public Mou? mouSignatureLatest { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            mouSignatureLatest = await organisationClient.GetLatestMouAsync();

            return Page();
        }
        catch (OrganisationApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        SignedInPersonId = UserDetails.PersonId;

        if (mouSignatureLatest != null && SignTheAgreement)
        {
            var signMouRequest = new SignMouRequest
            (
                createdById: (Guid)SignedInPersonId!,
                jobTitle: JobTitleValue,
                mouId: mouSignatureLatest.Id,
                name: Name
            );
            try
            {
                OrganisationDetails = await organisationClient.GetOrganisationAsync(Id);

                if (OrganisationDetails != null)
                {
                    await organisationClient.SignOrganisationMouAsync(OrganisationDetails.Id, signMouRequest);
                }

            }
            catch
            {
                return Redirect("/page-not-found");
            }
        }
        else
        {
            return Page();
        }

        return RedirectToPage("ReviewAndSignMemorandomComplete", new { Id });
    }
}
