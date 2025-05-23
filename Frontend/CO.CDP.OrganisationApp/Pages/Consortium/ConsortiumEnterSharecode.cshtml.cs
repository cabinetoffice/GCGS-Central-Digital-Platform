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
public class ConsortiumEnterSharecodeModel(
    IOrganisationClient organisationClient,
    IDataSharingClient dataSharingClient,
    ITempDataService tempDataService) : PageModel
{
    [BindProperty]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Consortium_ConsortiumEnterSharecode_EnterSharecodeError), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? EnterSharecode { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public string? ConsortiumName { get; set; }
    public async Task<IActionResult> OnGet()
    {
        try
        {
            var consortium = await organisationClient.GetOrganisationAsync(Id);
            if (consortium == null) return Redirect("/page-not-found");

            var sc = tempDataService.Get<ConsortiumSharecode>(ConsortiumSharecode.TempDataKey);
            if (sc != null)
            {
                EnterSharecode = sc.Sharecode;
            }
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

            OrganisationParties? parties;

            try
            {
                parties = await organisationClient.GetOrganisationPartiesAsync(Id);
            }
            catch (CO.CDP.Organisation.WebApiClient.ApiException ex) when (ex.StatusCode == 404)
            {
                parties = null;
            }

            if (parties != null && parties.Parties.Where(p => p.Id == shareCode.Id).Any())
            {
                ModelState.AddModelError(nameof(EnterSharecode), string.Format(StaticTextResource.Consortium_ConsortiumEnterSharecode_SharecodeAlreadyExists, shareCode.Name));
                return Page();
            }

            var sc = new ConsortiumSharecode
            {
                Sharecode = EnterSharecode,
                SharecodeOrganisationName = shareCode.Name,
                OrganisationPartyId = shareCode.Id
            };

            tempDataService.Put(ConsortiumSharecode.TempDataKey, sc);

            return RedirectToPage("ConsortiumConfirmSupplier", new { Id });
        }
        catch (CO.CDP.DataSharing.WebApiClient.ApiException<DataSharing.WebApiClient.ProblemDetails> aex)
        {
            DataSharingApiExceptionMapper.MapApiExceptions(aex, ModelState);
            return Page();
        }
    }
}

public class ConsortiumSharecode
{
    public const string TempDataKey = "ConsortiumSharecodeTempData";
    public string? Sharecode { get; set; }
    public string? SharecodeOrganisationName { get; set; }
    public Guid? OrganisationPartyId { get; set; }
    public bool HasBeenUpdated { get; set; } = false;
}