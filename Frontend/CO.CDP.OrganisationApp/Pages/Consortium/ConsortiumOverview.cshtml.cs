using CO.CDP.DataSharing.WebApiClient;
using CO.CDP.Forms.WebApiClient;
using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement.Mvc;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Consortium;

[FeatureGate(FeatureFlags.Consortium)]

public class ConsortiumOverviewModel(
    IOrganisationClient organisationClient,
    IFormsClient formsClient,
    IFlashMessageService flashMessageService,
    ITempDataService tempDataService,
    IDataSharingClient dataSharingClient) : PageModel
{
    public OrganisationWebApiClient.Organisation? OrganisationDetails { get; set; }

    public OrganisationParties? Parties { get; set; }

    public ICollection<FormSectionSummary> FormSections { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            OrganisationDetails = await organisationClient.GetOrganisationAsync(Id);

            try
            {
                Parties = await organisationClient.GetOrganisationPartiesAsync(Id);
            }
            catch (OrganisationWebApiClient.ApiException ex) when (ex.StatusCode == 404)
            {
                Parties = null;
            }

            HandleFlashMessages(flashMessageService, tempDataService);

            var forms = await formsClient.GetFormSectionsAsync(new Guid(FormsEngine.OrganisationConsortiumFormId), Id);

            FormSections = forms.FormSections;

            return Page();
        }
        catch (Exception ex)
            when ((ex is OrganisationWebApiClient.ApiException oex && oex.StatusCode == 404)
                || (ex is CDP.Forms.WebApiClient.ApiException wex && wex.StatusCode == 404))
        {
            return Redirect("/page-not-found");
        }
    }

    private static void HandleFlashMessages(IFlashMessageService flashMessageService, ITempDataService tempDataService)
    {
        var sc = tempDataService.Get<ConsortiumSharecode>(ConsortiumSharecode.TempDataKey);

        if (sc != null)
        {
            if (!sc.HasBeenUpdated)
            {
                flashMessageService.SetFlashMessage
                (
                    FlashMessageType.Success,
                    heading: string.Format(StaticTextResource.Consortium_ConsortiumOverview_Success_Heading, sc.SharecodeOrganisationName)
                );
            }
            else
            {
                flashMessageService.SetFlashMessage
                (
                    FlashMessageType.Success,
                    heading: string.Format(StaticTextResource.Consortium_ConsortiumOverview_Updated_Success_Heading, sc.SharecodeOrganisationName)
                );
            }
        }

        var removedPartyName = tempDataService.Get<string>(ConsortiumRemoveOrganisationModel.TempDataKey);

        if (removedPartyName != null)
        {
            flashMessageService.SetFlashMessage
                (
                    FlashMessageType.Success,
                    heading: string.Format(StaticTextResource.Consortium_RemoveSupplier_SuccessMessage, removedPartyName)
                );
        }
    }

    public async Task<int> GetShareCodesCount()
    {
        return (await dataSharingClient.GetShareCodeListAsync(Id)).Count;
    }
}