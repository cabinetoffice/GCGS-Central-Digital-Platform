using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[ValidateRegistrationStep]
public class OrganisationNameSearchModel(ISession session, IOrganisationClient organisationClient) : RegistrationStepModel(session)
{
    public override string CurrentPage => OrganisationNameSearchPage;

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.OrganisationRegistration_SearchOrganisationName_Heading))]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Global_PleaseSelect), ErrorMessageResourceType = typeof(StaticTextResource))]
    public required string OrganisationId { get; set; }

    public string? OrganisationName { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }
    public required List<(CO.CDP.Organisation.WebApiClient.Organisation Organisation, BuyerInformation BuyerInfo)> OrganisationsWithBuyerInfo { get; set; }

    public async Task<IActionResult> OnGet()
    {
        if (RegistrationDetails.OrganisationType != Constants.OrganisationType.Buyer
            || RegistrationDetails.OrganisationName?.Length < 3)
        {
            return RedirectToPage("OrganisationEmail");
        }

        try
        {
            await FindMatchingOrgs(organisationClient);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return RedirectToPage("OrganisationEmail");
        }

        return Page();
    }

    private async Task FindMatchingOrgs(IOrganisationClient organisationClient)
    {
        OrganisationName = RegistrationDetails.OrganisationName;
        
        var matchingOrganisations = await organisationClient.SearchOrganisationAsync(RegistrationDetails.OrganisationName, Constants.OrganisationType.Buyer.ToString(), 10);

        OrganisationsWithBuyerInfo = (await Task.WhenAll(
            matchingOrganisations.Select(async organisation =>
            {
                var buyerInfo = await organisationClient.GetOrganisationBuyerInformationAsync(organisation.Id);
                return (organisation, buyerInfo);
            }))).ToList();
    }

    public async Task<IActionResult> OnPost()
    {
        try
        {
            await FindMatchingOrgs(organisationClient);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return RedirectToPage("OrganisationEmail");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!string.IsNullOrEmpty(OrganisationId) && OrganisationId != "None" && Guid.TryParse(OrganisationId, out var organisationId))
        {
            var organisation = await organisationClient.GetOrganisationAsync(organisationId);

            if (organisation != null)
            {
                var identifier = $"{organisation.Identifier.Scheme}:{organisation.Identifier.Id}";
                return Redirect($"/registration/{identifier}/join-organisation");
            }
        }

        if (RedirectToSummary == true)
        {
            return RedirectToPage("OrganisationDetailsSummary");
        }
        else
        {
            return RedirectToPage("OrganisationEmail");
        }
    }
}