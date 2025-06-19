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
public class ConsortiumRemoveOrganisationModel(
    IOrganisationClient organisationClient,
    ITempDataService tempDataService) : PageModel
{
    public const string TempDataKey = "ConsortiumOrganisationRemoved";

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid PartyId { get; set; }

    [BindProperty]
    [Required(
        ErrorMessageResourceName = nameof(StaticTextResource.Consortium_RemoveSupplier_ErrorMessage),
        ErrorMessageResourceType = typeof(StaticTextResource))]
    public bool? ConfirmRemove { get; set; }

    public string? ConsortiumName { get; set; }
    public string? PartyName { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            var consortium = await organisationClient.GetOrganisationAsync(Id);
            if (consortium == null) return Redirect("/page-not-found");

            var parties = await organisationClient.GetOrganisationPartiesAsync(Id);
            if (parties == null || !parties.Parties.Any(p => p.Id == PartyId)) return Redirect("/page-not-found");

            InitializeValues(consortium, parties);

            return Page();
        }
        catch (ApiException<OrganisationWebApiClient.ProblemDetails> aex)
        {
            ApiExceptionMapper.MapApiExceptions(aex, ModelState);
            return Page();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }

    private void InitializeValues(OrganisationWebApiClient.Organisation consortium, OrganisationParties parties)
    {
        ConsortiumName = consortium.Name;
        PartyName = parties.Parties.First(p => p.Id == PartyId).Name;
    }

    public async Task<IActionResult> OnPost()
    {
        var consortium = await organisationClient.GetOrganisationAsync(Id);
        if (consortium == null) return Redirect("/page-not-found");

        var parties = await organisationClient.GetOrganisationPartiesAsync(Id);
        if (parties == null || !parties.Parties.Any(p => p.Id == PartyId)) return Redirect("/page-not-found");

        InitializeValues(consortium, parties);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            if (ConfirmRemove == true)
            {
                await organisationClient.RemoveOrganisationPartyAsync(Id, new RemoveOrganisationParty(organisationPartyId: PartyId));

                tempDataService.Put(TempDataKey, PartyName);
            }

            return RedirectToPage("ConsortiumOverview", new { Id });
        }
        catch (ApiException<OrganisationWebApiClient.ProblemDetails> aex)
        {
            ApiExceptionMapper.MapApiExceptions(aex, ModelState);
            return Page();
        }
    }
}