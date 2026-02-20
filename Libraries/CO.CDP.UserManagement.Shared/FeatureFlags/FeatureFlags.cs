namespace CO.CDP.UserManagement.Shared.FeatureFlags;

public static class FeatureFlags
{
    public static class Subscribers
    {
        public const string OrganisationRegisteredEnabled = "Features:Subscribers:OrganisationRegisteredEnabled";
        public const string OrganisationUpdatedEnabled = "Features:Subscribers:OrganisationUpdatedEnabled";
        public const string PersonInviteClaimedEnabled = "Features:Subscribers:PersonInviteClaimedEnabled";
    }

    public static class UserFlows
    {
        public const string InviteFlowEnabled = "Features:UserFlows:InviteFlowEnabled";
        public const string MembershipFlowEnabled = "Features:UserFlows:MembershipFlowEnabled";
    }
}
