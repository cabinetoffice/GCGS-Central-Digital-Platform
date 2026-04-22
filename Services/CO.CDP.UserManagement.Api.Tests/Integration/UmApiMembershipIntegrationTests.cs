using System.Net;
using System.Net.Http.Json;
using CO.CDP.MQ;
using CO.CDP.MQ.Outbox;
using CO.CDP.TestKit.Mvc;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Core.Models;
using CO.CDP.UserManagement.Infrastructure.Data;
using CO.CDP.UserManagement.Infrastructure.Events;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit.Abstractions;
using UmOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;
using UmMembership = CO.CDP.UserManagement.Core.Entities.UserOrganisationMembership;
using UmOrgApplication = CO.CDP.UserManagement.Core.Entities.OrganisationApplication;
using UmInviteRoleMapping = CO.CDP.UserManagement.Core.Entities.InviteRoleMapping;
using CorePartyRole = CO.CDP.UserManagement.Core.Constants.PartyRole;

namespace CO.CDP.UserManagement.Api.Tests.Integration;

/// <summary>
/// HTTP-level integration tests that call the <c>OrganisationUsersController</c> endpoints
/// and assert the resulting state in the User Management database and outbox.
///
/// Uses a custom factory that preserves the real <see cref="CO.CDP.MQ.IPublisher"/> so that
/// outbox messages are actually written to the test-container database.
///
/// OI database changes happen asynchronously via events consumed by Organisation.WebApi;
/// those are not asserted here.
/// </summary>
public class UmApiMembershipIntegrationTests
    : IClassFixture<UserManagementPostgreSqlFixture>
{
    private readonly HttpClient _httpClient;
    private readonly Mock<IOrganisationApiAdapter> _mockOrgAdapter;
    private readonly string _umConnectionString;
    private readonly UserManagementDbContext _umContext;
    private readonly UserManagementPostgreSqlFixture _umPostgreSql;

    public UmApiMembershipIntegrationTests(
        ITestOutputHelper testOutputHelper,
        UserManagementPostgreSqlFixture umPostgreSql)
    {
        _umPostgreSql = umPostgreSql;
        _umConnectionString = umPostgreSql.ConnectionString;

        _mockOrgAdapter = new Mock<IOrganisationApiAdapter>();
        _mockOrgAdapter
            .Setup(a => a.GetPartyRolesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<CorePartyRole>());
        _mockOrgAdapter
            .Setup(a => a.GetOrganisationPersonsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<OiOrganisationPerson>());

        var mockPersonAdapter = new Mock<IPersonApiAdapter>();

        var factory = new UmMembershipWebFactory(
            umPostgreSql,
            testOutputHelper,
            _mockOrgAdapter,
            mockPersonAdapter);

        _httpClient = factory.CreateClient();
        _umContext = umPostgreSql.UserManagementContext();
    }

    // ─────────────────────── setup helpers ───────────────────────

    private void ClearDatabase()
    {
        _umContext.UserApplicationAssignments.RemoveRange(
            _umContext.UserApplicationAssignments.IgnoreQueryFilters().ToList());
        _umContext.UserOrganisationMemberships.RemoveRange(
            _umContext.UserOrganisationMemberships.IgnoreQueryFilters().ToList());
        _umContext.InviteRoleMappings.RemoveRange(
            _umContext.InviteRoleMappings.IgnoreQueryFilters().ToList());
        _umContext.OrganisationApplications.RemoveRange(
            _umContext.OrganisationApplications.IgnoreQueryFilters().ToList());
        _umContext.Organisations.RemoveRange(_umContext.Organisations);
        _umContext.OutboxMessages.RemoveRange(_umContext.OutboxMessages);
        _umContext.SaveChanges();
    }

    private UmOrganisation CreateUmOrganisation(Guid cdpGuid, string name)
    {
        var org = new UmOrganisation
        {
            CdpOrganisationGuid = cdpGuid,
            Name = name,
            Slug = $"{name.ToLower().Replace(" ", "-")}-{Guid.NewGuid():N}",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            ModifiedAt = DateTimeOffset.UtcNow,
            CreatedBy = "test",
            ModifiedBy = "test"
        };
        _umContext.Organisations.Add(org);
        _umContext.SaveChanges();
        return org;
    }

    private UmMembership CreateUmMembership(UmOrganisation org, Guid cdpPersonGuid,
        OrganisationRole role = OrganisationRole.Member)
    {
        var membership = new UmMembership
        {
            UserPrincipalId = $"urn:fdc:test:{Guid.NewGuid():N}",
            CdpPersonId = cdpPersonGuid,
            OrganisationId = org.Id,
            OrganisationRoleId = (int)role,
            IsActive = true,
            JoinedAt = DateTimeOffset.UtcNow,
            CreatedBy = "test"
        };
        _umContext.UserOrganisationMemberships.Add(membership);
        _umContext.SaveChanges();
        return membership;
    }

    private UmOrgApplication CreateOrganisationApplication(UmOrganisation org, int applicationId)
    {
        var orgApp = new UmOrgApplication
        {
            OrganisationId = org.Id,
            ApplicationId = applicationId,
            IsActive = true,
            EnabledAt = DateTimeOffset.UtcNow,
            EnabledBy = "test",
            CreatedAt = DateTimeOffset.UtcNow,
            ModifiedAt = DateTimeOffset.UtcNow,
            CreatedBy = "test",
            ModifiedBy = "test"
        };
        _umContext.OrganisationApplications.Add(orgApp);
        _umContext.SaveChanges();
        return orgApp;
    }

    private UmInviteRoleMapping CreateInviteRoleMapping(UmOrganisation org, OrganisationRole role)
    {
        var mapping = new UmInviteRoleMapping
        {
            CdpPersonInviteGuid = Guid.NewGuid(),
            OrganisationId = org.Id,
            OrganisationRoleId = (int)role,
            CreatedAt = DateTimeOffset.UtcNow,
            ModifiedAt = DateTimeOffset.UtcNow,
            CreatedBy = "test"
        };
        _umContext.InviteRoleMappings.Add(mapping);
        _umContext.SaveChanges();
        return mapping;
    }

    // ─────────────────────── tests ───────────────────────

    /// <summary>
    /// Verifies that <c>DELETE /api/organisations/{org}/users/{person}</c> soft-deletes the UM
    /// membership and publishes a <c>PersonRemovedFromOrganisation</c> outbox event.
    /// OI cleanup happens asynchronously when Organisation.WebApi processes the event.
    /// </summary>
    [Fact]
    public async Task RemoveUser_SoftDeletesUmMembershipAndPublishesOutboxEvent()
    {
        ClearDatabase();
        var cdpOrgGuid = Guid.NewGuid();
        var cdpPersonGuid = Guid.NewGuid();

        var umOrg = CreateUmOrganisation(cdpOrgGuid, $"Remove Test Org {cdpOrgGuid:N}");
        CreateUmMembership(umOrg, cdpPersonGuid);

        var response = await _httpClient.DeleteAsync(
            $"/api/organisations/{cdpOrgGuid}/users/{cdpPersonGuid}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "remove user should succeed with 204");

        await using var umAssert = new UserManagementDbContext(
            new DbContextOptionsBuilder<UserManagementDbContext>()
                .UseNpgsql(_umConnectionString)
                .Options);

        var membership = await umAssert.UserOrganisationMemberships
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.CdpPersonId == cdpPersonGuid);

        membership.Should().NotBeNull();
        membership!.IsActive.Should().BeFalse("UM membership should be deactivated");
        membership.IsDeleted.Should().BeTrue("UM membership should be soft-deleted");

        var outboxEvent = await umAssert.OutboxMessages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Type == nameof(PersonRemovedFromOrganisation));

        outboxEvent.Should().NotBeNull("a PersonRemovedFromOrganisation event should be queued in the outbox");
        outboxEvent!.Message.Should().Contain(cdpOrgGuid.ToString());
        outboxEvent.Message.Should().Contain(cdpPersonGuid.ToString());
    }

    /// <summary>
    /// Verifies that <c>PUT /api/organisations/{org}/users/{person}/role</c> updates the
    /// UM membership's organisation role and publishes a <c>PersonScopesUpdated</c> outbox event.
    /// </summary>
    [Fact]
    public async Task UpdateRole_ToAdmin_UpdatesUmMembershipRole()
    {
        ClearDatabase();
        var cdpOrgGuid = Guid.NewGuid();
        var cdpPersonGuid = Guid.NewGuid();

        var umOrg = CreateUmOrganisation(cdpOrgGuid, $"Role Test Org {cdpOrgGuid:N}");
        CreateUmMembership(umOrg, cdpPersonGuid);

        var request = new ChangeOrganisationRoleRequest
        {
            OrganisationRole = OrganisationRole.Admin
        };

        var response = await _httpClient.PutAsJsonAsync(
            $"/api/organisations/{cdpOrgGuid}/users/{cdpPersonGuid}/role",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "update role should succeed with 204");

        await using var umAssert = new UserManagementDbContext(
            new DbContextOptionsBuilder<UserManagementDbContext>()
                .UseNpgsql(_umConnectionString)
                .Options);

        var membership = await umAssert.UserOrganisationMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.CdpPersonId == cdpPersonGuid);

        membership.Should().NotBeNull();
        membership!.OrganisationRole.Should().Be(OrganisationRole.Admin,
            "UM membership role should be updated to Admin");

        var outboxEvent = await umAssert.OutboxMessages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Type == nameof(PersonScopesUpdated));

        outboxEvent.Should().NotBeNull("a PersonScopesUpdated event should be queued in the outbox");
    }

    /// <summary>
    /// Verifies that calling <c>DELETE /api/organisations/{org}/users/{person}</c> on an
    /// already-removed user returns 204 without throwing — the operation is idempotent.
    /// </summary>
    [Fact]
    public async Task RemoveUser_IsIdempotent_WhenAlreadyRemoved()
    {
        ClearDatabase();
        var cdpOrgGuid = Guid.NewGuid();
        var cdpPersonGuid = Guid.NewGuid();

        var umOrg = CreateUmOrganisation(cdpOrgGuid, $"Idempotent Test Org {cdpOrgGuid:N}");
        CreateUmMembership(umOrg, cdpPersonGuid);

        // First removal
        var first = await _httpClient.DeleteAsync(
            $"/api/organisations/{cdpOrgGuid}/users/{cdpPersonGuid}");
        first.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Second removal — should also return 204, not 404 or 500
        var second = await _httpClient.DeleteAsync(
            $"/api/organisations/{cdpOrgGuid}/users/{cdpPersonGuid}");
        second.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "removing an already-removed user should be idempotent");
    }

    /// <summary>
    /// Verifies that demoting a user from Admin to Member updates the UM membership role
    /// and publishes a <c>PersonScopesUpdated</c> outbox event.
    /// </summary>
    [Fact]
    public async Task UpdateRole_ToMember_DemotesUmMembershipAndPublishesOutboxEvent()
    {
        ClearDatabase();
        var cdpOrgGuid = Guid.NewGuid();
        var cdpPersonGuid = Guid.NewGuid();

        var umOrg = CreateUmOrganisation(cdpOrgGuid, $"Demote Test Org {cdpOrgGuid:N}");
        CreateUmMembership(umOrg, cdpPersonGuid, OrganisationRole.Admin);

        var request = new ChangeOrganisationRoleRequest { OrganisationRole = OrganisationRole.Member };

        var response = await _httpClient.PutAsJsonAsync(
            $"/api/organisations/{cdpOrgGuid}/users/{cdpPersonGuid}/role",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "demoting to Member should succeed with 204");

        await using var umAssert = new UserManagementDbContext(
            new DbContextOptionsBuilder<UserManagementDbContext>()
                .UseNpgsql(_umConnectionString)
                .Options);

        var membership = await umAssert.UserOrganisationMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.CdpPersonId == cdpPersonGuid);

        membership.Should().NotBeNull();
        membership!.OrganisationRole.Should().Be(OrganisationRole.Member,
            "UM membership role should be demoted to Member");

        var outboxEvent = await umAssert.OutboxMessages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Type == nameof(PersonScopesUpdated));

        outboxEvent.Should().NotBeNull("a PersonScopesUpdated event should be queued in the outbox");
        outboxEvent!.Message.Should().Contain(cdpOrgGuid.ToString());
        outboxEvent.Message.Should().Contain(cdpPersonGuid.ToString());
    }

    /// <summary>
    /// Verifies that <c>POST /api/organisations/{orgId}/users/{userId}/assignments</c> creates
    /// a <c>UserApplicationAssignment</c> and publishes a <c>PersonScopesUpdated</c> outbox event.
    /// Uses the <c>payments</c> application (seeded by migration, IsEnabledByDefault=false).
    /// </summary>
    [Fact]
    public async Task AssignApplication_CreatesUmAssignmentAndPublishesOutboxEvent()
    {
        ClearDatabase();
        var cdpOrgGuid = Guid.NewGuid();
        var cdpPersonGuid = Guid.NewGuid();

        // payments has IsEnabledByDefault=false, safe to use for assignment/revoke tests
        var paymentsApp = await _umContext.Applications.AsNoTracking()
            .FirstAsync(a => a.ClientId == "payments");

        var paymentsRole = await _umContext.ApplicationRoles.AsNoTracking()
            .FirstAsync(r => r.ApplicationId == paymentsApp.Id && r.IsActive);

        var umOrg = CreateUmOrganisation(cdpOrgGuid, $"Assign App Test Org {cdpOrgGuid:N}");
        var membership = CreateUmMembership(umOrg, cdpPersonGuid);
        CreateOrganisationApplication(umOrg, paymentsApp.Id);

        var request = new AssignUserToApplicationRequest
        {
            ApplicationId = paymentsApp.Id,
            RoleIds = [paymentsRole.Id]
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"/api/organisations/{umOrg.Id}/users/{membership.UserPrincipalId}/assignments",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.Created,
            "assigning a user to an application should return 201");

        await using var umAssert = new UserManagementDbContext(
            new DbContextOptionsBuilder<UserManagementDbContext>()
                .UseNpgsql(_umConnectionString)
                .Options);

        var assignment = await umAssert.UserApplicationAssignments
            .Include(a => a.OrganisationApplication)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserOrganisationMembershipId == membership.Id);

        assignment.Should().NotBeNull("a UserApplicationAssignment row should be created");
        assignment!.IsActive.Should().BeTrue();
        assignment.OrganisationApplication.ApplicationId.Should().Be(paymentsApp.Id);

        var outboxEvent = await umAssert.OutboxMessages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Type == nameof(PersonScopesUpdated));

        outboxEvent.Should().NotBeNull("a PersonScopesUpdated event should be queued in the outbox");
        outboxEvent!.Message.Should().Contain(cdpOrgGuid.ToString());
        outboxEvent.Message.Should().Contain(cdpPersonGuid.ToString());
    }

    /// <summary>
    /// Verifies that <c>DELETE /api/organisations/{orgId}/users/{userId}/assignments/{id}</c>
    /// deactivates the assignment and publishes a <c>PersonScopesUpdated</c> outbox event.
    /// Uses <c>payments</c> (IsEnabledByDefault=false) because the use case blocks revocation
    /// of default-enabled applications.
    /// </summary>
    [Fact]
    public async Task RevokeApplication_DeactivatesUmAssignmentAndPublishesOutboxEvent()
    {
        ClearDatabase();
        var cdpOrgGuid = Guid.NewGuid();
        var cdpPersonGuid = Guid.NewGuid();

        var paymentsApp = await _umContext.Applications.AsNoTracking()
            .FirstAsync(a => a.ClientId == "payments");

        var umOrg = CreateUmOrganisation(cdpOrgGuid, $"Revoke App Test Org {cdpOrgGuid:N}");
        var membership = CreateUmMembership(umOrg, cdpPersonGuid);
        var orgApp = CreateOrganisationApplication(umOrg, paymentsApp.Id);

        var assignment = new UserApplicationAssignment
        {
            UserOrganisationMembershipId = membership.Id,
            OrganisationApplicationId = orgApp.Id,
            IsActive = true,
            AssignedAt = DateTimeOffset.UtcNow,
            AssignedBy = "test",
            CreatedAt = DateTimeOffset.UtcNow,
            ModifiedAt = DateTimeOffset.UtcNow,
            CreatedBy = "test"
        };
        _umContext.UserApplicationAssignments.Add(assignment);
        _umContext.SaveChanges();

        var response = await _httpClient.DeleteAsync(
            $"/api/organisations/{umOrg.Id}/users/{membership.UserPrincipalId}/assignments/{assignment.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "revoking an assignment should return 204");

        await using var umAssert = new UserManagementDbContext(
            new DbContextOptionsBuilder<UserManagementDbContext>()
                .UseNpgsql(_umConnectionString)
                .Options);

        var revokedAssignment = await umAssert.UserApplicationAssignments
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == assignment.Id);

        revokedAssignment.Should().NotBeNull();
        revokedAssignment!.IsActive.Should().BeFalse("the assignment should be deactivated");
        revokedAssignment.RevokedAt.Should().NotBeNull("RevokedAt should be set on revocation");

        var outboxEvent = await umAssert.OutboxMessages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Type == nameof(PersonScopesUpdated));

        outboxEvent.Should().NotBeNull("a PersonScopesUpdated event should be queued in the outbox");
        outboxEvent!.Message.Should().Contain(cdpOrgGuid.ToString());
        outboxEvent.Message.Should().Contain(cdpPersonGuid.ToString());
    }

    /// <summary>
    /// Verifies that <c>POST /api/organisations/{org}/invites/{id}/accept</c> creates a UM
    /// membership, soft-deletes the <c>InviteRoleMapping</c>, and publishes a
    /// <c>PersonScopesUpdated</c> outbox event.
    /// </summary>
    [Fact]
    public async Task AcceptInvite_CreatesUmMembershipAndPublishesOutboxEvent()
    {
        ClearDatabase();
        var cdpOrgGuid = Guid.NewGuid();
        var cdpPersonGuid = Guid.NewGuid();
        var userPrincipalId = $"urn:fdc:test:{Guid.NewGuid():N}";

        var umOrg = CreateUmOrganisation(cdpOrgGuid, $"Accept Invite Test Org {cdpOrgGuid:N}");
        // No OrganisationApplication created → AssignDefaultApplicationsAsync exits early,
        // avoiding the GetPartyRolesAsync call on the mocked IOrganisationApiAdapter.
        var mapping = CreateInviteRoleMapping(umOrg, OrganisationRole.Member);

        var request = new AcceptOrganisationInviteRequest
        {
            UserPrincipalId = userPrincipalId,
            CdpPersonId = cdpPersonGuid
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"/api/organisations/{cdpOrgGuid}/invites/{mapping.Id}/accept",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "accepting an invite should return 204");

        await using var umAssert = new UserManagementDbContext(
            new DbContextOptionsBuilder<UserManagementDbContext>()
                .UseNpgsql(_umConnectionString)
                .Options);

        var membership = await umAssert.UserOrganisationMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.CdpPersonId == cdpPersonGuid);

        membership.Should().NotBeNull("a UM membership should be created on invite acceptance");
        membership!.IsActive.Should().BeTrue();
        membership.UserPrincipalId.Should().Be(userPrincipalId);

        var consumedMapping = await umAssert.InviteRoleMappings
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == mapping.Id);

        consumedMapping!.IsDeleted.Should().BeTrue(
            "the InviteRoleMapping should be soft-deleted after acceptance");

        var outboxEvent = await umAssert.OutboxMessages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Type == nameof(PersonScopesUpdated));

        outboxEvent.Should().NotBeNull("a PersonScopesUpdated event should be queued in the outbox");
        outboxEvent!.Message.Should().Contain(cdpOrgGuid.ToString());
        outboxEvent.Message.Should().Contain(cdpPersonGuid.ToString());
    }

    /// <summary>
    /// Verifies that <c>GET /api/organisations/{org}/users</c> returns all active memberships
    /// for a known organisation, with correct roles populated.
    /// </summary>
    [Fact]
    public async Task GetUsers_ReturnsAllActiveMembers()
    {
        ClearDatabase();
        var cdpOrgGuid = Guid.NewGuid();

        var umOrg = CreateUmOrganisation(cdpOrgGuid, $"GetUsers Test Org {cdpOrgGuid:N}");
        CreateUmMembership(umOrg, Guid.NewGuid());
        CreateUmMembership(umOrg, Guid.NewGuid(), OrganisationRole.Admin);

        var response = await _httpClient.GetAsync($"/api/organisations/{cdpOrgGuid}/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK, "list endpoint should succeed with 200");

        var users = await response.Content.ReadFromJsonAsync<List<OrganisationUserResponse>>();
        users.Should().NotBeNull().And.HaveCount(2, "both active memberships should be returned");

        var roles = users!.Select(u => u.OrganisationRole).ToList();
        roles.Should().Contain(OrganisationRole.Member);
        roles.Should().Contain(OrganisationRole.Admin);
    }

    /// <summary>
    /// Verifies that <c>GET /api/organisations/{org}/users/{userId}</c> returns 200 and
    /// the correct membership data when the member exists.
    /// </summary>
    [Fact]
    public async Task GetUser_ByPrincipalId_Returns200_WhenMemberExists()
    {
        ClearDatabase();
        var cdpOrgGuid = Guid.NewGuid();
        var cdpPersonGuid = Guid.NewGuid();

        var umOrg = CreateUmOrganisation(cdpOrgGuid, $"GetUser Test Org {cdpOrgGuid:N}");
        var membership = CreateUmMembership(umOrg, cdpPersonGuid, OrganisationRole.Admin);

        var response = await _httpClient.GetAsync(
            $"/api/organisations/{cdpOrgGuid}/users/{Uri.EscapeDataString(membership.UserPrincipalId)}");

        response.StatusCode.Should().Be(HttpStatusCode.OK, "get single user should succeed with 200");

        var user = await response.Content.ReadFromJsonAsync<OrganisationUserResponse>();
        user.Should().NotBeNull();
        user!.OrganisationRole.Should().Be(OrganisationRole.Admin,
            "the returned membership should carry the correct role");
        user.CdpPersonId.Should().Be(cdpPersonGuid);
    }

    /// <summary>
    /// Verifies that <c>GET /api/organisations/{org}/users/{userId}</c> returns 404
    /// when no membership exists for the given user principal ID.
    /// </summary>
    [Fact]
    public async Task GetUser_ByPrincipalId_Returns404_WhenMemberNotFound()
    {
        ClearDatabase();
        var cdpOrgGuid = Guid.NewGuid();

        CreateUmOrganisation(cdpOrgGuid, $"GetUser 404 Test Org {cdpOrgGuid:N}");

        var response = await _httpClient.GetAsync(
            $"/api/organisations/{cdpOrgGuid}/users/urn:fdc:test:nonexistent-user");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "get single user should return 404 when no membership exists for the principal ID");
    }

    /// <summary>
    /// Verifies that <c>DELETE /api/organisations/{org}/users/{person}</c> returns 404
    /// when the organisation GUID is not registered in UM.
    /// </summary>
    [Fact]
    public async Task RemoveUser_Returns404_WhenOrganisationNotFound()
    {
        ClearDatabase();
        var unknownOrgGuid = Guid.NewGuid();
        var cdpPersonGuid = Guid.NewGuid();

        var response = await _httpClient.DeleteAsync(
            $"/api/organisations/{unknownOrgGuid}/users/{cdpPersonGuid}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "remove user should return 404 when the organisation does not exist");
    }

    /// <summary>
    /// Verifies that <c>DELETE /api/organisations/{org}/users/{person}</c> returns 409 Conflict
    /// when the target user is the last Owner of the organisation.
    /// </summary>
    [Fact]
    public async Task RemoveUser_Returns409_WhenRemovingLastOwner()
    {
        ClearDatabase();
        var cdpOrgGuid = Guid.NewGuid();
        var cdpPersonGuid = Guid.NewGuid();

        var umOrg = CreateUmOrganisation(cdpOrgGuid, $"LastOwner Test Org {cdpOrgGuid:N}");
        CreateUmMembership(umOrg, cdpPersonGuid, OrganisationRole.Owner);

        var response = await _httpClient.DeleteAsync(
            $"/api/organisations/{cdpOrgGuid}/users/{cdpPersonGuid}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict,
            "removing the last owner should return 409 Conflict");
    }

    /// <summary>
    /// Verifies that <c>DELETE /api/organisations/{org}/users/{person}</c> also soft-deletes
    /// all active <c>UserApplicationAssignment</c> rows for the removed member.
    /// </summary>
    [Fact]
    public async Task RemoveUser_AlsoDeactivatesActiveApplicationAssignments()
    {
        ClearDatabase();
        var cdpOrgGuid = Guid.NewGuid();
        var cdpPersonGuid = Guid.NewGuid();

        var paymentsApp = await _umContext.Applications.AsNoTracking()
            .FirstAsync(a => a.ClientId == "payments");

        var umOrg = CreateUmOrganisation(cdpOrgGuid, $"Remove+Assign Test Org {cdpOrgGuid:N}");
        var membership = CreateUmMembership(umOrg, cdpPersonGuid);
        var orgApp = CreateOrganisationApplication(umOrg, paymentsApp.Id);

        var assignment = new UserApplicationAssignment
        {
            UserOrganisationMembershipId = membership.Id,
            OrganisationApplicationId = orgApp.Id,
            IsActive = true,
            AssignedAt = DateTimeOffset.UtcNow,
            AssignedBy = "test",
            CreatedAt = DateTimeOffset.UtcNow,
            ModifiedAt = DateTimeOffset.UtcNow,
            CreatedBy = "test"
        };
        _umContext.UserApplicationAssignments.Add(assignment);
        _umContext.SaveChanges();

        var response = await _httpClient.DeleteAsync(
            $"/api/organisations/{cdpOrgGuid}/users/{cdpPersonGuid}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "remove user should succeed with 204");

        await using var umAssert = new UserManagementDbContext(
            new DbContextOptionsBuilder<UserManagementDbContext>()
                .UseNpgsql(_umConnectionString)
                .Options);

        var revokedAssignment = await umAssert.UserApplicationAssignments
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == assignment.Id);

        revokedAssignment.Should().NotBeNull();
        revokedAssignment!.IsActive.Should().BeFalse(
            "the user's application assignment should be deactivated when the user is removed");
        revokedAssignment.RevokedAt.Should().NotBeNull(
            "RevokedAt should be set when the assignment is deactivated via user removal");
    }

    /// <summary>
    /// Verifies that <c>PUT /api/organisations/{org}/users/{person}/role</c> returns 404
    /// when the organisation GUID is not registered in UM.
    /// </summary>
    [Fact]
    public async Task UpdateRole_Returns404_WhenOrganisationNotFound()
    {
        ClearDatabase();
        var unknownOrgGuid = Guid.NewGuid();
        var cdpPersonGuid = Guid.NewGuid();

        var request = new ChangeOrganisationRoleRequest { OrganisationRole = OrganisationRole.Admin };

        var response = await _httpClient.PutAsJsonAsync(
            $"/api/organisations/{unknownOrgGuid}/users/{cdpPersonGuid}/role",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "update role should return 404 when the organisation does not exist");
    }

    /// <summary>
    /// Verifies that <c>PUT /api/organisations/{org}/users/{person}/role</c> returns 404
    /// when the person is not a member of the (existing) organisation.
    /// </summary>
    [Fact]
    public async Task UpdateRole_Returns404_WhenMemberNotFound()
    {
        ClearDatabase();
        var cdpOrgGuid = Guid.NewGuid();
        var unknownPersonGuid = Guid.NewGuid();

        CreateUmOrganisation(cdpOrgGuid, $"UpdateRole 404 Test Org {cdpOrgGuid:N}");

        var request = new ChangeOrganisationRoleRequest { OrganisationRole = OrganisationRole.Admin };

        var response = await _httpClient.PutAsJsonAsync(
            $"/api/organisations/{cdpOrgGuid}/users/{unknownPersonGuid}/role",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "update role should return 404 when the person is not a member of the organisation");
    }

    /// <summary>
    /// Verifies that <c>POST /api/organisations/{org}/invites/{id}/accept</c> returns 404
    /// when the <c>InviteRoleMapping</c> does not exist.
    /// </summary>
    [Fact]
    public async Task AcceptInvite_Returns404_WhenInviteNotFound()
    {
        ClearDatabase();
        var cdpOrgGuid = Guid.NewGuid();
        var cdpPersonGuid = Guid.NewGuid();

        CreateUmOrganisation(cdpOrgGuid, $"AcceptInvite 404 Test Org {cdpOrgGuid:N}");

        var request = new AcceptOrganisationInviteRequest
        {
            UserPrincipalId = $"urn:fdc:test:{Guid.NewGuid():N}",
            CdpPersonId = cdpPersonGuid
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"/api/organisations/{cdpOrgGuid}/invites/99999/accept",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "accept invite should return 404 when the invite does not exist");
    }

    /// <summary>
    /// Verifies that <c>POST /api/organisations/{org}/invites/{id}/accept</c> creates a UM
    /// membership with the role specified on the <c>InviteRoleMapping</c> (Admin in this case),
    /// not a fixed default.
    /// </summary>
    [Fact]
    public async Task AcceptInvite_WithAdminRole_SetsMembershipRoleToAdmin()
    {
        ClearDatabase();
        var cdpOrgGuid = Guid.NewGuid();
        var cdpPersonGuid = Guid.NewGuid();
        var userPrincipalId = $"urn:fdc:test:{Guid.NewGuid():N}";

        var umOrg = CreateUmOrganisation(cdpOrgGuid, $"AcceptInvite Admin Test Org {cdpOrgGuid:N}");
        var mapping = CreateInviteRoleMapping(umOrg, OrganisationRole.Admin);

        var request = new AcceptOrganisationInviteRequest
        {
            UserPrincipalId = userPrincipalId,
            CdpPersonId = cdpPersonGuid
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"/api/organisations/{cdpOrgGuid}/invites/{mapping.Id}/accept",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent,
            "accepting an admin invite should return 204");

        await using var umAssert = new UserManagementDbContext(
            new DbContextOptionsBuilder<UserManagementDbContext>()
                .UseNpgsql(_umConnectionString)
                .Options);

        var membership = await umAssert.UserOrganisationMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.CdpPersonId == cdpPersonGuid);

        membership.Should().NotBeNull("a UM membership should be created on invite acceptance");
        membership!.OrganisationRole.Should().Be(OrganisationRole.Admin,
            "the created membership should carry the Admin role from the InviteRoleMapping");

        var outboxEvent = await umAssert.OutboxMessages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Type == nameof(PersonScopesUpdated));

        outboxEvent.Should().NotBeNull("a PersonScopesUpdated event should be queued in the outbox");
        outboxEvent!.Message.Should().Contain(cdpOrgGuid.ToString());
        outboxEvent.Message.Should().Contain(cdpPersonGuid.ToString());
    }

    /// <summary>
    /// Shared implementation for FTS role-change tests.  Creates an org + membership + FTS
    /// application, assigns the named FTS role via
    /// <c>PUT /api/organisations/{id}/users/{upi}/assignments/{id}</c>, and asserts that the
    /// resulting <c>PersonScopesUpdated</c> outbox event contains every scope in
    /// <paramref name="expectedScopes"/>.
    /// </summary>
    private async Task AssertFtsRoleUpdatePublishesScopes(string ftsRoleName, string[] expectedScopes)
    {
        ClearDatabase();
        var cdpOrgGuid = Guid.NewGuid();
        var cdpPersonGuid = Guid.NewGuid();

        var ftsApp = await _umContext.Applications.AsNoTracking()
            .FirstAsync(a => a.ClientId == "find-a-tender");
        var targetRole = await _umContext.ApplicationRoles.AsNoTracking()
            .FirstAsync(r => r.ApplicationId == ftsApp.Id && r.Name == ftsRoleName && r.IsActive);

        _mockOrgAdapter
            .Setup(a => a.GetPartyRolesAsync(cdpOrgGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<CorePartyRole>(Enum.GetValues<CorePartyRole>()));

        var umOrg = CreateUmOrganisation(cdpOrgGuid, $"FTS Test Org {cdpOrgGuid:N}");
        var membership = CreateUmMembership(umOrg, cdpPersonGuid);
        var orgApp = CreateOrganisationApplication(umOrg, ftsApp.Id);

        var assignment = new UserApplicationAssignment
        {
            UserOrganisationMembershipId = membership.Id,
            OrganisationApplicationId = orgApp.Id,
            IsActive = true,
            AssignedAt = DateTimeOffset.UtcNow,
            AssignedBy = "test",
            CreatedAt = DateTimeOffset.UtcNow,
            ModifiedAt = DateTimeOffset.UtcNow,
            CreatedBy = "test"
        };
        _umContext.UserApplicationAssignments.Add(assignment);
        _umContext.SaveChanges();

        var request = new UpdateAssignmentRolesRequest { RoleIds = [targetRole.Id] };

        var response = await _httpClient.PutAsJsonAsync(
            $"/api/organisations/{umOrg.Id}/users/{membership.UserPrincipalId}/assignments/{assignment.Id}",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.OK,
            $"updating FTS assignment to '{ftsRoleName}' should return 200");

        await using var umAssert = new UserManagementDbContext(
            new DbContextOptionsBuilder<UserManagementDbContext>()
                .UseNpgsql(_umConnectionString)
                .Options);

        var outboxEvent = await umAssert.OutboxMessages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Type == nameof(PersonScopesUpdated));

        outboxEvent.Should().NotBeNull("a PersonScopesUpdated event should be queued in the outbox");
        outboxEvent!.Message.Should().Contain(cdpOrgGuid.ToString());
        outboxEvent.Message.Should().Contain(cdpPersonGuid.ToString());

        foreach (var scope in expectedScopes)
            outboxEvent.Message.Should().Contain($"\"{scope}\"",
                $"the PersonScopesUpdated message should contain the scope '{scope}'");
    }

    /// <summary>
    /// FTS Viewer (buyer) → OI scopes should be VIEWER + RESPONDER.
    /// </summary>
    [Fact]
    public Task UpdateFtsAssignment_ToViewerBuyer_PublishesViewerAndResponderScopes() =>
        AssertFtsRoleUpdatePublishesScopes("Viewer (buyer)", ["VIEWER", "RESPONDER"]);

    /// <summary>
    /// FTS Viewer (supplier) → OI scopes should be VIEWER + RESPONDER.
    /// </summary>
    [Fact]
    public Task UpdateFtsAssignment_ToViewerSupplier_PublishesViewerAndResponderScopes() =>
        AssertFtsRoleUpdatePublishesScopes("Viewer (supplier)", ["VIEWER", "RESPONDER"]);

    /// <summary>
    /// FTS Editor (buyer) → OI scopes should be ADMIN + RESPONDER.
    /// </summary>
    [Fact]
    public Task UpdateFtsAssignment_ToEditorBuyer_PublishesAdminAndResponderScopes() =>
        AssertFtsRoleUpdatePublishesScopes("Editor (buyer)", ["ADMIN", "RESPONDER"]);

    /// <summary>
    /// FTS Editor (supplier) → OI scopes should be EDITOR + RESPONDER.
    /// </summary>
    [Fact]
    public Task UpdateFtsAssignment_ToEditorSupplier_PublishesEditorAndResponderScopes() =>
        AssertFtsRoleUpdatePublishesScopes("Editor (supplier)", ["EDITOR", "RESPONDER"]);

    // ─────────────────────── outbox atomicity tests ───────────────────────

    /// <summary>
    /// Verifies that soft-deleting a user membership and writing its <c>PersonRemovedFromOrganisation</c>
    /// outbox row are atomic. When the outbox write fails, the soft-delete must also be rolled back.
    ///
    /// <c>RemovePersonFromOrganisationUseCase</c> follows the correct outbox convention: entity changes
    /// are tracked first and then saved atomically alongside the outbox row via a single
    /// <c>SaveChangesAsync</c> call inside <c>publisher.Publish()</c>.
    /// </summary>
    [Fact]
    public async Task RemoveUser_RollsBackSoftDelete_WhenOutboxWriteFails()
    {
        ClearDatabase();
        var cdpOrgGuid = Guid.NewGuid();
        var cdpPersonGuid = Guid.NewGuid();

        var umOrg = CreateUmOrganisation(cdpOrgGuid, $"Atomicity Remove Org {cdpOrgGuid:N}");
        CreateUmMembership(umOrg, cdpPersonGuid);

        var factory = new UmMembershipAtomicityWebFactory(_umPostgreSql, _mockOrgAdapter);
        var client = factory.CreateClient();

        var response = await client.DeleteAsync(
            $"/api/organisations/{cdpOrgGuid}/users/{cdpPersonGuid}");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError,
            "the endpoint should fail when the outbox write throws");

        await using var umAssert = new UserManagementDbContext(
            new DbContextOptionsBuilder<UserManagementDbContext>()
                .UseNpgsql(_umConnectionString)
                .Options);

        var membership = await umAssert.UserOrganisationMemberships
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.CdpPersonId == cdpPersonGuid);

        membership.Should().NotBeNull();
        membership!.IsActive.Should().BeTrue(
            "the soft-delete must be rolled back when the outbox write fails — they must be atomic");
        membership.IsDeleted.Should().BeFalse(
            "the membership must not be soft-deleted when the outbox write fails — they must be atomic");
    }

    /// <summary>
    /// Verifies that updating a membership role and writing its <c>PersonScopesUpdated</c>
    /// outbox row are atomic. When the outbox write fails, the role change must also be rolled back.
    ///
    /// <c>UpdateOrganisationRoleUseCase</c> follows the correct outbox convention: entity changes
    /// are tracked first and then saved atomically alongside the outbox row via a single
    /// <c>SaveChangesAsync</c> call inside <c>publisher.Publish()</c>.
    /// </summary>
    [Fact]
    public async Task UpdateRole_RollsBackRoleChange_WhenOutboxWriteFails()
    {
        ClearDatabase();
        var cdpOrgGuid = Guid.NewGuid();
        var cdpPersonGuid = Guid.NewGuid();

        var umOrg = CreateUmOrganisation(cdpOrgGuid, $"Atomicity UpdateRole Org {cdpOrgGuid:N}");
        CreateUmMembership(umOrg, cdpPersonGuid);

        var factory = new UmMembershipAtomicityWebFactory(_umPostgreSql, _mockOrgAdapter);
        var client = factory.CreateClient();

        var request = new ChangeOrganisationRoleRequest { OrganisationRole = OrganisationRole.Admin };
        var response = await client.PutAsJsonAsync(
            $"/api/organisations/{cdpOrgGuid}/users/{cdpPersonGuid}/role",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError,
            "the endpoint should fail when the outbox write throws");

        await using var umAssert = new UserManagementDbContext(
            new DbContextOptionsBuilder<UserManagementDbContext>()
                .UseNpgsql(_umConnectionString)
                .Options);

        var membership = await umAssert.UserOrganisationMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.CdpPersonId == cdpPersonGuid);

        membership.Should().NotBeNull();
        membership!.OrganisationRole.Should().Be(OrganisationRole.Member,
            "the role change must be rolled back when the outbox write fails — they must be atomic");
    }

    /// <summary>
    /// Verifies that assigning a user to an application and writing its <c>PersonScopesUpdated</c>
    /// outbox row are atomic. When the outbox write fails, the assignment must also be rolled back.
    ///
    /// All three steps — entity save, scope DB query, outbox write — run inside a single explicit
    /// DB transaction via <c>ExecuteInTransactionAsync</c>. If the outbox write throws, the
    /// whole transaction rolls back.
    /// </summary>
    [Fact]
    public async Task AssignApplication_RollsBackAssignment_WhenOutboxWriteFails()
    {
        ClearDatabase();
        var cdpOrgGuid = Guid.NewGuid();
        var cdpPersonGuid = Guid.NewGuid();

        var paymentsApp = await _umContext.Applications.AsNoTracking()
            .FirstAsync(a => a.ClientId == "payments");
        var paymentsRole = await _umContext.ApplicationRoles.AsNoTracking()
            .FirstAsync(r => r.ApplicationId == paymentsApp.Id && r.IsActive);

        var umOrg = CreateUmOrganisation(cdpOrgGuid, $"Atomicity Assign Org {cdpOrgGuid:N}");
        var membership = CreateUmMembership(umOrg, cdpPersonGuid);
        CreateOrganisationApplication(umOrg, paymentsApp.Id);

        var factory = new UmMembershipAtomicityWebFactory(_umPostgreSql, _mockOrgAdapter);
        var client = factory.CreateClient();

        var request = new AssignUserToApplicationRequest
        {
            ApplicationId = paymentsApp.Id,
            RoleIds = [paymentsRole.Id]
        };
        var response = await client.PostAsJsonAsync(
            $"/api/organisations/{umOrg.Id}/users/{membership.UserPrincipalId}/assignments",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError,
            "the endpoint should fail when the outbox write throws");

        await using var umAssert = new UserManagementDbContext(
            new DbContextOptionsBuilder<UserManagementDbContext>()
                .UseNpgsql(_umConnectionString)
                .Options);

        var assignment = await umAssert.UserApplicationAssignments
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserOrganisationMembershipId == membership.Id);

        assignment.Should().BeNull(
            "the assignment must be rolled back when the outbox write fails — they must be atomic");
    }

    /// <summary>
    /// Verifies that revoking an application assignment and writing its <c>PersonScopesUpdated</c>
    /// outbox row are atomic. When the outbox write fails, the revocation must also be rolled back.
    /// </summary>
    [Fact]
    public async Task RevokeApplication_RollsBackRevocation_WhenOutboxWriteFails()
    {
        ClearDatabase();
        var cdpOrgGuid = Guid.NewGuid();
        var cdpPersonGuid = Guid.NewGuid();

        var paymentsApp = await _umContext.Applications.AsNoTracking()
            .FirstAsync(a => a.ClientId == "payments");

        var umOrg = CreateUmOrganisation(cdpOrgGuid, $"Atomicity Revoke Org {cdpOrgGuid:N}");
        var membership = CreateUmMembership(umOrg, cdpPersonGuid);
        var orgApp = CreateOrganisationApplication(umOrg, paymentsApp.Id);

        var assignment = new UserApplicationAssignment
        {
            UserOrganisationMembershipId = membership.Id,
            OrganisationApplicationId = orgApp.Id,
            IsActive = true,
            AssignedAt = DateTimeOffset.UtcNow,
            AssignedBy = "test",
            CreatedAt = DateTimeOffset.UtcNow,
            ModifiedAt = DateTimeOffset.UtcNow,
            CreatedBy = "test"
        };
        _umContext.UserApplicationAssignments.Add(assignment);
        _umContext.SaveChanges();

        var factory = new UmMembershipAtomicityWebFactory(_umPostgreSql, _mockOrgAdapter);
        var client = factory.CreateClient();

        var response = await client.DeleteAsync(
            $"/api/organisations/{umOrg.Id}/users/{membership.UserPrincipalId}/assignments/{assignment.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError,
            "the endpoint should fail when the outbox write throws");

        await using var umAssert = new UserManagementDbContext(
            new DbContextOptionsBuilder<UserManagementDbContext>()
                .UseNpgsql(_umConnectionString)
                .Options);

        var notRevoked = await umAssert.UserApplicationAssignments
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == assignment.Id);

        notRevoked.Should().NotBeNull();
        notRevoked!.IsActive.Should().BeTrue(
            "the revocation must be rolled back when the outbox write fails — they must be atomic");
    }

    /// <summary>
    /// Verifies that updating an assignment's roles and writing its <c>PersonScopesUpdated</c>
    /// outbox row are atomic. When the outbox write fails, the role update must also be rolled back.
    /// </summary>
    [Fact]
    public async Task UpdateAssignment_RollsBackRoleUpdate_WhenOutboxWriteFails()
    {
        ClearDatabase();
        var cdpOrgGuid = Guid.NewGuid();
        var cdpPersonGuid = Guid.NewGuid();

        var paymentsApp = await _umContext.Applications.AsNoTracking()
            .FirstAsync(a => a.ClientId == "payments");
        var paymentsRoles = await _umContext.ApplicationRoles
            .Where(r => r.ApplicationId == paymentsApp.Id && r.IsActive)
            .ToListAsync();

        paymentsRoles.Should().HaveCountGreaterThan(1,
            "payments must have at least two roles for the update test to be meaningful");

        var initialRole = paymentsRoles[0];
        var updatedRole = paymentsRoles[1];

        var umOrg = CreateUmOrganisation(cdpOrgGuid, $"Atomicity UpdateAssign Org {cdpOrgGuid:N}");
        var membership = CreateUmMembership(umOrg, cdpPersonGuid);
        var orgApp = CreateOrganisationApplication(umOrg, paymentsApp.Id);

        var assignment = new UserApplicationAssignment
        {
            UserOrganisationMembershipId = membership.Id,
            OrganisationApplicationId = orgApp.Id,
            IsActive = true,
            AssignedAt = DateTimeOffset.UtcNow,
            AssignedBy = "test",
            CreatedAt = DateTimeOffset.UtcNow,
            ModifiedAt = DateTimeOffset.UtcNow,
            CreatedBy = "test",
            Roles = [initialRole]
        };
        _umContext.UserApplicationAssignments.Add(assignment);
        _umContext.SaveChanges();

        var factory = new UmMembershipAtomicityWebFactory(_umPostgreSql, _mockOrgAdapter);
        var client = factory.CreateClient();

        var request = new UpdateAssignmentRolesRequest { RoleIds = [updatedRole.Id] };
        var response = await client.PutAsJsonAsync(
            $"/api/organisations/{umOrg.Id}/users/{membership.UserPrincipalId}/assignments/{assignment.Id}",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError,
            "the endpoint should fail when the outbox write throws");

        await using var umAssert = new UserManagementDbContext(
            new DbContextOptionsBuilder<UserManagementDbContext>()
                .UseNpgsql(_umConnectionString)
                .Options);

        var notUpdated = await umAssert.UserApplicationAssignments
            .Include(a => a.Roles)
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == assignment.Id);

        notUpdated.Should().NotBeNull();
        notUpdated!.Roles.Should().ContainSingle(r => r.Id == initialRole.Id,
            "the role update must be rolled back when the outbox write fails — they must be atomic");
        notUpdated.Roles.Should().NotContain(r => r.Id == updatedRole.Id,
            "the new role must not be persisted when the outbox write fails");
    }

    /// <summary>
    /// Verifies that accepting an invite (creating a membership + removing the mapping) and writing
    /// its <c>PersonScopesUpdated</c> outbox row are atomic. When the outbox write fails, all
    /// entity changes must also be rolled back.
    ///
    /// The entire <c>AcceptInviteUseCase.Execute</c> runs inside a single explicit DB transaction
    /// via <c>ExecuteInTransactionAsync</c>, so membership creation, mapping deletion, and the
    /// outbox write either all commit or all roll back together.
    /// </summary>
    [Fact]
    public async Task AcceptInvite_RollsBackMembership_WhenOutboxWriteFails()
    {
        ClearDatabase();
        var cdpOrgGuid = Guid.NewGuid();
        var cdpPersonGuid = Guid.NewGuid();
        var userPrincipalId = $"urn:fdc:test:{Guid.NewGuid():N}";

        var umOrg = CreateUmOrganisation(cdpOrgGuid, $"Atomicity AcceptInvite Org {cdpOrgGuid:N}");
        var mapping = CreateInviteRoleMapping(umOrg, OrganisationRole.Member);

        var factory = new UmMembershipAtomicityWebFactory(_umPostgreSql, _mockOrgAdapter);
        var client = factory.CreateClient();

        var request = new AcceptOrganisationInviteRequest
        {
            UserPrincipalId = userPrincipalId,
            CdpPersonId = cdpPersonGuid
        };
        var response = await client.PostAsJsonAsync(
            $"/api/organisations/{cdpOrgGuid}/invites/{mapping.Id}/accept",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError,
            "the endpoint should fail when the outbox write throws");

        await using var umAssert = new UserManagementDbContext(
            new DbContextOptionsBuilder<UserManagementDbContext>()
                .UseNpgsql(_umConnectionString)
                .Options);

        var membership = await umAssert.UserOrganisationMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.CdpPersonId == cdpPersonGuid);

        membership.Should().BeNull(
            "the membership creation must be rolled back when the outbox write fails — they must be atomic");

        var unconsumedMapping = await umAssert.InviteRoleMappings
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == mapping.Id);

        unconsumedMapping!.IsDeleted.Should().BeFalse(
            "the invite mapping soft-delete must be rolled back when the outbox write fails — they must be atomic");
    }

    /// <summary>
    /// A <see cref="WebApplicationFactory{TProgram}"/> that wires the test-container
    /// databases and mocks the external HTTP adapters, but deliberately does NOT mock
    /// <c>IPublisher</c> so the real outbox-writing path is exercised.
    /// </summary>
    private sealed class UmMembershipWebFactory(
        UserManagementPostgreSqlFixture umPostgreSql,
        ITestOutputHelper testOutputHelper,
        Mock<IOrganisationApiAdapter> mockOrgAdapter,
        Mock<IPersonApiAdapter> mockPersonAdapter)
        : WebApplicationFactory<Program>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureFakePolicyEvaluator();
            builder.ConfigureLogging(testOutputHelper);

            builder.ConfigureServices((_, services) =>
            {
                services.PostConfigure<RedisCacheOptions>(options =>
                {
                    options.Configuration = $"{umPostgreSql.RedisHost}:{umPostgreSql.RedisPort}";
                    options.InstanceName = "UserManagement_";
                });

                // Redirect UM DbContext to the test container
                services.RemoveAll<UserManagementDbContext>();
                services.RemoveAll<DbContextOptions<UserManagementDbContext>>();
                services.AddDbContext<UserManagementDbContext>((sp, options) =>
                    options.UseNpgsql(
                            umPostgreSql.ConnectionString,
                            npgsqlOptions => npgsqlOptions
                                .MigrationsAssembly(typeof(UserManagementDbContext).Assembly.FullName)
                                .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                        .AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>()));

                // Register the real production outbox publisher backed by the test-container PostgreSQL.
                // This exercises the full outbox-writing path without the SQS transport layer.
                // Program.cs only registers IPublisher inside a guard that is false at test time,
                // so it must be wired explicitly here.
                services.RemoveAll<IPublisher>();
                services.RemoveAll<IOutboxMessageRepository>();
                services.RemoveAll<MultiQueueOutboxMessagePublisherConfiguration>();
                services
                    .AddScoped<IOutboxMessageRepository, DatabaseOutboxMessageRepository<UserManagementDbContext>>();
                services.AddSingleton(new MultiQueueOutboxMessagePublisherConfiguration
                {
                    Destinations =
                    [
                        new OutboxMessagePublisher.OutboxMessagePublisherConfiguration
                        {
                            QueueUrl = "http://test-queue",
                            MessageGroupId = "test"
                        }
                    ]
                });
                services.AddScoped<IPublisher, MultiQueueOutboxMessagePublisher>();

                services.RemoveAll<IOrganisationApiAdapter>();
                services.AddScoped(_ => mockOrgAdapter.Object);

                services.RemoveAll<IPersonApiAdapter>();
                services.AddScoped(_ => mockPersonAdapter.Object);
            });

            return base.CreateHost(builder);
        }
    }

    /// <summary>
    /// A <see cref="WebApplicationFactory{TProgram}"/> used for outbox atomicity tests.
    /// Replaces <see cref="IOutboxMessageRepository"/> with a mock that always throws, so that
    /// any use case calling <c>publisher.Publish()</c> will fail. The test then asserts that the
    /// associated entity change was also rolled back — proving entity writes and outbox writes are
    /// atomic.
    /// </summary>
    private sealed class UmMembershipAtomicityWebFactory(
        UserManagementPostgreSqlFixture umPostgreSql,
        Mock<IOrganisationApiAdapter> mockOrgAdapter)
        : WebApplicationFactory<Program>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureFakePolicyEvaluator();

            builder.ConfigureServices((_, services) =>
            {
                services.PostConfigure<RedisCacheOptions>(options =>
                {
                    options.Configuration = $"{umPostgreSql.RedisHost}:{umPostgreSql.RedisPort}";
                    options.InstanceName = "UserManagement_";
                });

                // Redirect UM DbContext to the test container
                services.RemoveAll<UserManagementDbContext>();
                services.RemoveAll<DbContextOptions<UserManagementDbContext>>();
                services.AddDbContext<UserManagementDbContext>((sp, options) =>
                    options.UseNpgsql(
                            umPostgreSql.ConnectionString,
                            npgsqlOptions => npgsqlOptions
                                .MigrationsAssembly(typeof(UserManagementDbContext).Assembly.FullName)
                                .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                        .AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>()));

                // Replace the outbox repository with one that always throws.
                // This simulates a failure in the outbox write to assert that entity changes are
                // also rolled back (i.e., both succeed or both fail together).
                var failingRepository = new Mock<IOutboxMessageRepository>();
                failingRepository
                    .Setup(r => r.SaveAsync(It.IsAny<OutboxMessage>()))
                    .ThrowsAsync(new Exception("Simulated outbox write failure"));

                services.RemoveAll<IPublisher>();
                services.RemoveAll<IOutboxMessageRepository>();
                services.RemoveAll<MultiQueueOutboxMessagePublisherConfiguration>();
                services.AddScoped(_ => failingRepository.Object);
                services.AddSingleton(new MultiQueueOutboxMessagePublisherConfiguration
                {
                    Destinations =
                    [
                        new OutboxMessagePublisher.OutboxMessagePublisherConfiguration
                        {
                            QueueUrl = "http://test-queue",
                            MessageGroupId = "test"
                        }
                    ]
                });
                services.AddScoped<IPublisher, MultiQueueOutboxMessagePublisher>();

                services.RemoveAll<IOrganisationApiAdapter>();
                services.AddScoped(_ => mockOrgAdapter.Object);

                var mockPersonAdapter = new Mock<IPersonApiAdapter>();
                services.RemoveAll<IPersonApiAdapter>();
                services.AddScoped(_ => mockPersonAdapter.Object);
            });

            return base.CreateHost(builder);
        }
    }
}