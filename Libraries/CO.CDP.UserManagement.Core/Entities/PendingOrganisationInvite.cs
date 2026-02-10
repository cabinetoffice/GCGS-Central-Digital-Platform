using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.Core.Entities;

/// <summary>
/// Represents a pending organisation invite.
/// </summary>
public class PendingOrganisationInvite : IAuditable
{
    /// <summary>
    /// Gets or sets the unique identifier for this invite.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the invited user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the organisation identifier.
    /// </summary>
    public int OrganisationId { get; set; }

    /// <summary>
    /// Gets or sets the role for the invited user within the organisation.
    /// </summary>
    public OrganisationRole OrganisationRole { get; set; }

    /// <summary>
    /// Gets or sets the CDP person invite GUID for cross-referencing.
    /// </summary>
    public Guid CdpPersonInviteGuid { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who sent the invite.
    /// </summary>
    public string? InvitedBy { get; set; }

    // IAuditable
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }

    // Navigation properties
    public Organisation Organisation { get; set; } = null!;
}
