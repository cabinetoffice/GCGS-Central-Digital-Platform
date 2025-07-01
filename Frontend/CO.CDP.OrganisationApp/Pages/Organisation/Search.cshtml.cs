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

    public IList<OrganisationDto> Organisations { get; set; } = [];

    public async Task<IActionResult> OnGet(string type, int pageNumber = 1, [FromQuery(Name = "q")] string? searchText = null)
    {
        InitModel(type, pageNumber);

        await GetResults(searchText);

        return Page();
    }

    private void InitModel(string type, int pageNumber)
    {
        PageSize = 2;
        if (pageNumber < 1) pageNumber = 1;

        Title = StaticTextResource.PponSearch_Title;
        SearchTitle = StaticTextResource.PponSearch_Hint;

        Skip = (pageNumber - 1) * PageSize;

        CurrentPage = pageNumber;
    }

    private async Task GetResults(string? searchText)
    {
        var orgs = await organisationClient.GetAllOrganisationsAsync("buyer", "buyer", searchText, PageSize, Skip);

        Organisations = orgs.Item1.ToList();
        TotalOrganisations = orgs.Item2;

        TotalPages = (int)Math.Ceiling((double)TotalOrganisations / PageSize);
    }

    // //Unused
    // public static List<Identifier> CombineIdentifiers(Identifier? identifier, ICollection<Identifier> additionalIdentifiers)
    // {
    //     var identifiers = new List<Identifier>();
    //
    //     if (identifier != null)
    //     {
    //         identifiers.Add(identifier);
    //     }
    //
    //     foreach (var additionalIdentifier in additionalIdentifiers)
    //     {
    //         identifiers.Add(additionalIdentifier);
    //     }
    //
    //     return identifiers;
    // }
}