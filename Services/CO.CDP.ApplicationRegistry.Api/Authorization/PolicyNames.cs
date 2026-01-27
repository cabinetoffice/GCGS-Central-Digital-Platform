namespace CO.CDP.ApplicationRegistry.Api.Authorization;

/// <summary>
/// Constants for authorization policy names.
/// </summary>
public static class PolicyNames
{
    /// <summary>
    /// Platform administrator policy - for platform-wide administrative actions.
    /// </summary>
    public const string PlatformAdmin = "PlatformAdmin";

    /// <summary>
    /// Service account policy - for service-to-service authentication.
    /// </summary>
    public const string ServiceAccount = "ServiceAccount";

    /// <summary>
    /// Organisation member policy - for users who are members of an organisation.
    /// </summary>
    public const string OrganisationMember = "OrganisationMember";

    /// <summary>
    /// Organisation admin policy - for users who are admins/owners of an organisation.
    /// </summary>
    public const string OrganisationAdmin = "OrganisationAdmin";
}
