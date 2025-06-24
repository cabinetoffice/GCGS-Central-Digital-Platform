using System.Text.Json;
using Microsoft.Playwright;

namespace E2ETests.Utilities;

public static class ApiUtility
{
    public static async Task<string> SendApiRequest(string url, string method, string token, string? requestBody = null)
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

        IAPIResponse response;

        switch (method.ToUpper())
        {
            case "GET":
                response = await requestContext.GetAsync(url);
                break;

            case "POST":
                if (string.IsNullOrWhiteSpace(requestBody))
                {
                    throw new ArgumentException("POST requests require a request body.");
                }
                response = await requestContext.PostAsync(url, new APIRequestContextOptions
                {
                    DataObject = JsonDocument.Parse(requestBody).RootElement
                });
                break;

            case "PUT":
                if (string.IsNullOrWhiteSpace(requestBody))
                {
                    throw new ArgumentException("PUT requests require a request body.");
                }
                response = await requestContext.PutAsync(url, new APIRequestContextOptions
                {
                    DataObject = JsonDocument.Parse(requestBody).RootElement
                });
                break;

            case "DELETE":
                response = await requestContext.DeleteAsync(url);
                break;

            default:
                throw new ArgumentException($"Unsupported HTTP method: {method}");
        }

        string responseBody = await response.TextAsync();
        Console.WriteLine($"Response Status: {response.Status}");
        Console.WriteLine($"Response Body: {responseBody}");

        return responseBody;
    }
}
