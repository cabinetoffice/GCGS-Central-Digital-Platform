namespace CO.CDP.ApplicationRegistry.Persistence.Entities;

public class FeatureFlagScope
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FeatureFlagId { get; set; }
    public FeatureFlag FeatureFlag { get; set; } = null!;
    public Guid OrganisationTypeId { get; set; }
}
