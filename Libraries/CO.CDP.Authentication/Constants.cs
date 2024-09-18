namespace CO.CDP.Authentication;

public static class Constants
{
    public static class ClaimType
    {
        public const string Channel = "channel";
        public const string Subject = "sub";
        public const string TenantLookup = "ten";
        public const string OrganisationId = "org";
        public const string ApiKeyScope = "scope";
    }

    public static class Channel
    {
        public const string OneLogin = "one-login";
        public const string OrganisationKey = "organisation-key";
        public const string ServiceKey = "service-key";
    }
}