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
    [DisplayName(nameof(StaticTextResource.OrganisationRegistration_EnterOrganisationName_Heading))]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.OrganisationRegistration_EnterOrganisationName_Heading), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string OrganisationId { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public ICollection<CO.CDP.Organisation.WebApiClient.Organisation> MatchingOrganisations;


    public async Task<IActionResult> OnGet()
    {
        if(RegistrationDetails.OrganisationType != Constants.OrganisationType.Buyer
            || RegistrationDetails.OrganisationName?.Length < 3)
        {
            return RedirectToPage("OrganisationEmail");
        }

        try
        {
            MatchingOrganisations = await organisationClient.SearchOrganisationAsync(RegistrationDetails.OrganisationName, Constants.OrganisationType.Buyer.ToString());
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return RedirectToPage("OrganisationEmail");
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
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