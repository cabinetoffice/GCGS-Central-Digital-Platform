using System;

namespace E2ETests.Utilities
{
    public static class ConfigUtility
    {
        /// Retrieves the base URL for the web application.
        /// Defaults to "http://localhost:8090" if not set.
        public static string GetBaseUrl()
        {
            return Environment.GetEnvironmentVariable("TEST_BASE_URL") ?? "http://localhost:8090";
        }

        /// Retrieves the Organisation API base URL.
        /// Defaults to "http://localhost:8082" if not set.
        public static string GetOrganisationApiBaseUrl()
        {
            return Environment.GetEnvironmentVariable("ORGANISATION_API_BASE_URL") ?? "http://localhost:8082";
        }

        /// Retrieves the test email.
        /// Defaults to "onelogin.test.user1@gmail.com" if not set.
        public static string GetTestEmail()
        {
            return Environment.GetEnvironmentVariable("TEST_EMAIL") ?? "test";
        }

        /// Retrieves the test password.
        /// Defaults to "T3ster1234**" if not set.
        public static string GetTestPassword()
        {
            return Environment.GetEnvironmentVariable("TEST_PASSWORD") ?? "test**";
        }

        /// Retrieves the test secret key for authentication.
        public static string GetSecretKey()
        {
            return Environment.GetEnvironmentVariable("TEST_SECRET_KEY") ?? "test";
        }
    }
}
