using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement.Mvc;
using System.ComponentModel.DataAnnotations;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;
using CO.CDP.DataSharing.WebApiClient;

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
            var shareCode = await dataSharingClient.GetSharedDataAsync(EnterSharecode);

            var parties = await organisationClient.GetOrganisationPartiesAsync(Id);

            if (parties.Parties.Where(p => p.ShareCode.Value == EnterSharecode).Any())
            {
                ModelState.AddModelError(nameof(EnterSharecode), string.Format(StaticTextResource.Consortium_ConsortiumEnterSharecode_SharecodeAlreadyExists, shareCode.Name));
                return Page();
            }

            var sc = new ConsortiumSharecode { Sharecode = EnterSharecode, SharecodeOrganisationName = shareCode.Name };

            tempDataService.Put(ConsortiumSharecode.TempDataKey, sc);

            return RedirectToPage("ConsortiumConfirmSupplier", new { Id });
        }
        catch (CO.CDP.DataSharing.WebApiClient.ApiException<DataSharing.WebApiClient.ProblemDetails> aex)
        {
            DataSharingApiExceptionMapper.MapApiExceptions(aex, ModelState);
            return Page();
        }

        //Verify share code
        //if valid proceed to next step
        //else show error message
        
    }
}

public class ConsortiumSharecode
{
    public const string TempDataKey = "ConsortiumSharecodeTempData";
    public string? Sharecode { get; set; }
    public string? SharecodeOrganisationName { get; set; }
}