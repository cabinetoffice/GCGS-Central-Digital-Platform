using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Constants;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Infrastructure.Services;
using CO.CDP.UserManagement.Shared.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using CdpOrganisation = CO.CDP.OrganisationInformation.Persistence.Organisation;
using CdpPerson = CO.CDP.OrganisationInformation.Persistence.Person;
using IOrganisationRepository = CO.CDP.UserManagement.Core.Interfaces.IOrganisationRepository;
using UmOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;

namespace CO.CDP.UserManagement.Api.Tests.Services;

public class CdpMembershipSyncServiceTests
{
    [Fact]
    public async Task SyncMembershipCreatedAsync_WhenEnabled_CreatesOrganisationPerson()
    {
        var context = CreateContext();
        var (org, person) = SeedOrganisationAndPerson(context);
        var configuration = BuildConfiguration(enabled: true);
        var organisationRepository = BuildOrganisationRepository(org.Guid);
        var logger = new Mock<ILogger<CdpMembershipSyncService>>();
        var service = new CdpMembershipSyncService(context, organisationRepository.Object, configuration, logger.Object);

        var membership = new UserOrganisationMembership
        {
            Id = 1,
            OrganisationId = 10,
            Organisation = new UmOrganisation { Id = 10, CdpOrganisationGuid = org.Guid, Name = "Org", Slug = "org" },
            OrganisationRole = OrganisationRole.Admin,
            CdpPersonId = person.Guid
        };

        await service.SyncMembershipCreatedAsync(membership);

        var organisationPerson = await context.Set<OrganisationPerson>()
            .FirstOrDefaultAsync(op => op.OrganisationId == org.Id && op.PersonId == person.Id);

        organisationPerson.Should().NotBeNull();
        organisationPerson!.Scopes.Should().ContainSingle(OrganisationPersonScopes.Admin);
    }

    [Fact]
    public async Task SyncMembershipRoleChangedAsync_WhenEnabled_UpdatesScopes()
    {
        var context = CreateContext();
        var (org, person) = SeedOrganisationAndPerson(context);
        var configuration = BuildConfiguration(enabled: true);
        var organisationRepository = BuildOrganisationRepository(org.Guid);
        var logger = new Mock<ILogger<CdpMembershipSyncService>>();
        var service = new CdpMembershipSyncService(context, organisationRepository.Object, configuration, logger.Object);

        var organisationPerson = new OrganisationPerson
        {
            OrganisationId = org.Id,
            Organisation = org,
            PersonId = person.Id,
            Person = person,
            Scopes = new List<string> { OrganisationPersonScopes.Viewer }
        };
        context.Add(organisationPerson);
        await context.SaveChangesAsync();

        var membership = new UserOrganisationMembership
        {
            Id = 2,
            OrganisationId = 10,
            Organisation = new UmOrganisation { Id = 10, CdpOrganisationGuid = org.Guid, Name = "Org", Slug = "org" },
            OrganisationRole = OrganisationRole.Admin,
            CdpPersonId = person.Guid
        };

        await service.SyncMembershipRoleChangedAsync(membership);

        var updated = await context.Set<OrganisationPerson>()
            .FirstOrDefaultAsync(op => op.OrganisationId == org.Id && op.PersonId == person.Id);

        updated!.Scopes.Should().ContainSingle(OrganisationPersonScopes.Admin);
    }

    [Fact]
    public async Task SyncMembershipRemovedAsync_WhenEnabled_RemovesOrganisationPerson()
    {
        var context = CreateContext();
        var (org, person) = SeedOrganisationAndPerson(context);
        var configuration = BuildConfiguration(enabled: true);
        var organisationRepository = BuildOrganisationRepository(org.Guid);
        var logger = new Mock<ILogger<CdpMembershipSyncService>>();
        var service = new CdpMembershipSyncService(context, organisationRepository.Object, configuration, logger.Object);

        var organisationPerson = new OrganisationPerson
        {
            OrganisationId = org.Id,
            Organisation = org,
            PersonId = person.Id,
            Person = person,
            Scopes = new List<string> { OrganisationPersonScopes.Viewer }
        };
        context.Add(organisationPerson);
        await context.SaveChangesAsync();

        var membership = new UserOrganisationMembership
        {
            Id = 3,
            OrganisationId = 10,
            Organisation = new UmOrganisation { Id = 10, CdpOrganisationGuid = org.Guid, Name = "Org", Slug = "org" },
            OrganisationRole = OrganisationRole.Member,
            CdpPersonId = person.Guid
        };

        await service.SyncMembershipRemovedAsync(membership);

        var remaining = await context.Set<OrganisationPerson>()
            .FirstOrDefaultAsync(op => op.OrganisationId == org.Id && op.PersonId == person.Id);

        remaining.Should().BeNull();
    }

    [Fact]
    public async Task SyncMembershipCreatedAsync_WhenDisabled_DoesNothing()
    {
        var context = CreateContext();
        var (org, person) = SeedOrganisationAndPerson(context);
        var configuration = BuildConfiguration(enabled: false);
        var organisationRepository = BuildOrganisationRepository(org.Guid);
        var logger = new Mock<ILogger<CdpMembershipSyncService>>();
        var service = new CdpMembershipSyncService(context, organisationRepository.Object, configuration, logger.Object);

        var membership = new UserOrganisationMembership
        {
            Id = 4,
            OrganisationId = 10,
            Organisation = new UmOrganisation { Id = 10, CdpOrganisationGuid = org.Guid, Name = "Org", Slug = "org" },
            OrganisationRole = OrganisationRole.Admin,
            CdpPersonId = person.Guid
        };

        await service.SyncMembershipCreatedAsync(membership);

        var organisationPerson = await context.Set<OrganisationPerson>()
            .FirstOrDefaultAsync(op => op.OrganisationId == org.Id && op.PersonId == person.Id);

        organisationPerson.Should().BeNull();
    }

    private static OrganisationInformationContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OrganisationInformationContext>()
            .UseInMemoryDatabase($"org-info-{Guid.NewGuid()}")
            .Options;
        return new OrganisationInformationContext(options);
    }

    private static (CdpOrganisation Organisation, CdpPerson Person) SeedOrganisationAndPerson(OrganisationInformationContext context)
    {
        var tenant = new Tenant
        {
            Guid = Guid.NewGuid(),
            Name = "Tenant"
        };
        var organisation = new CdpOrganisation
        {
            Guid = Guid.NewGuid(),
            Name = "Org",
            Tenant = tenant,
            Type = OrganisationType.Organisation,
            Roles = new List<PartyRole>()
        };
        var person = new CdpPerson
        {
            Guid = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            UserUrn = "user-urn"
        };

        context.Tenants.Add(tenant);
        context.Organisations.Add(organisation);
        context.Persons.Add(person);
        context.SaveChanges();

        return (organisation, person);
    }

    private static IConfiguration BuildConfiguration(bool enabled)
    {
        var values = new Dictionary<string, string?>
        {
            ["Features:CdpMembershipSyncEnabled"] = enabled.ToString()
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    private static Mock<IOrganisationRepository> BuildOrganisationRepository(Guid cdpOrganisationGuid)
    {
        var organisationRepository = new Mock<IOrganisationRepository>();
        organisationRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UmOrganisation
            {
                Id = 10,
                CdpOrganisationGuid = cdpOrganisationGuid,
                Name = "Org",
                Slug = "org"
            });
        return organisationRepository;
    }
}
