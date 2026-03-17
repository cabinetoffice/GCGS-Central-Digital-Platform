namespace CO.CDP.UserManagement.Shared.FeatureFlags;

public static class FeatureFlags
{
    public static class Subscribers
    {
        public const string OrganisationRegisteredEnabled = "Features:Subscribers:OrganisationRegisteredEnabled";
        public const string OrganisationUpdatedEnabled = "Features:Subscribers:OrganisationUpdatedEnabled";
        public const string PersonInviteClaimedEnabled = "Features:Subscribers:PersonInviteClaimedEnabled";
    }

    public static class Messaging
    {
        /// <summary>
        /// Enables the SQS dispatcher background service and all pub/sub subscriber wiring.
        /// Set to false to temporarily disable messaging without removing configuration.
        /// </summary>
        public const string SqsDispatcherEnabled = "Features:Messaging:SqsDispatcherEnabled";
    }
}
