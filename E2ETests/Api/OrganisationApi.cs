using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Playwright;
using E2ETests.Utilities;

namespace E2ETests.ApiTests
{
    public static class OrganisationApi
    {
        private static readonly string ApiBaseUrl = ConfigUtility.GetOrganisationApiBaseUrl();
        private static readonly string OrganisationsEndpoint = $"{ApiBaseUrl}/organisations";

        /// <summary>
        /// Creates a new organisation and stores its ID under a given key.
        /// </summary>
        public static async Task<string> CreateOrganisation(string token, string organisationPrefix, string storageKey)
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

            string uniqueOrgId = Guid.NewGuid().ToString();
            string uniqueOrgName = $"{organisationPrefix} {uniqueOrgId}";

            string requestBody = $@"{{
                ""name"": ""{uniqueOrgName}"",
                ""type"": ""organisation"",
                ""identifier"": {{
                    ""scheme"": ""GB-PPON"",
                    ""id"": ""{uniqueOrgId}"",
                    ""legalName"": ""{uniqueOrgName} Ltd.""
                }},
                ""contactPoint"": {{
                    ""name"": ""Test Contact"",
                    ""email"": ""contact@test.com"",
                    ""telephone"": ""079256123321"",
                    ""url"": ""https://test.com""
                }},
                ""roles"": [""tenderer""]
            }}";

            var response = await requestContext.PostAsync(OrganisationsEndpoint, new APIRequestContextOptions
            {
                DataObject = JsonDocument.Parse(requestBody).RootElement
            });

            string responseBody = await response.TextAsync();
            Console.WriteLine($"✅ Organisation Created: {responseBody}");

            try
            {
                var jsonResponse = JsonNode.Parse(responseBody);
                string organisationId = jsonResponse?["id"]?.ToString();

                if (!string.IsNullOrEmpty(organisationId))
                {
                    StorageUtility.Store(storageKey, organisationId);
                    Console.WriteLine($"📌 Stored Organisation ID: {organisationId} under key '{storageKey}'");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error extracting/storing organisation ID: {ex.Message}");
            }

            return responseBody;
        }

        /// <summary>
        /// Returns an organisation ID by its key.
        /// </summary>
        public static string GetOrganisationId(string key)
        {
            return StorageUtility.Retrieve(key);
        }
    }
}
