namespace CO.CDP.RegisterOfCommercialTools.WebApi.Tests.Integration;

public static class AuthenticationTestHelpers
{
    public const string TestApiKey = "test-api-key-for-local-development";
    private const string ApiKeyHeaderName = "cdp-api-key";

    public static void AddApiKeyAuthentication(HttpClient client)
    {
        client.DefaultRequestHeaders.Add(ApiKeyHeaderName, TestApiKey);
    }

    public static void AddApiKeyConfiguration(Dictionary<string, string?> config)
    {
        config["ApiKey"] = TestApiKey;
    }
}
