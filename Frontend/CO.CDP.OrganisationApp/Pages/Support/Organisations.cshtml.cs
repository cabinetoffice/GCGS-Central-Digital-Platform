using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.OrganisationApp.Pages.Support;

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
        Title = type.Substring(0, 1).ToUpper() + type.Substring(1, type.Length-1) + " organisations";

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