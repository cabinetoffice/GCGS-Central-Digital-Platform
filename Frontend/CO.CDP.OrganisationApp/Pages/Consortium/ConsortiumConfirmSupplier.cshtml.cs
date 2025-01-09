using CO.CDP.DataSharing.WebApiClient;
using CO.CDP.Localization;
using CO.CDP.Mvc.Validation;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Consortium;

[FeatureGate(FeatureFlags.Consortium)]
public class ConsortiumConfirmSupplierModel(
    IOrganisationClient organisationClient,
    IDataSharingClient dataSharingClient,
    ITempDataService tempDataService) : PageModel
{
    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Global_SelectYesOrNo), ErrorMessageResourceType = typeof(StaticTextResource))]
    public bool? ConfirmSupplier { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public string? ConsortiumName { get; set; }
    public string? Heading { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            var organisation = await organisationClient.GetOrganisationAsync(Id);
            if (organisation == null) return Redirect("/page-not-found");

            var sc = tempDataService.PeekOrDefault<ConsortiumSharecode>(ConsortiumSharecode.TempDataKey);

            ConsortiumName = organisation.Name;
            Heading = string.Format(StaticTextResource.Consortium_ConsortiumConfirmSupplier_Heading, sc.SharecodeOrganisationName);
            return Page();

        }
        catch (OrganisationWebApiClient.ApiException<OrganisationWebApiClient.ProblemDetails> aex)
        {
            ApiExceptionMapper.MapApiExceptions(aex, ModelState);
            return Page();
        }
        catch (OrganisationWebApiClient.ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }

    public async Task<IActionResult> OnPost()
    {
        var organisation = await organisationClient.GetOrganisationAsync(Id);
        if (organisation == null) return Redirect("/page-not-found");

        var sc = tempDataService.PeekOrDefault<ConsortiumSharecode>(ConsortiumSharecode.TempDataKey);

        ConsortiumName = organisation.Name;
        Heading = string.Format(StaticTextResource.Consortium_ConsortiumConfirmSupplier_Heading, sc.SharecodeOrganisationName);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            //var cp = new OrganisationWebApiClient.OrganisationContactPoint(
            //        name: organisation.ContactPoint.Name,
            //        email: EmailAddress,
            //        telephone: organisation.ContactPoint.Telephone,
            //        url: organisation.ContactPoint.Url?.ToString());

            //await organisationClient.UpdateOrganisationEmail(Id, cp);

            var csc = tempDataService.PeekOrDefault<ConsortiumSharecode>(ConsortiumSharecode.TempDataKey);
            if (csc == null)
            {
                ModelState.AddModelError(string.Empty, ErrorMessagesList.PayLoadIssueOrNullAurgument);
                return Page();
            }
            //Save to database
        }
        catch (CO.CDP.Organisation.WebApiClient.ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return RedirectToPage("ConsortiumOverview", new { Id });
    }
}
