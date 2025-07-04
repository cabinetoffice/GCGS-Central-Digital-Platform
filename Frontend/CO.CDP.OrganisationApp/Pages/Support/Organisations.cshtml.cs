using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Pages.Support;

[Authorize(Policy = PersonScopeRequirement.SupportAdmin)]
public class OrganisationsModel(
    IOrganisationClient organisationClient,
    ISession session)
    : LoggedInUserAwareModel(session)
{
    public string? Title { get; set; }

    public string? SearchTitle { get; set; }

    public string? Type { get; set; }

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
        PageSize = 50;
        if (pageNumber < 1) pageNumber = 1;

        Type = type;

        if (Type == "buyer")
        {
            Title = StaticTextResource.Support_Organisations_BuyerOrganisations_Title;
            SearchTitle = StaticTextResource.Support_Organisations_SearchTitleBuyer;
        }
        else
        {
            Title = StaticTextResource.Support_Organisations_SupplierOrganisations_Title;
            SearchTitle = StaticTextResource.Support_Organisations_SearchTitleSupplier;
        }

        Skip = (pageNumber - 1) * PageSize;

        CurrentPage = pageNumber;
    }

    private async Task GetResults(string? searchText)
    {
        switch (Type)
        {
            case "supplier":
                {
                    var orgs = await organisationClient.GetAllOrganisationsAsync("tenderer", "tenderer", searchText, PageSize, Skip);

                    Organisations = orgs.Item1.ToList();
                    TotalOrganisations = orgs.Item2;
                    break;
                }
            case "buyer":
                {
                    var orgs = await organisationClient.GetAllOrganisationsAsync("buyer", "buyer", searchText, PageSize, Skip);

                    Organisations = orgs.Item1.ToList();
                    TotalOrganisations = orgs.Item2;
                    break;
                }
        }

        TotalPages = (int)Math.Ceiling((double)TotalOrganisations / PageSize);
    }


    public static List<Identifier> CombineIdentifiers(Identifier? identifier, ICollection<Identifier> additionalIdentifiers)
    {
        var identifiers = new List<Identifier>();

        if (identifier != null)
        {
            identifiers.Add(identifier);
        }

        foreach (var additionalIdentifier in additionalIdentifiers)
        {
            identifiers.Add(additionalIdentifier);
        }

        return identifiers;
    }
}