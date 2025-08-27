using System.Collections.Immutable;
using System.Text.RegularExpressions;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Logging;
using CO.CDP.OrganisationApp.WebApiClients;
using CO.CDP.UI.Foundation.Utilities;
using Microsoft.FeatureManagement.Mvc;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

[ValidateAntiForgeryToken]
[Authorize(Policy = PolicyNames.PartyRole.BuyerWithSignedMou)]
[Authorize(Policy = OrgScopeRequirement.Viewer)]
[FeatureGate(FeatureFlags.SearchRegistryPpon)]
public partial class OrganisationPponSearchModel(
    IOrganisationClient organisationClient,
    ISession session,
    ILogger<OrganisationPponSearchModel> logger) : LoggedInUserAwareModel(session)
{
    private readonly ILogger<OrganisationPponSearchModel> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public int TotalOrganisations { get; set; }

    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }

    public int PageSize { get; set; }

    public int Skip { get; set; }

    public string? ErrorMessage { get; set; }

    public double Threshold { get; set; } = 0.2;

    public string? FeedbackMessage { get; set; }

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)] public string SearchText { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)] public string SortOrder { get; set; } = "rel";

    public required Shared.PaginationPartialModel Pagination { get; set; }

    public IReadOnlyList<OrganisationSearchByPponResult> Organisations { get; set; } =
        ImmutableList<OrganisationSearchByPponResult>.Empty;

    public async Task<IActionResult> OnGet()
    {
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var result = await HandleSearch(PageNumber, SearchText, SortOrder, Threshold);
            ApplySearchResult(result);
        }
        else
        {
            Organisations = ImmutableList<OrganisationSearchByPponResult>.Empty;
            TotalOrganisations = 0;
            TotalPages = 0;
            ErrorMessage = null;
            Pagination = CreatePaginationModel(PageNumber, TotalOrganisations, 10, Id, SearchText, SortOrder);
        }
        return Page();
    }

    private record SearchResult(
        IReadOnlyList<OrganisationSearchByPponResult> Organisations,
        int TotalOrganisations,
        int TotalPages,
        int PageSize,
        int Skip,
        int CurrentPage,
        string? ErrorMessage,
        string? FeedbackMessage
    );

    private async Task<SearchResult> HandleSearch(int pageNumber, string searchText, string sortOrder, double threshold)
    {
        var (pageSize, skip, currentPage) = CalculatePagination(pageNumber);
        var validationResult = ValidateSearchInput(searchText);
        if (!validationResult.IsValid)
        {
            return CreateInvalidSearchResult(pageSize, skip, currentPage, validationResult.ErrorMessage);
        }
        return await FetchOrganisationSearchResults(validationResult.CleanedSearchText, sortOrder, pageSize, skip, currentPage, threshold);
    }

    private static SearchResult
        CreateInvalidSearchResult(int pageSize, int skip, int currentPage, string? errorMessage) =>
        new(
            ImmutableList<OrganisationSearchByPponResult>.Empty,
            0, 0, pageSize, skip, currentPage,
            errorMessage, null

        );

    private async Task<SearchResult> FetchOrganisationSearchResults(string cleanedSearchText, string sortOrder,
        int pageSize, int skip, int currentPage, double threshold)
    {
        try
        {
            var (orgs, totalCount) =
                await organisationClient.SearchOrganisationByNameOrPpon(cleanedSearchText, pageSize, skip, sortOrder, threshold, OrganisationSearchFilter.ExcludeOnlyPendingBuyerRoles);

            if (orgs.Count == 0)
            {
                return new SearchResult(
                    ImmutableList<OrganisationSearchByPponResult>.Empty,
                    0, 0, pageSize, skip, currentPage, null,
                    StaticTextResource.PponSearch_NoResults
                );
            }

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            return new SearchResult(
                orgs.ToImmutableList(),
                totalCount,
                totalPages,
                pageSize,
                skip,
                currentPage,
                null,
                null
            );
        }
        catch (Exception ex)
        {
            LogApiError(ex);
            return new SearchResult(
                ImmutableList<OrganisationSearchByPponResult>.Empty,
                0, 0, pageSize, skip, currentPage,
                null, StaticTextResource.PponSearch_NoResults
            );
        }
    }

    private void ApplySearchResult(SearchResult result)
    {
        Organisations = result.Organisations;
        TotalOrganisations = result.TotalOrganisations;
        TotalPages = result.TotalPages;
        PageSize = result.PageSize;
        Skip = result.Skip;
        CurrentPage = result.CurrentPage;
        ErrorMessage = result.ErrorMessage;
        FeedbackMessage = result.FeedbackMessage;
        Pagination = CreatePaginationModel(CurrentPage, TotalOrganisations, PageSize, Id, SearchText, SortOrder);
    }

    private void LogApiError(Exception ex)
    {
        var errorMessage = "Error occurred while searching for organisations";
        var cdpException = new CdpExceptionLogging(errorMessage, "SEARCH_REGISTRY_OF_PPON_ERROR", ex);
        _logger.LogError(cdpException, errorMessage);
    }

    public static (bool IsValid, string ErrorMessage, string CleanedSearchText) ValidateSearchInput(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return (false, StaticTextResource.Global_EnterSearchTerm, string.Empty);
        }

        string cleanedSearchText = InputSanitiser.SanitiseSingleLineTextInput(searchText) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(cleanedSearchText))
        {
            return (false, StaticTextResource.PponSearch_Invalid_Search_Value, cleanedSearchText);
        }

        if (!HasLetterOrNumberRegex().IsMatch(cleanedSearchText))
        {
            return (false, StaticTextResource.PponSearch_Invalid_Search_Value, cleanedSearchText);
        }

        return (true, string.Empty, cleanedSearchText);
    }

    public static (int PageSize, int Skip, int CurrentPage) CalculatePagination(int pageNumber,
        int defaultPageSize = 10)
    {
        var page = pageNumber < 1 ? 1 : pageNumber;
        var skip = (page - 1) * defaultPageSize;
        return (defaultPageSize, skip, page);
    }

    public string FormatAddresses(IEnumerable<OrganisationAddress> addresses)
    {
        var address = addresses.FirstOrDefault();
        if (address == null)
        {
            return "N/A";
        }

        var parts = new[]
        {
            address.StreetAddress,
            address.Locality,
            address.Region,
            address.PostalCode,
            address.Country
        }.Where(part => !string.IsNullOrWhiteSpace(part));
        var result = string.Join(", ", parts);
        return !string.IsNullOrEmpty(result) ? result : "N/A";
    }

    public string GetTagClassForRole(PartyRole role) => role switch
    {
        PartyRole.Buyer => "govuk-tag--blue",
        PartyRole.ProcuringEntity => "govuk-tag--green",
        PartyRole.Supplier => "govuk-tag--purple",
        PartyRole.Tenderer => "govuk-tag--yellow",
        PartyRole.Funder => "govuk-tag--red",
        PartyRole.Enquirer => "govuk-tag--orange",
        PartyRole.Payer => "govuk-tag--pink",
        PartyRole.Payee => "govuk-tag--grey",
        PartyRole.ReviewBody => "govuk-tag--turquoise",
        PartyRole.InterestedParty => "govuk-tag--light-blue",
        _ => "govuk-tag--black"
    };

    public static Shared.PaginationPartialModel CreatePaginationModel(int currentPage, int totalItems, int pageSize,
        Guid id, string? searchText, string sortOrder)
    {
        return new Shared.PaginationPartialModel
        {
            CurrentPage = currentPage,
            TotalItems = totalItems,
            PageSize = pageSize,
            Url = $"/organisation/{id}/buyer/search?SearchText={Uri.EscapeDataString(searchText ?? string.Empty)}&sortOrder={sortOrder}&pageSize={pageSize}"
        };
    }

    [GeneratedRegex(@"[a-zA-Z0-9]")]
    private static partial Regex HasLetterOrNumberRegex();
}