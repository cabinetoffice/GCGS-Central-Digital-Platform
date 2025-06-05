using E2ETests.ApiTests;
using E2ETests.Pages.ApiKeyManagement;

namespace E2ETests.Tests.ApiKeyManagement;
public class ApiKeyManagementBaseTest : BaseTest
{
    protected static string _organisationId;
    // Page Objects
    protected ManageApiKeyPage _manageApiKeyPage;
    protected CreateApiKeyPage _createApiKeyPage;
    protected NewApiKeyDetailsPage _newApiKeyDetailsPage;
    protected RevokeApiKeyPage _revokeApiKeyPage;

    [SetUp]
    public async Task SetupApiKeyManagementDataAsync()
    {
        await Setup();
        string accessToken = GetAccessToken();
        string storageKey = "ApiKeyManagement_Org";

        await OrganisationApi.CreateOrganisation(accessToken, "ApiKeyManagementOrg", storageKey);
        _organisationId = OrganisationApi.GetOrganisationId(storageKey);
        Console.WriteLine($"📌 Stored Organisation ID for API Key Management Tests: {_organisationId}");

        _manageApiKeyPage = new ManageApiKeyPage(_page);
        _createApiKeyPage = new CreateApiKeyPage(_page);
        _newApiKeyDetailsPage = new NewApiKeyDetailsPage(_page);
        _revokeApiKeyPage = new RevokeApiKeyPage(_page);

        Console.WriteLine($"📌 ManageApiKeyPage initialized: {_manageApiKeyPage}");
    }
}