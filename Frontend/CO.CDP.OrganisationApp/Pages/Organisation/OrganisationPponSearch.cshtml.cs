using System.Net;
using System.Text.RegularExpressions;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Logging;
using CO.CDP.OrganisationApp.WebApiClients;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

[ValidateAntiForgeryToken]
[Authorize(Policy = OrgScopeRequirement.Viewer)]
public class OrganisationPponSearchModel(
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

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string SearchText { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string SortOrder { get; set; } = "rel";

    public required Shared.PaginationPartialModel Pagination { get; set; }

    public IList<OrganisationSearchByPponResult> Organisations { get; set; } = new List<OrganisationSearchByPponResult>();

    public async Task<IActionResult> OnGet()
    {
        await HandleSearch(PageNumber, SearchText, SortOrder);
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        await HandleSearch(PageNumber, SearchText, SortOrder);
        return Page();
    }

    private async Task HandleSearch(int pageNumber, string searchText, string sortOrder)
    {
        var validationResult = ValidateSearchInput(searchText);
        ErrorMessage = validationResult.ErrorMessage;
        SetPaginationParams(pageNumber);
        if (validationResult.IsValid)
        {
            await FetchAndSetResults(validationResult.CleanedSearchText, sortOrder);
        }
        SetPaginationModel(searchText, sortOrder);
    }

    private (bool IsValid, string ErrorMessage, string CleanedSearchText) ValidateSearchInput(string searchText)
    {
        var isValid = true;
        var errorMessage = string.Empty;
        var cleanedSearchText = string.Empty;
        if (string.IsNullOrWhiteSpace(searchText))
        {
            isValid = false;
            errorMessage = StaticTextResource.PponSearch_Invalid_Search_Value;
        }
        else
        {
            string regexPattern = @"[^a-zA-Z0-9\s\-]";
            string originalSearchText = searchText.Trim();
            bool containsInvalidChars = Regex.IsMatch(originalSearchText, regexPattern);
            cleanedSearchText = Regex.Replace(originalSearchText, regexPattern, string.Empty);
            if (string.IsNullOrWhiteSpace(cleanedSearchText) || containsInvalidChars)
            {
                isValid = false;
                errorMessage = StaticTextResource.PponSearch_Invalid_Search_Value;
            }
        }
        return (isValid, errorMessage, cleanedSearchText);
    }

    private void SetPaginationParams(int pageNumber)
    {
        PageSize = 10;
        if (pageNumber < 1) pageNumber = 1;
        Skip = (pageNumber - 1) * PageSize;
        CurrentPage = pageNumber;
    }

    private async Task FetchAndSetResults(string cleanedSearchText, string sortOrder)
    {
        if (string.IsNullOrWhiteSpace(sortOrder)) sortOrder = "rel";
        try
        {
            var (orgs, totalCount) = await organisationClient.SearchOrganisationByNameOrPpon(cleanedSearchText, PageSize, Skip, sortOrder);
            if (orgs.Count == 0)
            {
                ErrorMessage = StaticTextResource.PponSearch_NoResults;
            }
            else
            {
                Organisations = orgs.ToList();
                TotalOrganisations = totalCount;
                TotalPages = (int)Math.Ceiling((double)TotalOrganisations / PageSize);
            }
        }
        catch (Exception ex) when (
            (ex is ApiException apiEx && apiEx.StatusCode != 404) ||
            (ex is HttpRequestException httpEx && httpEx.StatusCode != HttpStatusCode.NotFound) ||
            (!(ex is ApiException) && !(ex is HttpRequestException)))
        {
            ErrorMessage = StaticTextResource.PponSearch_NoResults;
            LogApiError(ex);
        }
    }

    private void SetPaginationModel(string searchText, string sortOrder)
    {
        Pagination = new Shared.PaginationPartialModel
        {
            CurrentPage = CurrentPage,
            TotalItems = TotalOrganisations,
            PageSize = PageSize,
            Url = $"/organisation/{Id}/buyer/search?q={searchText}&sortOrder={sortOrder}"
        };
    }

    public string FormatAddresses(IEnumerable<OrganisationAddress> addresses)
    {
        var address = addresses?.FirstOrDefault();
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
            }
            .Where(part => !string.IsNullOrWhiteSpace(part));

        var result = string.Join(", ", parts);
        return !string.IsNullOrEmpty(result) ? result : "N/A";
    }

    public string GetTagClassForRole(PartyRole role)
    {
        return role switch
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
    }

    private void LogApiError(Exception ex)
    {
        var errorMessage = "Error occurred while searching for organisations";
        var cdpException = new CdpExceptionLogging(errorMessage, "SEARCH_REGISTRY_OF_PPON_ERROR", ex);
        _logger.LogError(cdpException, errorMessage);
    }
}