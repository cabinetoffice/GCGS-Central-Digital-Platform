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
            return Environment.GetEnvironmentVariable("TEST_EMAIL") ?? "onelogin.test.user1@gmail.com";
        }

=        public static string GetTestPassword()
        {
            return Environment.GetEnvironmentVariable("TEST_PASSWORD") ?? "T3ster1234**";
        }

=        public static string GetSecretKey()
        {
            return Environment.GetEnvironmentVariable("TEST_SECRET_KEY") ?? "SUW6J5JMXY425EFP32DVXGRIY54LNDBS";
        }
    }
}
