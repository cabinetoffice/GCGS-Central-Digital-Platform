namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;

public record FeatureFlagDto(
    string TileId,
    bool Enabled,
    string? Reason,
    Guid UpdatedBy,
    DateTimeOffset UpdatedAt);

public record UpdateFeatureFlag(
    bool Enabled,
    string? Reason);

public record FeatureFlagScopeDto(
    IEnumerable<Guid> OrganisationTypeIds);
