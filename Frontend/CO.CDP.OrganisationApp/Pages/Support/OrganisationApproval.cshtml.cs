using System.ComponentModel.DataAnnotations;
using CO.CDP.Localization;
using CO.CDP.Mvc.Validation;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Mvc;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Support;

public class OrganisationApprovalModel(
    IOrganisationClient organisationClient,
    ISession session,
    IFlashMessageService flashMessageService) : LoggedInUserAwareModel(session)
{
    public OrganisationWebApiClient.Organisation? OrganisationDetails { get; set; }

    public OrganisationWebApiClient.Person? AdminUser { get; set; }

    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Support_OrganisationApproval_ValidationErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public bool? Approval { get; set; }

    [BindProperty]
    [RequiredIf(nameof(Approval), false, ErrorMessageResourceName = nameof(StaticTextResource.Support_OrganisationApproval_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? Comments { get; set; }

    public ICollection<OrganisationSearchResult>? MatchingOrganisations { get; set; }

    public ICollection<OrganisationSearchResult>? MatchingOrganisationsByOrgEmail { get; set; }

    public ICollection<OrganisationSearchResult>? MatchingOrganisationsByAdminEmail { get; set; }

    public async Task<IActionResult> OnGet(Guid organisationId)
    {
        try
        {
            OrganisationDetails = await organisationClient.GetOrganisationAsync(organisationId);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        try
        {
            var organisationReviews = await organisationClient.GetOrganisationReviewsAsync(organisationId);

            if (organisationReviews.Any(x => x.Status == ReviewStatus.Approved))
            {
                flashMessageService.SetFlashMessage(
                    FlashMessageType.Important,
                    heading: StaticTextResource.Support_OrganisationApproval_AlreadyApproved_FlashMessage,
                    htmlParameters: new() { ["organisationName"] = OrganisationDetails.Name }
                );

                return RedirectToPage("Organisations", new { type = "buyer" });
            }
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            // If there's no reviews, we just carry on
        }

        var persons = await organisationClient.GetOrganisationPersonsAsync(organisationId);

        AdminUser = persons.FirstOrDefault(p => p.Scopes.Contains(OrganisationPersonScopes.Admin));

        try
        {
            MatchingOrganisations =
                await organisationClient.SearchOrganisationAsync(OrganisationDetails.Name, "buyer", 3, 0.3, false);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            MatchingOrganisations = [];
        }

        try
        {
            MatchingOrganisationsByOrgEmail = await organisationClient.FindOrganisationsByOrganisationEmailAsync(OrganisationDetails.ContactPoint.Email, "buyer", 10);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            MatchingOrganisationsByOrgEmail = [];
        }

        try
        {
            MatchingOrganisationsByAdminEmail = await organisationClient.FindOrganisationsByAdminEmailAsync(AdminUser?.Email, "buyer", 10);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            MatchingOrganisationsByAdminEmail = [];
        }

        return Page();
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
                approved: Approval ?? false,
                comment: Comments ?? "",
                reviewedById: UserDetails.PersonId.Value,
                additionalIdentifiers: null
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