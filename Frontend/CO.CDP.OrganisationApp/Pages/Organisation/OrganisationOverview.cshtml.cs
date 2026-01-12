using CO.CDP.EntityVerificationClient;
using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement;
using DevolvedRegulation = CO.CDP.OrganisationApp.Constants.DevolvedRegulation;
using OrganisationApiException = CO.CDP.Organisation.WebApiClient.ApiException;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

[Authorize(Policy = OrgScopeRequirement.Viewer)]
public class OrganisationOverviewModel(IOrganisationClient organisationClient, IPponClient pponClient, IFeatureManager featureManager) : PageModel
{
    public OrganisationWebApiClient.Organisation? OrganisationDetails { get; set; }

    public ICollection<IdentifierRegistries>? IdentifierRegistriesDetails { get; set; }

    public BuyerInformation? BuyerInformation { get; set; }

    public List<DevolvedRegulation>? Regulations { get; set; }
    public Review? Review { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Origin { get; set; }

    public bool HasBuyerSignedMou { get; set; } = false;

    public string MouSignedOnDate { get; set; } = "";

    public bool SearchRegistryPponEnabled { get; private set; }
    public string BackLinkUrl { get; private set; } = "";

    public ICollection<OrganisationSummary>? ChildOrganisations { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            OrganisationDetails = await organisationClient.GetOrganisationAsync(Id);

            SearchRegistryPponEnabled = await featureManager.IsEnabledAsync(FeatureFlags.SearchRegistryPpon);

            if (OrganisationDetails.Type == OrganisationWebApiClient.OrganisationType.InformalConsortium)
            {
                return RedirectToPage("/Consortium/ConsortiumOverview", new { Id });
            }

            BackLinkUrl = Origin switch
            {
                "organisation-home" => $"/organisation/{Id}/home",
                _ => "/organisation-selection"
            };

            IdentifierRegistriesDetails = await GetIdentifierDetails(OrganisationDetails);

            if (OrganisationDetails.IsBuyer() || OrganisationDetails.IsPendingBuyer())
            {
                BuyerInformation = await organisationClient.GetOrganisationBuyerInformationAsync(OrganisationDetails.Id);

                var devolvedRegulations = BuyerInformation.DevolvedRegulations;

                Regulations = devolvedRegulations.AsDevolvedRegulationList();

                var mouSignatureLatest = await GetBuyerMouSignature(OrganisationDetails.Id);

                if (mouSignatureLatest != null && mouSignatureLatest.IsLatest)
                {
                    HasBuyerSignedMou = true;
                    MouSignedOnDate = string.Format(@StaticTextResource.MoU_SignedOn, mouSignatureLatest.SignatureOn.ToString("dd MMMM yyyy"));
                }

                if (OrganisationDetails.IsBuyer())
                {
                    var isBuyerParentChildRelationshipEnabled = await featureManager.IsEnabledAsync(FeatureFlags.BuyerParentChildRelationship);
                    if (isBuyerParentChildRelationshipEnabled)
                    {
                        ChildOrganisations = await OrganisationClientExtensions.GetChildOrganisationsAsync(organisationClient, OrganisationDetails.Id);
                    }
                }
            }

            if (OrganisationDetails.Details.PendingRoles.Count > 0)
            {
                var reviews = await organisationClient.GetOrganisationReviewsAsync(Id) ?? new List<Review>();
                Review = reviews.FirstOrDefault();
            }
            return Page();
        }
        catch (OrganisationApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }

    private async Task<ICollection<IdentifierRegistries>> GetIdentifierDetails(OrganisationWebApiClient.Organisation organisationDetails)
    {
        var identifiers = organisationDetails.AdditionalIdentifiers?.ToList()
                        ?? new List<OrganisationWebApiClient.Identifier>();

        if (organisationDetails.Identifier != null)
        {
            identifiers.Add(organisationDetails.Identifier);
        }

        var schemes = identifiers
            .Where(x => x != null)
            .Select(x => x.Scheme)
            .ToArray();
        try
        {
            return await pponClient.GetIdentifierRegistriesDetailAsync(schemes);
        }
        catch
        {
            return new List<IdentifierRegistries>();
        }
    }

    private async Task<MouSignatureLatest?> GetBuyerMouSignature(Guid organisationId)
    {
        try
        {
            var mouSignature = await organisationClient.GetOrganisationLatestMouSignatureAsync(organisationId);
            if (mouSignature != null && mouSignature.IsLatest)
            {
                return mouSignature;
            }
        }
        catch (OrganisationApiException ex) when (ex.StatusCode == 404)
        {

        }

        return null;
    }
}