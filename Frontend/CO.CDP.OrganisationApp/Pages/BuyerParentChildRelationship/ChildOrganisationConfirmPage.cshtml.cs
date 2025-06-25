using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Logging;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.BuyerParentChildRelationship;

public class ChildOrganisationConfirmPage : PageModel
{
    private readonly IOrganisationClient _organisationClient;
    private readonly ILogger<ChildOrganisationConfirmPage> _logger;

    public ChildOrganisationConfirmPage(
        IOrganisationClient organisationClient,
        ILogger<ChildOrganisationConfirmPage> logger)
    {
        _organisationClient = organisationClient ?? throw new ArgumentNullException(nameof(organisationClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Ppon { get; set; }

    public ChildOrganisation? ChildOrganisation { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrWhiteSpace(Ppon))
        {
            return RedirectToPage("ChildOrganisationSearchPage", new { Id });
        }

        try
        {
            var formattedPponIdentifier = FormatPponIdentifier(Ppon);
            var organisation = await _organisationClient.LookupOrganisationAsync(
                name: null,
                identifier: formattedPponIdentifier);

            if (organisation != null)
            {
                Console.WriteLine(organisation.Name);
            }

            ChildOrganisation = new ChildOrganisation(
                organisation.Name,
                organisation.Id,
                organisation.Identifier
            );
        }
        catch (Exception ex)
        {
            var errorMessage = "Error occurred while retrieving organisation details";
            var cdpException = new CdpExceptionLogging(errorMessage, "LOOKUP_ERROR", ex);
            _logger.LogError(cdpException, errorMessage);
            return RedirectToPage("/Error");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {

            var formattedIdentifier = FormatPponIdentifier(Ppon);

            // await _organisationClient.CreateParentChildRelationshipAsync(
            //     parentId: Id,
            //     childId: childOrganisation.Id);

            return RedirectToPage("ChildOrganisationSuccessPage", new { Id });
        }
        catch (Exception ex)
        {
            var errorMessage = "Error occurred while establishing parent-child relationship";
            var cdpException = new CdpExceptionLogging(errorMessage, "RELATIONSHIP_ERROR", ex);
            _logger.LogError(cdpException, errorMessage);
            return RedirectToPage("/Error");
        }
    }

    private string FormatPponIdentifier(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return string.Empty;
        }

        if (identifier.StartsWith("GB-PPON:", StringComparison.OrdinalIgnoreCase))
        {
            return identifier;
        }

        if (identifier.StartsWith("GB-PPON-", StringComparison.OrdinalIgnoreCase))
        {
            return "GB-PPON:" + identifier.Substring("GB-PPON-".Length);
        }

        var pponRegex = new System.Text.RegularExpressions.Regex("^[A-Z]{4}-\\d{4}-[A-Z]{4}$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (pponRegex.IsMatch(identifier))
        {
            return $"GB-PPON:{identifier}";
        }

        return identifier.StartsWith("GB-PPON", StringComparison.OrdinalIgnoreCase)
            ? identifier
            : $"GB-PPON:{identifier}";
    }
}