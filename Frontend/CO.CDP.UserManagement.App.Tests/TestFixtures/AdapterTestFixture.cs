using CO.CDP.Functional;
using CO.CDP.UserManagement.App.Adapters;
using CO.CDP.UserManagement.App.Models;
using CO.CDP.UserManagement.App.Services;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Responses;
using Moq;

namespace CO.CDP.UserManagement.App.Tests.TestFixtures;

public abstract class AdapterTestFixture
{
    protected static readonly Guid OrgGuid = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000000");
    protected static readonly int OrgId = 1;

    protected void SetupOrg(
        Mock<IUserManagementApiAdapter> adapter,
        string name = "Test Org") =>
        adapter
            .Setup(a => a.GetOrganisationBySlugAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganisationResponse
            {
                Slug = "test-org",
                CdpOrganisationGuid = OrgGuid,
                Id = OrgId,
                Name = name,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            });

    protected void SetupOrg()
    {
        var field = GetType().GetField("_adapter",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        if (field == null)
            throw new InvalidOperationException(
                "No _adapter field found on test class; pass the adapter to SetupOrg(adapter) instead.");
        var adapter = field.GetValue(this) as Mock<IUserManagementApiAdapter>;
        if (adapter == null)
            throw new InvalidOperationException("_adapter field is not Mock<IUserManagementApiAdapter>");
        SetupOrg(adapter);
    }


    protected static OrganisationUserResponse MakeUser(
        Guid? personId = null,
        OrganisationRole role = OrganisationRole.Member,
        string firstName = "Test", string lastName = "User",
        string email = "test@example.com",
        IReadOnlyList<UserAssignmentResponse>? applicationRoles = null) => new()
    {
        MembershipId = 1,
        OrganisationId = OrgId,
        CdpPersonId = personId ?? Guid.NewGuid(),
        FirstName = firstName,
        LastName = lastName,
        Email = email,
        OrganisationRole = role,
        Status = UserStatus.Active,
        IsActive = true,
        CreatedAt = DateTimeOffset.UtcNow,
        ApplicationAssignments = applicationRoles ?? new List<UserAssignmentResponse>()
    };

    protected static PendingOrganisationInviteResponse MakeInvite(
        Guid? inviteGuid = null, int pendingInviteId = 1,
        string email = "invite@example.com",
        OrganisationRole role = OrganisationRole.Member,
        string firstName = "Invite", string lastName = "User") => new()
    {
        PendingInviteId = pendingInviteId,
        OrganisationId = OrgId,
        CdpPersonInviteGuid = inviteGuid ?? Guid.NewGuid(),
        Email = email,
        FirstName = firstName,
        LastName = lastName,
        OrganisationRole = role,
        Status = UserStatus.Pending,
        CreatedAt = DateTimeOffset.UtcNow,
        ApplicationAssignments = new List<InviteApplicationAssignmentResponse>()
    };

    protected static OrganisationApplicationResponse MakeApplication(
        int orgAppId = 1, int appId = 1, string name = "Test App",
        bool allowsMultipleRoles = false) => new()
    {
        Id = orgAppId,
        OrganisationId = OrgId,
        ApplicationId = appId,
        Application = new ApplicationResponse
        {
            Id = appId,
            Name = name,
            ClientId = $"app-{appId}",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        },
        IsActive = true,
        CreatedAt = DateTimeOffset.UtcNow,
        CreatedBy = "test"
    };

    protected static RoleResponse MakeRole(int id = 1, string name = "Test Role", int applicationId = 1) => new()
    {
        Id = id,
        ApplicationId = applicationId,
        Name = name,
        IsActive = true,
        CreatedAt = DateTimeOffset.UtcNow,
        CreatedBy = "test"
    };

    protected static InviteUserState MakeState(
        string email = "test@example.com",
        string firstName = "Test", string lastName = "User",
        OrganisationRole role = OrganisationRole.Member,
        IReadOnlyList<InviteApplicationAssignment>? assignments = null) =>
        new("test-org", email, firstName, lastName, role, assignments);

    protected static Result<ServiceFailure, ServiceOutcome> SuccessResult()
        => Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.Success);

    protected static Result<ServiceFailure, ServiceOutcome> NotFoundResult()
        => Result<ServiceFailure, ServiceOutcome>.Success(ServiceOutcome.NotFound);

    protected static Result<ServiceFailure, ServiceOutcome> FailureResult(ServiceFailure failure)
        => Result<ServiceFailure, ServiceOutcome>.Failure(failure);
}