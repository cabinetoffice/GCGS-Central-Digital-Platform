using System.Text.Json.Serialization;

namespace CO.CDP.Organisation.WebApi.Model;

public record UserClaimsResponse
{
    [JsonPropertyName("userPrincipalId")]
    public required string UserPrincipalId { get; init; }

    [JsonPropertyName("organisations")]
    public ICollection<OrganisationMembershipClaim> Organisations { get; init; } = [];
}

public record OrganisationMembershipClaim
{
    [JsonPropertyName("organisationId")]
    public Guid OrganisationId { get; init; }

    [JsonPropertyName("organisationName")]
    public string OrganisationName { get; init; } = "";

    [JsonPropertyName("organisationRole")]
    public string OrganisationRole { get; init; } = "";

    [JsonPropertyName("applications")]
    public ICollection<ApplicationAssignmentClaim> Applications { get; init; } = [];
}

public record ApplicationAssignmentClaim
{
    [JsonPropertyName("applicationId")]
    public Guid ApplicationId { get; init; }

    [JsonPropertyName("applicationName")]
    public string ApplicationName { get; init; } = "";

    [JsonPropertyName("clientId")]
    public string ClientId { get; init; } = "";

    [JsonPropertyName("roles")]
    public ICollection<string> Roles { get; init; } = [];

    [JsonPropertyName("permissions")]
    public ICollection<string> Permissions { get; init; } = [];
}
