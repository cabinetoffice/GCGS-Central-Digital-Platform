using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.UI.Foundation.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;

namespace CO.CDP.OrganisationApp.Pages.Buyer.Hierarchy;

[Authorize(Policy = PolicyNames.PartyRole.BuyerWithSignedMou)]
[Authorize(Policy = OrgScopeRequirement.Editor)]
[FeatureGate(FeatureFlags.BuyerParentChildRelationship)]
public partial class ChildOrganisationSearchPage(IFeatureManager featureManager)
    : PageModel
{
    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }

    [BindProperty]
    public string? Query { get; set; }

    public bool SearchRegistryPponEnabled { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        SearchRegistryPponEnabled = await featureManager.IsEnabledAsync(FeatureFlags.SearchRegistryPpon);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var validationResult = ValidateSearchInput(Query ?? string.Empty);
        if (!validationResult.IsValid)
        {
            ModelState.AddModelError(nameof(Query), validationResult.ErrorMessage);
        }

        if (!ModelState.IsValid)
        {
            SearchRegistryPponEnabled = await featureManager.IsEnabledAsync(FeatureFlags.SearchRegistryPpon);
            return Page();
        }

        return RedirectToPage("ChildOrganisationResultsPage", new { Id, query = validationResult.CleanedSearchText });
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

    [System.Text.RegularExpressions.GeneratedRegex(@"[a-zA-Z0-9]")]
    private static partial System.Text.RegularExpressions.Regex HasLetterOrNumberRegex();
}