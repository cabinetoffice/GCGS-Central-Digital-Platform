namespace CO.CDP.ApplicationRegistry.Persistence.Entities;

public class FeatureFlag
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string TileId { get; set; }
    public bool Enabled { get; set; }
    public string? Reason { get; set; }
    public Guid UpdatedBy { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<FeatureFlagScope> Scopes { get; set; } = new List<FeatureFlagScope>();
}
