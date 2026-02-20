using Microsoft.Extensions.Configuration;
using CO.CDP.UserManagement.Shared.FeatureFlags;

namespace CO.CDP.UserManagement.Api.FeatureFlags;

public sealed record SubscriberFeatureFlags(
    bool OrganisationRegisteredEnabled,
    bool OrganisationUpdatedEnabled,
    bool PersonInviteClaimedEnabled)
{
    public static SubscriberFeatureFlags FromConfiguration(IConfiguration configuration) =>
        new(
            configuration.GetValue(Shared.FeatureFlags.FeatureFlags.Subscribers.OrganisationRegisteredEnabled, true),
            configuration.GetValue(Shared.FeatureFlags.FeatureFlags.Subscribers.OrganisationUpdatedEnabled, true),
            configuration.GetValue(Shared.FeatureFlags.FeatureFlags.Subscribers.PersonInviteClaimedEnabled, true));
}
