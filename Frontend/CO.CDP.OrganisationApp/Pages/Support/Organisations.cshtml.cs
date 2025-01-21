using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Pages.Support;
[Authorize(Policy = PersonScopeRequirement.SupportAdmin)]
public class OrganisationsModel(
    IOrganisationClient organisationClient,
    ISession session) : LoggedInUserAwareModel(session)
{
    public string? Title { get; set; }

    public string? Type { get; set; }

    public int TotalOrganisations { get; set; }

    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }

    public int PageSize { get; set; }

    public string? PageUrl { get; set; }

    public IList<OrganisationExtended> Organisations { get; set; } = [];

    public async Task<IActionResult> OnGet(string type, int pageNumber = 1)
    {
        Type = type;

        PageUrl = $"/support/organisations/{Type}";

        PageSize = 10;

        Title = (Type == "buyer"
                ? StaticTextResource.Support_Organisations_BuyerOrganisations_Title
                : StaticTextResource.Support_Organisations_SupplierOrganisations_Title);

        var skip = (pageNumber - 1) * PageSize;

        CurrentPage = pageNumber;

        Organisations = (await organisationClient.GetAllOrganisationsAsync(Type, PageSize, skip)).ToList();

        TotalOrganisations = await organisationClient.GetOrganisationsTotalCountAsync(type);

        TotalPages = (int)Math.Ceiling((double)TotalOrganisations / PageSize);

        return Page();
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