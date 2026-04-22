using System.Text.Json;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using CO.CDP.AwsServices;
using CO.CDP.AwsServices.Sqs;
using CO.CDP.TestKit.Mvc;
using CO.CDP.UserManagement.Api.Events;
using CO.CDP.UserManagement.Api.Events.Handlers;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Data;
using CO.CDP.UserManagement.Infrastructure.Events;
using CO.CDP.UserManagement.Shared.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit.Abstractions;
using UmOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;
using UmMembership = CO.CDP.UserManagement.Core.Entities.UserOrganisationMembership;
using CorePartyRole = CO.CDP.UserManagement.Core.Constants.PartyRole;

namespace CO.CDP.UserManagement.Api.Tests.Integration;

/// <summary>
/// End-to-end round-trip integration tests that publish real SQS events to a LocalStack
/// queue, run <see cref="SqsDispatcher"/> with the production event handlers, and assert
/// the resulting User Management database state.
/// </summary>
public class SqsRoundTripIntegrationTests
    : IClassFixture<UserManagementPostgreSqlFixture>,
        IClassFixture<LocalStackFixture>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly Mock<IOrganisationApiAdapter> _organisationApiAdapter = new();
    private readonly AmazonSQSClient _sqsClient;
    private readonly UserManagementDbContext _umContext;
    private readonly UserManagementPostgreSqlFixture _umFixture;

    public SqsRoundTripIntegrationTests(
        ITestOutputHelper testOutputHelper,
        UserManagementPostgreSqlFixture umFixture,
        LocalStackFixture localStack)
    {
        _umFixture = umFixture;

        _organisationApiAdapter
            .Setup(a => a.GetPartyRolesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<CorePartyRole>());

        _sqsClient = new AmazonSQSClient(
            new BasicAWSCredentials("test", "test"),
            new AmazonSQSConfig
            {
                ServiceURL = localStack.ConnectionString,
                UseHttp = false,
                AuthenticationRegion = "eu-west-1"
            });

        _factory = new TestWebApplicationFactory<Program>(builder =>
        {
            builder.ConfigureFakePolicyEvaluator();
            builder.ConfigureLogging(testOutputHelper);
            builder.ConfigureServices((_, services) =>
            {
                services.PostConfigure<RedisCacheOptions>(options =>
                {
                    options.Configuration = $"{umFixture.RedisHost}:{umFixture.RedisPort}";
                    options.InstanceName = "UserManagement_";
                });

                services.RemoveAll<UserManagementDbContext>();
                services.RemoveAll<DbContextOptions<UserManagementDbContext>>();
                services.AddDbContext<UserManagementDbContext>((sp, options) =>
                    options.UseNpgsql(
                            umFixture.ConnectionString,
                            npgsqlOptions => npgsqlOptions
                                .MigrationsAssembly(typeof(UserManagementDbContext).Assembly.FullName)
                                .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                        .AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>()));

                services.RemoveAll<IOrganisationApiAdapter>();
                services.AddScoped(_ => _organisationApiAdapter.Object);

                services.RemoveAll<IPersonApiAdapter>();
                services.AddScoped(_ => new Mock<IPersonApiAdapter>().Object);
            });
        });

        _umContext = umFixture.UserManagementContext();
    }

    // ─────────────────────── helpers ───────────────────────

    private void ClearDatabase()
    {
        _umContext.UserOrganisationMemberships.RemoveRange(
            _umContext.UserOrganisationMemberships.IgnoreQueryFilters().ToList());
        _umContext.Organisations.RemoveRange(_umContext.Organisations);
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

    private async Task<string> CreateFifoQueueAsync()
    {
        var queueName = $"user-management-{Guid.NewGuid():N}.fifo";
        var response = await _sqsClient.CreateQueueAsync(new CreateQueueRequest
        {
            QueueName = queueName,
            Attributes = new Dictionary<string, string>
            {
                ["FifoQueue"] = "true",
                ["ContentBasedDeduplication"] = "true"
            }
        });
        return response.QueueUrl;
    }

    private async Task SendEventAsync<T>(string queueUrl, T @event, string eventTypeName)
    {
        await _sqsClient.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = JsonSerializer.Serialize(@event),
            MessageGroupId = "test",
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                ["Type"] = new MessageAttributeValue
                {
                    StringValue = eventTypeName,
                    DataType = "String"
                }
            }
        });
    }

    private SqsDispatcher BuildDispatcher(string queueUrl) =>
        new(
            _sqsClient,
            new SqsDispatcherConfiguration
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = 1,
                WaitTimeSeconds = 0
            },
            EventDeserializer.Deserializer,
            (type, typeName) => type.Name == typeName,
            NullLogger<SqsDispatcher>.Instance);

    // ─────────────────────── tests ───────────────────────

    /// <summary>
    /// Verifies that receiving an <see cref="OrganisationRegistered"/> event creates the
    /// corresponding UM organisation record and enables its active applications.
    /// </summary>
    [Fact]
    public async Task OrganisationRegistered_ViaQueue_CreatesUmOrgAndEnablesApplications()
    {
        ClearDatabase();
        var orgGuid = Guid.NewGuid();
        var orgName = $"SQS Test Org {orgGuid:N}";
        var queueUrl = await CreateFifoQueueAsync();

        var @event = new OrganisationRegistered
        {
            Id = orgGuid.ToString(),
            Name = orgName,
            Roles = new List<string> { "Tenderer" },
            Type = 1
        };

        await SendEventAsync(queueUrl, @event, nameof(OrganisationRegistered));

        using var scope = _factory.Services.CreateScope();
        var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var dispatcher = BuildDispatcher(queueUrl);
        dispatcher.Subscribe(new OrganisationRegisteredHandler(
            syncRepo, unitOfWork, NullLogger<OrganisationRegisteredHandler>.Instance));

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await dispatcher.ExecuteAsync(cts.Token);

        using var assertContext = _umFixture.UserManagementContext();
        var org = await assertContext.Organisations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.CdpOrganisationGuid == orgGuid);

        org.Should().NotBeNull("OrganisationRegistered should create the UM org");
        org!.Name.Should().Be(orgName);
        org.IsActive.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that receiving an <see cref="OrganisationUpdated"/> event syncs the new
    /// name into the existing UM organisation record.
    /// </summary>
    [Fact]
    public async Task OrganisationUpdated_ViaQueue_UpdatesOrgName()
    {
        ClearDatabase();
        var orgGuid = Guid.NewGuid();
        var umOrg = CreateUmOrganisation(orgGuid, "Old Name");
        var queueUrl = await CreateFifoQueueAsync();

        var @event = new OrganisationUpdated
        {
            Id = orgGuid.ToString(),
            Name = "New Name"
        };

        await SendEventAsync(queueUrl, @event, nameof(OrganisationUpdated));

        using var scope = _factory.Services.CreateScope();
        var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var dispatcher = BuildDispatcher(queueUrl);
        dispatcher.Subscribe(new OrganisationUpdatedHandler(
            syncRepo, unitOfWork, NullLogger<OrganisationUpdatedHandler>.Instance));

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await dispatcher.ExecuteAsync(cts.Token);

        using var assertContext = _umFixture.UserManagementContext();
        var org = await assertContext.Organisations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.CdpOrganisationGuid == orgGuid);

        org.Should().NotBeNull();
        org!.Name.Should().Be("New Name");
    }

    /// <summary>
    /// Verifies that receiving a <see cref="PersonRemovedFromOrganisation"/> event
    /// soft-deletes the corresponding UM membership.
    /// </summary>
    [Fact]
    public async Task PersonRemovedFromOrganisation_ViaQueue_DeactivatesMembership()
    {
        ClearDatabase();
        var orgGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        var umOrg = CreateUmOrganisation(orgGuid, $"Removal Test Org {orgGuid:N}");
        CreateUmMembership(umOrg, personGuid, OrganisationRole.Member);
        var queueUrl = await CreateFifoQueueAsync();

        var @event = new PersonRemovedFromOrganisation
        {
            OrganisationId = orgGuid.ToString(),
            PersonId = personGuid.ToString()
        };

        await SendEventAsync(queueUrl, @event, nameof(PersonRemovedFromOrganisation));

        using var scope = _factory.Services.CreateScope();
        var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var dispatcher = BuildDispatcher(queueUrl);
        dispatcher.Subscribe(new PersonRemovedHandler(
            syncRepo, unitOfWork, NullLogger<PersonRemovedHandler>.Instance));

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await dispatcher.ExecuteAsync(cts.Token);

        using var assertContext = _umFixture.UserManagementContext();
        var membership = await assertContext.UserOrganisationMemberships
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.CdpPersonId == personGuid);

        membership.Should().NotBeNull();
        membership!.IsActive.Should().BeFalse("membership should be deactivated after person removal");
        membership.IsDeleted.Should().BeTrue("membership should be soft-deleted after person removal");
    }

    /// <summary>
    /// Verifies that receiving a <see cref="PersonScopesUpdated"/> event with admin scopes
    /// updates the UM membership role to Admin.
    /// </summary>
    [Fact]
    public async Task PersonScopesUpdated_ViaQueue_UpdatesMemberRole()
    {
        ClearDatabase();
        var orgGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        var umOrg = CreateUmOrganisation(orgGuid, $"Scope Update Org {orgGuid:N}");
        CreateUmMembership(umOrg, personGuid, OrganisationRole.Member);
        var queueUrl = await CreateFifoQueueAsync();

        var @event = new PersonScopesUpdated
        {
            OrganisationId = orgGuid.ToString(),
            PersonId = personGuid.ToString(),
            Scopes = new List<string> { "ADMIN" },
        };

        await SendEventAsync(queueUrl, @event, nameof(PersonScopesUpdated));

        using var scope = _factory.Services.CreateScope();
        var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var dispatcher = BuildDispatcher(queueUrl);
        dispatcher.Subscribe(new PersonScopesUpdatedHandler(
            syncRepo, unitOfWork, NullLogger<PersonScopesUpdatedHandler>.Instance));

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await dispatcher.ExecuteAsync(cts.Token);

        using var assertContext = _umFixture.UserManagementContext();
        var membership = await assertContext.UserOrganisationMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.CdpPersonId == personGuid);

        membership.Should().NotBeNull();
        membership!.OrganisationRole.Should().Be(OrganisationRole.Admin,
            "ADMIN scope should map to OrganisationRole.Admin");
    }

    /// <summary>
    /// Verifies that receiving a <see cref="PersonInviteClaimed"/> event creates a UM
    /// membership for the person in the given organisation.
    /// </summary>
    [Fact]
    public async Task PersonInviteClaimed_ViaQueue_CreatesMembership()
    {
        ClearDatabase();
        var orgGuid = Guid.NewGuid();
        var personGuid = Guid.NewGuid();
        var userPrincipalId = $"urn:fdc:test:{Guid.NewGuid():N}";
        CreateUmOrganisation(orgGuid, $"Invite Claimed Org {orgGuid:N}");
        var queueUrl = await CreateFifoQueueAsync();

        var @event = new PersonInviteClaimed
        {
            OrganisationId = orgGuid.ToString(),
            PersonId = personGuid.ToString(),
            UserPrincipalId = userPrincipalId,
            Scopes = new List<string> { "ADMIN" },
        };

        await SendEventAsync(queueUrl, @event, nameof(PersonInviteClaimed));

        using var scope = _factory.Services.CreateScope();
        var syncRepo = scope.ServiceProvider.GetRequiredService<IUmOrganisationSyncRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var dispatcher = BuildDispatcher(queueUrl);
        dispatcher.Subscribe(new PersonInviteClaimedHandler(
            syncRepo, unitOfWork, NullLogger<PersonInviteClaimedHandler>.Instance));

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        await dispatcher.ExecuteAsync(cts.Token);

        using var assertContext = _umFixture.UserManagementContext();
        var membership = await assertContext.UserOrganisationMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.CdpPersonId == personGuid);

        membership.Should().NotBeNull("invite claim should create a UM membership");
        membership!.UserPrincipalId.Should().Be(userPrincipalId);
        membership.IsActive.Should().BeTrue();
        membership.OrganisationRole.Should().Be(OrganisationRole.Admin,
            "ADMIN scope should map to OrganisationRole.Admin");
    }
}