using System.Net;
using System.Net.Http.Json;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Tests;
using CO.CDP.TestKit.Mvc;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Core.Models;
using CO.CDP.UserManagement.Infrastructure.Data;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit.Abstractions;
using CdpOrganisation = CO.CDP.OrganisationInformation.Persistence.Organisation;
using ContactPoint = CO.CDP.OrganisationInformation.Persistence.ContactPoint;
using Identifier = CO.CDP.OrganisationInformation.Persistence.Identifier;
using OrganisationAddress = CO.CDP.OrganisationInformation.Persistence.OrganisationAddress;
using OrganisationType = CO.CDP.OrganisationInformation.OrganisationType;
using PartyRole = CO.CDP.OrganisationInformation.PartyRole;
using UmOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;

namespace CO.CDP.UserManagement.Api.Tests.Integration;

public class InviteBridgeIntegrationTests : IClassFixture<UserManagementPostgreSqlFixture>, IClassFixture<OrganisationInformationPostgreSqlFixture>
{
    private readonly HttpClient _httpClient;
    private readonly UserManagementDbContext _umContext;
    private readonly OrganisationInformationContext _cdpContext;
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;

    public InviteBridgeIntegrationTests(
        ITestOutputHelper testOutputHelper,
        UserManagementPostgreSqlFixture umPostgreSql,
        OrganisationInformationPostgreSqlFixture cdpPostgreSql)
    {
        var umPostgreSql1 = umPostgreSql;
        var cdpPostgreSql1 = cdpPostgreSql;
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        var mockPersonLookupService = new Mock<IPersonLookupService>();

        TestWebApplicationFactory<Program> factory = new(builder =>
        {
            builder.ConfigureFakePolicyEvaluator();
            builder.ConfigureLogging(testOutputHelper);

            builder.ConfigureServices((_, services) =>
            {
                services.RemoveAll<UserManagementDbContext>();
                services.RemoveAll<DbContextOptions<UserManagementDbContext>>();
                services.AddDbContext<UserManagementDbContext>((sp, options) =>
                    options.UseNpgsql(umPostgreSql1.ConnectionString,
                            npgsqlOptions => npgsqlOptions
                                .MigrationsAssembly(typeof(UserManagementDbContext).Assembly.FullName)
                                .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                        .AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>()));

                services.RemoveAll<OrganisationInformationContext>();
                services.AddScoped(_ => cdpPostgreSql1.OrganisationInformationContext());

                services.RemoveAll<IOrganisationClient>();
                services.AddScoped(_ => _mockOrganisationClient.Object);

                services.RemoveAll<IPersonLookupService>();
                services.AddScoped(_ => mockPersonLookupService.Object);

            });
        });

        _httpClient = factory.CreateClient();
        _umContext = umPostgreSql1.UserManagementContext();
        _cdpContext = cdpPostgreSql1.OrganisationInformationContext();

        mockPersonLookupService
            .Setup(c => c.GetPersonDetailsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PersonDetails?)null);
    }

    [Fact]
    public async Task PostInvite_WithValidRequest_CreatesInviteRoleMapping()
    {
        ClearDatabases();
        var cdpOrg = CreateCdpOrganisation("Test Organisation");
        var umOrg = CreateUmOrganisation(cdpOrg.Guid, "Test Organisation");
        var cdpPersonInviteGuid = Guid.NewGuid();

        var request = new InviteUserRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            OrganisationRole = OrganisationRole.Admin,
            ApplicationAssignments = new List<ApplicationAssignment>()
        };

        _mockOrganisationClient
            .Setup(c => c.CreatePersonInviteForServiceAsync(
                cdpOrg.Guid,
                It.IsAny<InvitePersonToOrganisation>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PersonInviteModel(
                email: request.Email,
                expiresOn: DateTimeOffset.UtcNow.AddDays(7),
                firstName: request.FirstName,
                id: cdpPersonInviteGuid,
                lastName: request.LastName,
                scopes: new List<string> { "ADMIN" }
            ));

        var response = await _httpClient.PostAsJsonAsync(
            $"/api/organisations/{cdpOrg.Guid}/invites",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var mapping = await _umContext.InviteRoleMappings
            .FirstOrDefaultAsync(m => m.CdpPersonInviteGuid == cdpPersonInviteGuid);

        mapping.Should().NotBeNull();
        mapping!.OrganisationId.Should().Be(umOrg.Id);
        mapping.OrganisationRole.Should().Be(OrganisationRole.Admin);
        mapping.IsDeleted.Should().BeFalse();

        _mockOrganisationClient.Verify(
            c => c.CreatePersonInviteForServiceAsync(
                cdpOrg.Guid,
                It.Is<InvitePersonToOrganisation>(req =>
                    req.Email == request.Email &&
                    req.FirstName == request.FirstName &&
                    req.LastName == request.LastName),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetInvites_ReturnsCombinedDataFromInviteRoleMappingAndCdpPersonInvites()
    {
        ClearDatabases();
        var cdpOrg = CreateCdpOrganisation("Test Organisation");
        var umOrg = CreateUmOrganisation(cdpOrg.Guid, "Test Organisation");
        var cdpPersonInviteGuid = Guid.NewGuid();

        var cdpPersonInvite = new PersonInvite
        {
            Guid = cdpPersonInviteGuid,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            OrganisationId = cdpOrg.Id,
            Scopes = new List<string> { "VIEWER" },
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };
        _cdpContext.PersonInvites.Add(cdpPersonInvite);
        await _cdpContext.SaveChangesAsync();

        var inviteRoleMapping = new InviteRoleMapping
        {
            CdpPersonInviteGuid = cdpPersonInviteGuid,
            OrganisationId = umOrg.Id,
            OrganisationRole = OrganisationRole.Member,
            CreatedBy = "test-user",
            CreatedAt = DateTimeOffset.UtcNow,
            ModifiedAt = DateTimeOffset.UtcNow
        };
        _umContext.InviteRoleMappings.Add(inviteRoleMapping);
        await _umContext.SaveChangesAsync();

        var response = await _httpClient.GetAsync($"/api/organisations/{cdpOrg.Guid}/invites");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var invites = await response.Content.ReadFromJsonAsync<List<PendingOrganisationInviteResponse>>();
        invites.Should().NotBeNull();
        invites.Should().HaveCount(1);

        var invite = invites![0];
        invite.Email.Should().Be("jane.smith@example.com");
        invite.FirstName.Should().Be("Jane");
        invite.LastName.Should().Be("Smith");
        invite.OrganisationRole.Should().Be(OrganisationRole.Member);
        invite.CdpPersonInviteGuid.Should().Be(cdpPersonInviteGuid);
    }

    [Fact]
    public async Task DeleteInvite_RemovesInviteRoleMapping()
    {
        ClearDatabases();
        var cdpOrg = CreateCdpOrganisation("Test Organisation");
        var umOrg = CreateUmOrganisation(cdpOrg.Guid, "Test Organisation");

        var inviteRoleMapping = new InviteRoleMapping
        {
            CdpPersonInviteGuid = Guid.NewGuid(),
            OrganisationId = umOrg.Id,
            OrganisationRole = OrganisationRole.Member,
            CreatedBy = "test-user",
            CreatedAt = DateTimeOffset.UtcNow,
            ModifiedAt = DateTimeOffset.UtcNow
        };
        _umContext.InviteRoleMappings.Add(inviteRoleMapping);
        await _umContext.SaveChangesAsync();

        var response = await _httpClient.DeleteAsync(
            $"/api/organisations/{cdpOrg.Guid}/invites/{inviteRoleMapping.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var deletedMapping = await _umContext.InviteRoleMappings
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == inviteRoleMapping.Id);

        deletedMapping.Should().NotBeNull();
        deletedMapping!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task PutInviteRole_UpdatesOrganisationRole()
    {
        ClearDatabases();
        var cdpOrg = CreateCdpOrganisation("Test Organisation");
        var umOrg = CreateUmOrganisation(cdpOrg.Guid, "Test Organisation");

        var inviteRoleMapping = new InviteRoleMapping
        {
            CdpPersonInviteGuid = Guid.NewGuid(),
            OrganisationId = umOrg.Id,
            OrganisationRole = OrganisationRole.Member,
            CreatedBy = "test-user",
            CreatedAt = DateTimeOffset.UtcNow,
            ModifiedAt = DateTimeOffset.UtcNow
        };
        _umContext.InviteRoleMappings.Add(inviteRoleMapping);
        await _umContext.SaveChangesAsync();

        var request = new ChangeOrganisationRoleRequest
        {
            OrganisationRole = OrganisationRole.Admin
        };

        var response = await _httpClient.PutAsJsonAsync(
            $"/api/organisations/{cdpOrg.Guid}/invites/{inviteRoleMapping.Id}/role",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var updatedMapping = await _umContext.InviteRoleMappings
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == inviteRoleMapping.Id);

        updatedMapping.Should().NotBeNull();
        updatedMapping!.OrganisationRole.Should().Be(OrganisationRole.Admin);
    }

    private void ClearDatabases()
    {
        _umContext.InviteRoleMappings.RemoveRange(_umContext.InviteRoleMappings);
        _umContext.Organisations.RemoveRange(_umContext.Organisations);
        _umContext.SaveChanges();

        _cdpContext.PersonInvites.RemoveRange(_cdpContext.PersonInvites);
        _cdpContext.Organisations.RemoveRange(_cdpContext.Organisations);
        _cdpContext.SaveChanges();
    }

    private CdpOrganisation CreateCdpOrganisation(string name)
    {
        var org = new CdpOrganisation
        {
            Guid = Guid.NewGuid(),
            Name = name,
            Tenant = new Tenant
            {
                Guid = Guid.NewGuid(),
                Name = $"Test Tenant {Guid.NewGuid()}"
            },
            Type = OrganisationType.Organisation,
            ContactPoints = new List<ContactPoint>(),
            Identifiers = new List<Identifier>(),
            Addresses = new List<OrganisationAddress>(),
            Roles = new List<PartyRole> { PartyRole.Tenderer },
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow
        };

        _cdpContext.Organisations.Add(org);
        _cdpContext.SaveChanges();
        return org;
    }

    private UmOrganisation CreateUmOrganisation(Guid cdpGuid, string name)
    {
        var org = new UmOrganisation
        {
            CdpOrganisationGuid = cdpGuid,
            Name = name,
            Slug = name.ToLower().Replace(" ", "-"),
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
}
