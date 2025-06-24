using System.Text.Json;
using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.ApiTests;

public static class ConnectedPersonsApi
{
    private static readonly string ApiBaseUrl = ConfigUtility.GetOrganisationApiBaseUrl();

    public static async Task<string> AddConnectedPerson(
        string token,
        string organisationStorageKey,
        string connectedPersonStorageKey,
        DateTime? registeredDate = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
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

        // Retrieve Organisation ID from Storage
        string organisationId = StorageUtility.Retrieve(organisationStorageKey);
        if (string.IsNullOrEmpty(organisationId))
        {
            throw new Exception($"❌ Organisation ID not found for key: {organisationStorageKey}");
        }

        // Generates unique IDs for the connected person
        string connectedPersonId = Guid.NewGuid().ToString();
        string postRequestUrl = $"{ApiBaseUrl}/organisations/{organisationId}/connected-entities";

        // Sets dates dynamically, default to current time
        string registeredDateStr = (registeredDate ?? DateTime.UtcNow).ToString("yyyy-MM-ddTHH:mm:ssZ");
        string startDateStr = (startDate ?? DateTime.UtcNow).ToString("yyyy-MM-ddTHH:mm:ssZ");
        string endDateStr = (endDate ?? DateTime.UtcNow.AddYears(1)).ToString("yyyy-MM-ddTHH:mm:ssZ");

        string requestBody = $@"{{
              ""entityType"": ""Organisation"",
              ""hasCompnayHouseNumber"": true,
              ""companyHouseNumber"": ""12345678"",
              ""overseasCompanyNumber"": ""OV123456"",
              ""organisation"": {{
                ""category"": ""RegisteredCompany"",
                ""name"": ""Connected Org {connectedPersonId}"",
                ""insolvencyDate"": ""{registeredDateStr}"",
                ""registeredLegalForm"": ""Private Limited"",
                ""lawRegistered"": ""UK Law"",
                ""controlCondition"": [""None""],
                ""organisationId"": ""{organisationId}""
              }},
              ""individualOrTrust"": {{
                ""category"": ""PersonWithSignificantControlForIndividual"",
                ""firstName"": ""John"",
                ""lastName"": ""Doe"",
                ""dateOfBirth"": ""1990-01-01T00:00:00Z"",
                ""nationality"": ""British"",
                ""controlCondition"": [""None""],
                ""connectedType"": ""Individual"",
                ""personId"": ""{connectedPersonId}"",
                ""residentCountry"": ""United Kingdom""
              }},
              ""addresses"": [
                {{
                  ""streetAddress"": ""82 St. John’s Road"",
                  ""locality"": ""CHESTER"",
                  ""region"": ""Lancashire"",
                  ""postalCode"": ""CH43 7UR"",
                  ""countryName"": ""United Kingdom"",
                  ""country"": ""GB"",
                  ""type"": ""Registered""
                }}
              ],
              ""registeredDate"": ""{registeredDateStr}"",
              ""registerName"": ""Company Register"",
              ""startDate"": ""{startDateStr}"",
              ""endDate"": ""{endDateStr}""
            }}";

        var postResponse = await requestContext.PostAsync(postRequestUrl, new APIRequestContextOptions
        {
            DataObject = JsonDocument.Parse(requestBody).RootElement
        });

        string postResponseBody = await postResponse.TextAsync();
        Console.WriteLine($"✅ Connected Person Created - Raw Response: {postResponseBody}");

        // After POST request, send a GET request to retrieve the connected person ID
        string getRequestUrl = $"{ApiBaseUrl}/organisations/{organisationId}/connected-entities";

        var getResponse = await requestContext.GetAsync(getRequestUrl);
        string getResponseBody = await getResponse.TextAsync();
        Console.WriteLine($"✅ Fetched Connected Persons - Raw Response: {getResponseBody}");

        // Handle Empty Response
        if (string.IsNullOrWhiteSpace(getResponseBody))
        {
            Console.WriteLine("⚠️ API response was empty. Could not extract connected person ID.");
            throw new Exception("❌ Connected person API returned an empty response.");
        }

        try
        {
            var jsonResponse = JsonDocument.Parse(getResponseBody);
            if (jsonResponse.RootElement.ValueKind == JsonValueKind.Array && jsonResponse.RootElement.GetArrayLength() > 0)
            {
                string? storedConnectedPersonId = jsonResponse.RootElement[0].GetProperty("entityId").GetString();

                if (!string.IsNullOrEmpty(storedConnectedPersonId))
                {
                    StorageUtility.Store(connectedPersonStorageKey, storedConnectedPersonId);
                    Console.WriteLine($"📌 Stored Connected Person ID: {storedConnectedPersonId} under key '{connectedPersonStorageKey}'");
                }
                else
                {
                    Console.WriteLine("⚠️ No Connected Person ID found in response.");
                    throw new Exception("❌ No Connected Person ID found in API response.");
                }
            }
            else
            {
                throw new Exception("❌ Expected JSON array response but received a different format.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Error extracting/storing connected person ID: {ex.Message}");
        }

        return postResponseBody;
    }

    /// Retrieves a connected person ID by its storage key.
    public static string GetConnectedPersonId(string key)
    {
        return StorageUtility.Retrieve(key);
    }
}
