using System;

namespace E2ETests.Utilities
{
    public static class ConfigUtility
    {
=        public static string GetBaseUrl()
        {
            return Environment.GetEnvironmentVariable("TEST_BASE_URL") ?? "http://localhost:8090";
        }

=        public static string GetOrganisationApiBaseUrl()
        {
            return Environment.GetEnvironmentVariable("ORGANISATION_API_BASE_URL") ?? "http://localhost:8082";
        }

=        public static string GetTestEmail()
        {
            return Environment.GetEnvironmentVariable("TEST_EMAIL") ?? "test";
        }

=        public static string GetTestPassword()
        {
            return Environment.GetEnvironmentVariable("TEST_PASSWORD") ?? "test**";
        }

=        public static string GetSecretKey()
        {
            return Environment.GetEnvironmentVariable("TEST_SECRET_KEY") ?? "test";
        }
    }
}
