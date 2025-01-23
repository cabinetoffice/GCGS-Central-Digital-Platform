using CO.CDP.EntityVerificationClient;
using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DevolvedRegulation = CO.CDP.OrganisationApp.Constants.DevolvedRegulation;
using OrganisationApiException = CO.CDP.Organisation.WebApiClient.ApiException;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

[Authorize(Policy = OrgScopeRequirement.Viewer)]
public class OrganisationOverviewModel(IOrganisationClient organisationClient, IPponClient pponClient) : PageModel
{
    public OrganisationWebApiClient.Organisation? OrganisationDetails { get; set; }

    public ICollection<IdentifierRegistries>? IdentifierRegistriesDetails { get; set; }

    public BuyerInformation? BuyerInformation { get; set; }

    public List<DevolvedRegulation>? Regulations { get; set; }
    public Review? Review { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public bool HasBuyerSignedMou { get; set; } = true;

    public Mou? MouLatest { get; set; }

    public string MouSignedOnDate { get; set; } = "";

    public async Task<IActionResult> OnGet()
    {
        try
        {
            OrganisationDetails = await organisationClient.GetOrganisationAsync(Id);

            IdentifierRegistriesDetails = await GetIdentifierDetails(OrganisationDetails);

            if (OrganisationDetails.IsBuyer() || OrganisationDetails.IsPendingBuyer())
            {
                BuyerInformation = await organisationClient.GetOrganisationBuyerInformationAsync(OrganisationDetails.Id);

                var devolvedRegulations = BuyerInformation.DevolvedRegulations;

                Regulations = devolvedRegulations.AsDevolvedRegulationList();

                HasBuyerSignedMou = await CheckBuyerMouSignature(OrganisationDetails.Id);

                if (HasBuyerSignedMou)
                {
                    MouLatest = await organisationClient.GetLatestMouAsync();
                    if (MouLatest != null)
                    {
                        MouSignedOnDate = string.Format(@StaticTextResource.MoU_SignedOn, MouLatest?.CreatedOn.ToString("dd MMMM yyyy"));
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

    private async Task<bool> CheckBuyerMouSignature(Guid organisationId)
    {
        try
        {
            var mouDetails = await organisationClient.GetOrganisationLatestMouSignatureAsync(organisationId);
            return mouDetails != null && mouDetails.IsLatest;
        }
        catch (OrganisationApiException ex) when (ex.StatusCode == 404)
        {
            // Handle "No MOU signature found" scenario
            return false;
        }
    }
}