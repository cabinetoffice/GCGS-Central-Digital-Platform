using System.Text.Json.Nodes;
using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.ApiTests;

/// <summary>
/// API helper for seeding and inspecting join requests via the OI API.
/// Used for E2E test data setup only.
/// </summary>
public static class JoinRequestApi
{
    private static readonly string ApiBaseUrl = ConfigUtility.GetOrganisationApiBaseUrl();

    /// <summary>
    /// Looks up the OI person GUID for the test user by email.
    /// </summary>
    public static async Task<Guid?> GetPersonIdByEmailAsync(string token, string email)
    {
        using var playwright = await Playwright.CreateAsync();
        var requestContext = await playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            ExtraHTTPHeaders = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {token}" },
                { "Accept", "application/json" }
            }
        });

        var response = await requestContext.GetAsync(
            $"{ApiBaseUrl}/persons?email={Uri.EscapeDataString(email)}");

        if (!response.Ok) return null;

        var body = await response.TextAsync();
        var json = JsonNode.Parse(body);
        var idString = json?["id"]?.ToString();
        return Guid.TryParse(idString, out var id) ? id : null;
    }

    /// <summary>
    /// Creates a join request for the given person to the given organisation.
    /// Returns the join request ID, or null on failure.
    /// </summary>
    public static async Task<Guid?> CreateJoinRequestAsync(string token, Guid organisationId, Guid personId)
    {
        using var playwright = await Playwright.CreateAsync();
        var requestContext = await playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            ExtraHTTPHeaders = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {token}" },
                { "Accept", "application/json" },
                { "Content-Type", "application/json" }
            }
        });

        var body = $@"{{ ""personId"": ""{personId}"" }}";

        var response = await requestContext.PostAsync(
            $"{ApiBaseUrl}/organisations/{organisationId}/join-requests",
            new APIRequestContextOptions { DataString = body });

        if (!response.Ok)
        {
            Console.WriteLine($"⚠️ CreateJoinRequest returned {response.Status}: {await response.TextAsync()}");
            return null;
        }

        var responseBody = await response.TextAsync();
        var json = JsonNode.Parse(responseBody);
        var idString = json?["id"]?.ToString();
        return Guid.TryParse(idString, out var id) ? id : null;
    }
}
