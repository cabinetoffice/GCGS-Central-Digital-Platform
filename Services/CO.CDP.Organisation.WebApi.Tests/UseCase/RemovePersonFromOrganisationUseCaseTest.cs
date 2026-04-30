using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationSync;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.FeatureManagement;
using Moq;
using OiPerson = CO.CDP.OrganisationInformation.Persistence.Person;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class RemovePersonFromOrganisationUseCaseTest
{
    private readonly Mock<IOrganisationRepository> _organisationRepository = new();
    private readonly Mock<IAtomicScope> _atomicScopeMock = new();
    private readonly Mock<IOrganisationMembershipSync> _membershipSyncMock = new();
    private readonly Mock<IFeatureManager> _featureManagerMock = new();

    private RemovePersonFromOrganisationUseCase UseCase => new(
        _organisationRepository.Object,
        _atomicScopeMock.Object,
        _membershipSyncMock.Object,
        _featureManagerMock.Object,
        NullLogger<RemovePersonFromOrganisationUseCase>.Instance);

    public RemovePersonFromOrganisationUseCaseTest()
    {
        _atomicScopeMock
            .Setup(s => s.ExecuteAsync(It.IsAny<Func<CancellationToken, Task<bool>>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task<bool>>, CancellationToken>((action, ct) => action(ct));
        _featureManagerMock.Setup(f => f.IsEnabledAsync(It.IsAny<string>())).ReturnsAsync(false);
    }

    [Fact]
    public async Task Execute_OrganisationPersonAndPersonWithTenant_BothLinksAreRemoved()
    {
        var person = GivenPerson();
        var organisation = GivenOrganisation(organisationPerson: person, tenantPerson: person);

        var command = (organisation.Guid, new RemovePersonFromOrganisation { PersonId = person.Guid });
        _organisationRepository.Setup(r => r.FindIncludingPersons(command.Item1)).ReturnsAsync(organisation);

        var result = await UseCase.Execute(command);

        result.Should().BeTrue();
        _organisationRepository.Verify(
            r => r.Track(It.Is<OrganisationInformation.Persistence.Organisation>(o =>
                o.OrganisationPersons.Count == 0 && o.Tenant.Persons.Count == 0)), Times.Once);
    }

    [Fact]
    public async Task Execute_NoOrganisationPerson_TenantLinkIsRemoved()
    {
        var person = GivenPerson();
        var organisation = GivenOrganisation(tenantPerson: person);

        var command = (organisation.Guid, new RemovePersonFromOrganisation { PersonId = person.Guid });
        _organisationRepository.Setup(r => r.FindIncludingPersons(command.Item1)).ReturnsAsync(organisation);

        var result = await UseCase.Execute(command);

        result.Should().BeTrue();
        _organisationRepository.Verify(
            r => r.Track(It.Is<OrganisationInformation.Persistence.Organisation>(o =>
                o.Tenant.Persons.Count == 0)), Times.Once);
    }

    [Fact]
    public async Task Execute_NoPersonWithTenant_OrganisationPersonIsRemoved()
    {
        var person = GivenPerson();
        var organisation = GivenOrganisation(organisationPerson: person);

        var command = (organisation.Guid, new RemovePersonFromOrganisation { PersonId = person.Guid });
        _organisationRepository.Setup(r => r.FindIncludingPersons(command.Item1)).ReturnsAsync(organisation);

        var result = await UseCase.Execute(command);

        result.Should().BeTrue();
        _organisationRepository.Verify(
            r => r.Track(It.Is<OrganisationInformation.Persistence.Organisation>(o =>
                o.OrganisationPersons.Count == 0)), Times.Once);
    }

    [Fact]
    public async Task Execute_NoOrganisationPersonNoPersonWithTenant_NoLinksAreRemoved()
    {
        var person = GivenPerson();
        var organisation = GivenOrganisation();

        var command = (organisation.Guid, new RemovePersonFromOrganisation { PersonId = person.Guid });
        _organisationRepository.Setup(r => r.FindIncludingPersons(command.Item1)).ReturnsAsync(organisation);

        var result = await UseCase.Execute(command);

        result.Should().BeFalse();
        _organisationRepository.Verify(r => r.Track(organisation), Times.Never);
    }

    [Fact]
    public async Task Execute_ShouldSyncToUm_WhenFeatureFlagEnabled()
    {
        var person = GivenPerson();
        var organisation = GivenOrganisation(organisationPerson: person);
        _featureManagerMock.Setup(f => f.IsEnabledAsync(It.IsAny<string>())).ReturnsAsync(true);
        _membershipSyncMock
            .Setup(s => s.RemoveMembershipAsync(It.IsAny<RemoveMembershipCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<SyncError, Unit>.Success(Unit.Value));

        var command = (organisation.Guid, new RemovePersonFromOrganisation { PersonId = person.Guid });
        _organisationRepository.Setup(r => r.FindIncludingPersons(command.Item1)).ReturnsAsync(organisation);

        var result = await UseCase.Execute(command);

        result.Should().BeTrue();
        _membershipSyncMock.Verify(s => s.RemoveMembershipAsync(
            It.Is<RemoveMembershipCommand>(c => c.OrganisationGuid == organisation.Guid && c.PersonGuid == person.Guid),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldSucceed_WhenSyncFails()
    {
        var person = GivenPerson();
        var organisation = GivenOrganisation(organisationPerson: person);
        _featureManagerMock.Setup(f => f.IsEnabledAsync(It.IsAny<string>())).ReturnsAsync(true);
        _membershipSyncMock
            .Setup(s => s.RemoveMembershipAsync(It.IsAny<RemoveMembershipCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<SyncError, Unit>.Failure(new SyncFailureError("UM unavailable")));

        var command = (organisation.Guid, new RemovePersonFromOrganisation { PersonId = person.Guid });
        _organisationRepository.Setup(r => r.FindIncludingPersons(command.Item1)).ReturnsAsync(organisation);

        var result = await UseCase.Execute(command);

        result.Should().BeTrue();
    }

    private static OiPerson GivenPerson() => new()
    {
        Id = 1,
        Guid = Guid.NewGuid(),
        FirstName = "Tom",
        LastName = "Smith",
        Email = "js@biz.com",
        UserUrn = "urn:1234"
    };

    private static OrganisationInformation.Persistence.Organisation GivenOrganisation(
        OiPerson? organisationPerson = null,
        OiPerson? tenantPerson = null)
    {
        var organisation = new OrganisationInformation.Persistence.Organisation
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            Name = "Acme",
            Type = OrganisationInformation.OrganisationType.Organisation,
            OrganisationPersons = [],
            Tenant = new Tenant { Guid = Guid.NewGuid(), Name = "A Tenant" }
        };
        if (organisationPerson != null)
            organisation.OrganisationPersons.Add(new OrganisationPerson
                { PersonId = organisationPerson.Id, Person = organisationPerson, Organisation = organisation });
        if (tenantPerson != null)
            organisation.Tenant.Persons.Add(tenantPerson);
        return organisation;
    }
}