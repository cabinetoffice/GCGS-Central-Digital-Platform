namespace E2ETests.Tests.ApiKeyManagement;
public class ApiKeyManagementFunctionalTests : ApiKeyManagementBaseTest
{
    [Test]
    public async Task CreateApiKeyJourneyHappyPath()
    {
        string apiKeyName = $"ApiKey_{Guid.NewGuid()}";
        await _manageApiKeyPage.NavigateTo("ApiKeyManagement_Org");
        await _manageApiKeyPage.CompletePage();
        await _createApiKeyPage.CompletePage(apiKeyName);

        await _newApiKeyDetailsPage.CaptureApiKey("key-1");

        await _newApiKeyDetailsPage.ClickBackToManageApiKeys();
        await _manageApiKeyPage.AssertApiKeyCount(1);
        await _manageApiKeyPage.ClickCancelApiKey();
        await _revokeApiKeyPage.CompletePage();
    }
}