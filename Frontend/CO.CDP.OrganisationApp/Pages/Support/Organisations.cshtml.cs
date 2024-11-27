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

    public IList<OrganisationExtended> Organisations { get; set; } = [];

    public async Task<IActionResult> OnGet(string type)
    {
        Type = type;        
        Title = (Type == "buyer"
                ? StaticTextResource.Support_Organisations_BuyerOrganisations_Title
                : StaticTextResource.Support_Organisations_SupplierOrganisations_Title);

        Organisations = (await organisationClient.GetAllOrganisationsAsync(Type, 1000, 0)).ToList();

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