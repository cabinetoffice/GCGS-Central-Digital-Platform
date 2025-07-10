using System.Text.RegularExpressions;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Pages.Organisation;

[Authorize(Policy = OrgScopeRequirement.Viewer)]
public class SearchModel(
    IOrganisationClient organisationClient,
    ISession session)
    : LoggedInUserAwareModel(session)
{
    public string? Title { get; set; }

    public string? SearchTitle { get; set; }

    public int TotalOrganisations { get; set; }

    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }

    public int PageSize { get; set; }

    public int Skip { get; set; }

    public IList<OrganisationSearchByPponResult> Organisations { get; set; } = [];

    public async Task<IActionResult> OnGet(string type, int pageNumber = 1,
        [FromQuery(Name = "q")] string? searchText = null,
        [FromQuery(Name = "sortOrder")] string? sortOrder = "relevance")
    {
        InitModel(type, pageNumber);
        await GetResults(searchText, sortOrder);
        return Page();
    }

    private void InitModel(string type, int pageNumber)
    {
        PageSize = 50;
        if (pageNumber < 1) pageNumber = 1;
        Title = StaticTextResource.PponSearch_Title;
        SearchTitle = StaticTextResource.PponSearch_Hint;
        Skip = (pageNumber - 1) * PageSize;
        CurrentPage = pageNumber;
    }

    private async Task GetResults(string? searchText, string? sortOrder)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            resetKeyParams();
            return;
        }

        searchText = Regex.Replace(searchText.Trim(), @"[^\w\s]", string.Empty);
        if (string.IsNullOrWhiteSpace(searchText))
        {
            resetKeyParams();
            return;
        }

        var orgs = await organisationClient.SearchByNameOrPponAsync(searchText.Trim(), PageSize, Skip);
        Organisations = sortOrder switch
        {
            "ascending" => orgs.OrderBy(o => o.Name).ToList(),
            "descending" => orgs.OrderByDescending(o => o.Name).ToList(),
            _ => orgs.ToList()
        };
        TotalOrganisations = orgs.Count;
        TotalPages = (int)Math.Ceiling((double)TotalOrganisations / PageSize);
    }

    private void resetKeyParams()
    {
        Organisations = [];
        TotalOrganisations = 0;
        TotalPages = 0;
    }

    public string FormatAddresses(IEnumerable<OrganisationAddress> addresses)
    {
        if (addresses == null || !addresses.Any())
            return "N/A";
        var address = addresses.FirstOrDefault();
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(address.StreetAddress))
            parts.Add(address.StreetAddress);
        if (!string.IsNullOrWhiteSpace(address.Locality))
            parts.Add(address.Locality);
        if (!string.IsNullOrWhiteSpace(address.Region))
            parts.Add(address.Region);
        if (!string.IsNullOrWhiteSpace(address.PostalCode))
            parts.Add(address.PostalCode);
        if (!string.IsNullOrWhiteSpace(address.Country))
            parts.Add(address.Country);
        return parts.Any() ? string.Join(", ", parts) : "N/A";
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
}