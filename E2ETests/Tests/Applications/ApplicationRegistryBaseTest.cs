using E2ETests.ApiTests;
using E2ETests.Pages.Applications;

namespace E2ETests.Applications;

/// <summary>
/// Base test fixture for Application Registry UI tests.
/// Creates a Buyer organisation via the API, then initialises all page objects.
/// </summary>
public class ApplicationRegistryBaseTest : BaseTest
{
    // ── IDs set up per fixture ──────────────────────────────────────────────
    protected static string _organisationId = string.Empty;

    // ── Page Objects ────────────────────────────────────────────────────────
    protected ApplicationListPage   _applicationListPage   = null!;
    protected ApplicationDetailPage _applicationDetailPage = null!;
    protected UserAssignmentsPage   _userAssignmentsPage   = null!;
    protected AssignUserPage        _assignUserPage        = null!;
    protected CheckAnswersPage      _checkAnswersPage      = null!;
    protected RevokeConfirmationPage _revokePage           = null!;

    [SetUp]
    public async Task SetupApplicationRegistryData()
    {
        await base.Setup();

        string accessToken = GetAccessToken();
        const string storageKey = "AppRegistry_Org";

        // Create a Buyer organisation (roles: ["buyer"]) via the existing API helper.
        // The API helper creates a tenderer by default; we reuse it with a Buyer-specific
        // prefix so this org is identifiable, then rely on the admin upgrading it via UI
        // or the API supporting buyer creation.
        await OrganisationApi.CreateOrganisation(accessToken, "AppRegistryBuyer", storageKey);
        _organisationId = OrganisationApi.GetOrganisationId(storageKey);

        Console.WriteLine($"📌 AppRegistry test org ID: {_organisationId}");

        // Initialise all page objects
        _applicationListPage    = new ApplicationListPage(_page);
        _applicationDetailPage  = new ApplicationDetailPage(_page);
        _userAssignmentsPage    = new UserAssignmentsPage(_page);
        _assignUserPage         = new AssignUserPage(_page);
        _checkAnswersPage       = new CheckAnswersPage(_page);
        _revokePage             = new RevokeConfirmationPage(_page);
    }
}
