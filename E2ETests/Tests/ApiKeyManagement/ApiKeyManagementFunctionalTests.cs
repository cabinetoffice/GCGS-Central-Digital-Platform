namespace E2ETests.Tests.ApiKeyManagement;
public class ApiKeyManagementFunctionalTests : ApiKeyManagementBaseTest
{
    [Test]
    public async Task CreateApiKeyJourneyHappyPath()
    {
        TestContext.Out.WriteLine("🔹 Scenario: Creating an API key successfully.");
        string apiKeyName = $"ApiKey_{Guid.NewGuid()}";
        await _manageApiKeyPage.NavigateTo("ApiKeyManagement_Org");
        await _manageApiKeyPage.CompletePage();
        await _createApiKeyPage.CompletePage(apiKeyName);
        Console.WriteLine($"✅ Created API Key with name: {apiKeyName}");

        await _newApiKeyDetailsPage.NavigateTo("ApiKeyManagement_Org", "key-1");

        await _newApiKeyDetailsPage.ClickBackToManageApiKeys();
        await _manageApiKeyPage.AssertApiKeyCount(1);
        await _manageApiKeyPage.ClickCancelApiKey();
        await _revokeApiKeyPage.CompletePage();
    }
}