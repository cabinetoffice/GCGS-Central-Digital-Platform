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

    public static class OrganisationPersonScope
    {
        public const string Admin = "ADMIN";
        public const string Responder = "RESPONDER";
        public const string Editor = "EDITOR";
        public const string Viewer = "VIEWER";
    }
}