using CO.CDP.DataSharing.WebApiClient;
using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement.Mvc;
using System.ComponentModel.DataAnnotations;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Consortium;

[FeatureGate(FeatureFlags.Consortium)]
public class ConsortiumEnterNewSharecodeModel(
    IOrganisationClient organisationClient,
    IDataSharingClient dataSharingClient,
    ITempDataService tempDataService) : PageModel
{
    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Consortium_ConsortiumEnterSharecode_EnterSharecodeError), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? EnterSharecode { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid PartyId { get; set; }

    public string? ConsortiumName { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            var consortium = await organisationClient.GetOrganisationAsync(Id);
            
            if (consortium == null) return Redirect("/page-not-found");

            ConsortiumName = consortium.Name;

            return Page();
        }
        catch (CO.CDP.Organisation.WebApiClient.ApiException<OrganisationWebApiClient.ProblemDetails> aex)
        {
            ApiExceptionMapper.MapApiExceptions(aex, ModelState);
            return Page();
        }
        catch (CO.CDP.Organisation.WebApiClient.ApiException ex) when (ex.StatusCode == 404)
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

        try
        {
            DataSharing.WebApiClient.SupplierInformation shareCode = await dataSharingClient.GetSharedDataAsync(EnterSharecode!);

            if (PartyId != shareCode.Id)
            {
                ModelState.AddModelError(string.Empty, StaticTextResource.Consortium_ConsortiumEnterSharecode_InValidSharecodeError);
                return Page();
            }

            await organisationClient.UpdateOrganisationPartyAsync(Id, new UpdateOrganisationParty
                (
                    organisationPartyId: shareCode.Id,
                    shareCode: EnterSharecode
                ));

            var sc = new ConsortiumSharecode
            {
                Sharecode = EnterSharecode,
                SharecodeOrganisationName = shareCode.Name,
                OrganisationPartyId = shareCode.Id,
                HasBeenUpdated = true
            };

            tempDataService.Put(ConsortiumSharecode.TempDataKey, sc);

            return RedirectToPage("ConsortiumOverview", new { Id });
        }
        catch (DataSharing.WebApiClient.ApiException<DataSharing.WebApiClient.ProblemDetails> aex)
        {
            DataSharingApiExceptionMapper.MapApiExceptions(aex, ModelState);
            return Page();
        }
    }
}
