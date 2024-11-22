using System.ComponentModel.DataAnnotations;
using CO.CDP.Localization;
using CO.CDP.Mvc.Validation;
using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Support;

public class OrganisationApprovalModel(
    IOrganisationClient organisationClient,
    ISession session) : LoggedInUserAwareModel(session)
{
    public OrganisationWebApiClient.Organisation? OrganisationDetails { get; set; }

    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Support_OrganisationApproval_ValidationErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public bool Approval { get; set; }

    [BindProperty]
    [RequiredIf(nameof(Approval), false, ErrorMessageResourceName = nameof(StaticTextResource.Support_OrganisationApproval_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? Comments { get; set; }

    public async Task<IActionResult> OnGet(Guid organisationId)
    {
        try
        {
            OrganisationDetails = await organisationClient.GetOrganisationAsync(organisationId);
            return Page();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }

    public async Task<IActionResult> OnPost(Guid organisationId)
    {
        if (!ModelState.IsValid)
        {
            OrganisationDetails = await organisationClient.GetOrganisationAsync(organisationId);
            return Page();
        }

        if (UserDetails.PersonId != null)
        {
            SupportOrganisationInfo orgInfo = new SupportOrganisationInfo(
                approved: Approval,
                comment: Comments ?? "",
                reviewedById: UserDetails.PersonId.Value
            );

            await organisationClient.SupportUpdateOrganisationAsync(organisationId, new SupportUpdateOrganisation(
                orgInfo,
                SupportOrganisationUpdateType.Review
            ));
        }
        else
        {
            return Redirect("/");
        }

        return RedirectToPage("Organisations", new { type = "buyer" });
    }
}