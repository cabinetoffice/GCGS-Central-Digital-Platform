using CO.CDP.Localization;
using CO.CDP.Mvc.Validation;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class BuyerOrganisationTypeModel(IOrganisationClient organisationClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }    

    [BindProperty]
    [Required(ErrorMessage = nameof(StaticTextResource.OrganisationRegistration_BuyerOrganisationType_ValidationErrorMessage))]
    public string? BuyerOrganisationType { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.OrganisationRegistration_BuyerOrganisationType_OtherEnterType_Label))]
    [RequiredIf("BuyerOrganisationType", "Other")]
    public string? OtherValue { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            var buyerInformation = await organisationClient.GetOrganisationBuyerInformationAsync(Id);
            if (buyerInformation == null) return Redirect("/page-not-found");

            BuyerOrganisationType = buyerInformation.BuyerType;
            if (!string.IsNullOrEmpty(BuyerOrganisationType) && !BuyerTypes.Keys.Contains(BuyerOrganisationType))
            {
                OtherValue = BuyerOrganisationType;
                BuyerOrganisationType = "Other";
            }

            return Page();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
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

        var buyerInformation = await organisationClient.GetOrganisationBuyerInformationAsync(Id);
        if (buyerInformation == null) return Redirect("/page-not-found");

        try
        {
            var buyerOrganisationType = (BuyerOrganisationType == "Other" ? OtherValue : BuyerOrganisationType);

            await organisationClient.UpdateBuyerOrganisationType(Id, buyerOrganisationType: buyerOrganisationType!);
        }
        catch (ApiException<OrganisationWebApiClient.ProblemDetails> aex)
        {
            ApiExceptionMapper.MapApiExceptions(aex, ModelState);
            return Page();
        }
        catch (CO.CDP.Organisation.WebApiClient.ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
        return RedirectToPage("OrganisationOverview", new { Id });
    }

    public static Dictionary<string, string> BuyerTypes => new()
    {
        { "CentralGovernment", StaticTextResource.OrganisationRegistration_BuyerOrganisationType_CentralGovernment_Label},
        { "RegionalAndLocalGovernment", StaticTextResource.OrganisationRegistration_BuyerOrganisationType_RegionalAndLocalGovernment_Label},
        { "PublicUndertaking", StaticTextResource.OrganisationRegistration_BuyerOrganisationType_PublicUndertaking_Label},
        { "PrivateUtility", StaticTextResource.OrganisationRegistration_BuyerOrganisationType_PrivateUtility_Label}
    };
}