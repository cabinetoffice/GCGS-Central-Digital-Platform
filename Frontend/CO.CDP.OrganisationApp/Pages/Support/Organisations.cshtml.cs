using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.OrganisationApp.Pages.Support;

public class OrganisationsModel(
    IOrganisationClient organisationClient,
    ISession session) : LoggedInUserAwareModel(session)
{
    [BindProperty]
    public string Title { get; set; }

    [BindProperty]
    public string Type { get; set; }

    [BindProperty]
    public IList<ApprovableOrganisation> Organisations { get; set; } = [];

    public async Task<IActionResult> OnGet(string type)
    {
        Type = type;
        Title = type.Substring(0, 1).ToUpper() + type.Substring(1, type.Length-1) + " organisations";

        Organisations = (await organisationClient.GetAllOrganisationsAsync(Type, 1000, 0)).ToList();

        return Page();
    }
}