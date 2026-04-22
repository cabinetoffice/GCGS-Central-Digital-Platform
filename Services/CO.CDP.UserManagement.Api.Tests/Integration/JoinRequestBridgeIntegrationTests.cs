using System.Net;
using System.Net.Http.Json;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Tests;
using CO.CDP.TestKit.Mvc;
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

public class JoinRequestBridgeIntegrationTests : IClassFixture<UserManagementPostgreSqlFixture>,
    IClassFixture<OrganisationInformationPostgreSqlFixture>
{
    private readonly OrganisationInformationContext _cdpContext;
    private readonly HttpClient _httpClient;
    private readonly Mock<IOrganisationApiAdapter> _mockOrganisationApiAdapter;
    private readonly Mock<IPersonApiAdapter> _mockPersonApiAdapter;
    private readonly UserManagementDbContext _umContext;

    public JoinRequestBridgeIntegrationTests(
        ITestOutputHelper testOutputHelper,
        UserManagementPostgreSqlFixture umPostgreSql,
        OrganisationInformationPostgreSqlFixture cdpPostgreSql)
    {
        _mockOrganisationApiAdapter = new Mock<IOrganisationApiAdapter>();
        _mockPersonApiAdapter = new Mock<IPersonApiAdapter>();

        var reviewerPersonGuid = Guid.NewGuid();
        _mockPersonApiAdapter
            .Setup(a => a.GetPersonDetailsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PersonDetails
            {
                CdpPersonId = reviewerPersonGuid, FirstName = "Reviewer", LastName = "User",
                Email = "reviewer@example.com"
            });

        TestWebApplicationFactory<Program> factory = new(builder =>
        {
            builder.ConfigureFakePolicyEvaluator();
            builder.ConfigureLogging(testOutputHelper);

            builder.ConfigureServices((_, services) =>
            {
                services.RemoveAll<UserManagementDbContext>();
                services.RemoveAll<DbContextOptions<UserManagementDbContext>>();
                services.AddDbContext<UserManagementDbContext>((sp, options) =>
                    options.UseNpgsql(umPostgreSql.ConnectionString,
                            npgsqlOptions => npgsqlOptions
                                .MigrationsAssembly(typeof(UserManagementDbContext).Assembly.FullName)
                                .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                        .AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>()));

                services.RemoveAll<OrganisationInformationContext>();
                services.AddScoped(_ => cdpPostgreSql.OrganisationInformationContext());

                services.RemoveAll<IOrganisationApiAdapter>();
                services.AddScoped(_ => _mockOrganisationApiAdapter.Object);

                services.RemoveAll<IPersonApiAdapter>();
                services.AddScoped(_ => _mockPersonApiAdapter.Object);
            });
        });

        _httpClient = factory.CreateClient();
        _umContext = umPostgreSql.UserManagementContext();
        _cdpContext = cdpPostgreSql.OrganisationInformationContext();
    }

    [Fact]
    public async Task GetJoinRequests_ReturnsProxiedOiData()
    {
        ClearDatabase();
        var cdpOrg = CreateCdpOrganisation("Join Req Org");
        CreateUmOrganisation(cdpOrg.Guid, "Join Req Org");

        var joinRequestId = Guid.NewGuid();
        var personId = Guid.NewGuid();

        _mockOrganisationApiAdapter
            .Setup(a => a.GetOrganisationJoinRequestsAsync(cdpOrg.Guid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OiJoinRequest>
            {
                new()
                {
                    Id = joinRequestId, PersonId = personId, FirstName = "Alice", LastName = "Brown",
                    Email = "alice@example.com", Status = "Pending"
                }
            });

        var response = await _httpClient.GetAsync($"/api/organisations/{cdpOrg.Guid}/join-requests");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await response.Content.ReadFromJsonAsync<List<JoinRequestResponse>>();
        results.Should().NotBeNull().And.HaveCount(1);
        results![0].Id.Should().Be(joinRequestId);
        results[0].FirstName.Should().Be("Alice");
        results[0].Email.Should().Be("alice@example.com");
    }

    [Fact]
    public async Task ApproveJoinRequest_CreatesUmMembership()
    {
        ClearDatabase();
        var cdpOrg = CreateCdpOrganisation("Approve Org");
        CreateUmOrganisation(cdpOrg.Guid, "Approve Org");

        var joinRequestId = Guid.NewGuid();
        var personId = Guid.NewGuid();

        _mockOrganisationApiAdapter
            .Setup(a => a.UpdateJoinRequestAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(),
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new ReviewJoinRequestRequest
        {
            Decision = JoinRequestDecision.Accepted,
            RequestingPersonId = personId
        };

        var response = await _httpClient.PutAsJsonAsync(
            $"/api/organisations/{cdpOrg.Guid}/join-requests/{joinRequestId}",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var membership = await _umContext.UserOrganisationMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.CdpPersonId == personId);

        membership.Should().NotBeNull();
        membership!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ApproveJoinRequest_CallsOiAdapterWithAccepted()
    {
        ClearDatabase();
        var cdpOrg = CreateCdpOrganisation("Adapter Call Org");
        CreateUmOrganisation(cdpOrg.Guid, "Adapter Call Org");

        var joinRequestId = Guid.NewGuid();
        var personId = Guid.NewGuid();

        _mockOrganisationApiAdapter
            .Setup(a => a.UpdateJoinRequestAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(),
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new ReviewJoinRequestRequest
        {
            Decision = JoinRequestDecision.Accepted,
            RequestingPersonId = personId
        };

        await _httpClient.PutAsJsonAsync(
            $"/api/organisations/{cdpOrg.Guid}/join-requests/{joinRequestId}",
            request);

        _mockOrganisationApiAdapter.Verify(a =>
                a.UpdateJoinRequestAsync(
                    cdpOrg.Guid, joinRequestId, "Accepted", It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RejectJoinRequest_DoesNotCreateMembership()
    {
        ClearDatabase();
        var cdpOrg = CreateCdpOrganisation("Reject Org");
        var umOrg = CreateUmOrganisation(cdpOrg.Guid, "Reject Org");

        var joinRequestId = Guid.NewGuid();
        var personId = Guid.NewGuid();

        _mockOrganisationApiAdapter
            .Setup(a => a.UpdateJoinRequestAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(),
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new ReviewJoinRequestRequest
        {
            Decision = JoinRequestDecision.Rejected,
            RequestingPersonId = personId
        };

        var response = await _httpClient.PutAsJsonAsync(
            $"/api/organisations/{cdpOrg.Guid}/join-requests/{joinRequestId}",
            request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var membership = await _umContext.UserOrganisationMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.CdpPersonId == personId && m.OrganisationId == umOrg.Id);

        membership.Should().BeNull();
    }

    private void ClearDatabase()
    {
        _umContext.UserOrganisationMemberships.RemoveRange(_umContext.UserOrganisationMemberships);
        _umContext.InviteRoleMappings.RemoveRange(_umContext.InviteRoleMappings);
        _umContext.Organisations.RemoveRange(_umContext.Organisations);
        _umContext.SaveChanges();

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
                Name = $"Tenant {Guid.NewGuid()}"
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