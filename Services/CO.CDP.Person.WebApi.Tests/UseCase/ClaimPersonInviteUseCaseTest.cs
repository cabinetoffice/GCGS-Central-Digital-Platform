using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Person.WebApi.Model;
using CO.CDP.Person.WebApi.UseCase;
using FluentAssertions;
using Moq;

namespace CO.CDP.Person.WebApi.Tests.UseCase;

public class ClaimPersonInviteUseCaseTests
{
    private readonly Mock<IPersonRepository> _mockPersonRepository;
    private readonly Mock<IPersonInviteRepository> _mockPersonInviteRepository;
    private readonly Mock<IOrganisationRepository> _mockOrganizationRepository;

    private readonly ClaimPersonInviteUseCase _useCase;
    private Guid _defaultPersonGuid;
    private Guid _defaultPersonInviteGuid;
    private OrganisationInformation.Persistence.Person _defaultPerson;

    public ClaimPersonInviteUseCaseTests()
    {
        _mockPersonRepository = new Mock<IPersonRepository>();
        _mockPersonInviteRepository = new Mock<IPersonInviteRepository>();
        _mockOrganizationRepository = new Mock<IOrganisationRepository>();
        _useCase = new ClaimPersonInviteUseCase(
                        _mockPersonRepository.Object,
                        _mockPersonInviteRepository.Object,
                        _mockOrganizationRepository.Object);
        _defaultPersonGuid = new Guid();
        _defaultPersonInviteGuid = new Guid();
        _defaultPerson = new OrganisationInformation.Persistence.Person
        {
            Id = 0,
            Guid = _defaultPersonGuid,
            FirstName = null!,
            LastName = null!,
            Email = null!
        };
    }

    [Fact]
    public async Task Execute_Throws_UnknownPersonException_When_Person_Not_Found()
    {
        var command = (personId: Guid.NewGuid(), claimPersonInvite: new ClaimPersonInvite { PersonInviteId = Guid.NewGuid() });

        _mockPersonRepository.Setup(repo => repo.Find(command.personId))
            .ReturnsAsync((OrganisationInformation.Persistence.Person)null!);

        Func<Task> act = async () => await _useCase.Execute(command);

        await act.Should()
            .ThrowAsync<UnknownPersonException>()
            .WithMessage($"Unknown person {command.personId}.");
    }

    [Fact]
    public async Task Execute_Throws_UnknownPersonInviteException_When_PersonInvite_Not_Found()
    {
        var command = (personId: _defaultPerson.Guid, claimPersonInvite: new ClaimPersonInvite { PersonInviteId = Guid.NewGuid() });

        _mockPersonRepository.Setup(repo => repo.Find(_defaultPerson.Guid))
            .ReturnsAsync(_defaultPerson);
        _mockPersonInviteRepository.Setup(repo => repo.Find(command.claimPersonInvite.PersonInviteId))
            .ReturnsAsync((PersonInvite)null!);

        Func<Task> act = async () => await _useCase.Execute(command);

        await act.Should()
            .ThrowAsync<UnknownPersonInviteException>()
            .WithMessage($"Unknown personInvite {command.claimPersonInvite.PersonInviteId}.");

    }

    [Fact]
    public async Task Execute_Throws_PersonInviteAlreadyClaimedException_When_PersonInvite_Already_Claimed()
    {
        var claimedPersonInvite = new PersonInvite
        {
            Id = 0,
            Person = _defaultPerson,
            Guid = _defaultPersonInviteGuid,
            FirstName = null!,
            LastName = null!,
            Email = null!,
            OrganisationId = 0,
            Organisation = null!,
            Scopes = null!
        };
        var command = (personId: _defaultPerson.Guid, claimPersonInvite: new ClaimPersonInvite { PersonInviteId = _defaultPersonInviteGuid });

        _mockPersonRepository.Setup(repo => repo.Find(_defaultPerson.Guid))
            .ReturnsAsync(_defaultPerson);
        _mockPersonInviteRepository.Setup(repo => repo.Find(command.claimPersonInvite.PersonInviteId))
            .ReturnsAsync(claimedPersonInvite);

        Func<Task> act = async () => await _useCase.Execute(command);

        await act.Should()
            .ThrowAsync<PersonInviteAlreadyClaimedException>()
            .WithMessage($"PersonInvite {command.claimPersonInvite.PersonInviteId} has already been claimed.");
    }

    [Fact]
    public async Task Execute_Throws_UnknownOrganisationException_When_PersonInvite_Already_Claimed()
    {
        var claimedPersonInvite = new PersonInvite
        {
            Id = 0,
            Person = null,
            Guid = _defaultPersonInviteGuid,
            FirstName = null!,
            LastName = null!,
            Email = null!,
            OrganisationId = 1,
            Organisation = null!,
            Scopes = new List<string> { "EDITOR", "ADMIN" }
        };
        var command = (personId: _defaultPerson.Guid, claimPersonInvite: new ClaimPersonInvite { PersonInviteId = _defaultPersonInviteGuid });

        _mockPersonRepository.Setup(repo => repo.Find(_defaultPerson.Guid))
            .ReturnsAsync(_defaultPerson);
        _mockPersonInviteRepository.Setup(repo => repo.Find(command.claimPersonInvite.PersonInviteId))
            .ReturnsAsync(claimedPersonInvite);
        _mockOrganizationRepository.Setup(repo => repo.FindIncludingTenantByOrgId(1))
            .ReturnsAsync((Organisation?)null);

        Func<Task> act = async () => await _useCase.Execute(command);

        await act.Should()
            .ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {claimedPersonInvite.OrganisationId} for PersonInvite {command.claimPersonInvite.PersonInviteId}.");
    }

    [Fact]
    public async Task Execute_Adds_Person_To_Organisation_And_Tenant_And_Claims_Invite()
    {
        var tenant = new Tenant
        {
            Id = 1,
            Guid = new Guid(),
            Name = "Test Organisation Tenant"
        };

        var personInvite = new PersonInvite
        {
            Id = 0,
            Person = null,
            Guid = _defaultPersonInviteGuid,
            FirstName = null!,
            LastName = null!,
            Email = null!,
            OrganisationId = 1,
            Organisation = new Organisation
            {
                OrganisationPersons = new List<OrganisationPerson>(),
                Id = 1,
                Name = "Test Organisation",
                Guid = new Guid(),
                Tenant = tenant
            },
            Scopes = new List<string> { "EDITOR", "ADMIN" }
        };
        var command = (personId: _defaultPerson.Guid, claimPersonInvite: new ClaimPersonInvite { PersonInviteId = personInvite.Guid });

        _mockPersonRepository.Setup(repo => repo.Find(_defaultPerson.Guid))
            .ReturnsAsync(_defaultPerson);
        _mockPersonInviteRepository.Setup(repo => repo.Find(command.claimPersonInvite.PersonInviteId))
            .ReturnsAsync(personInvite);
        _mockOrganizationRepository.Setup(repo => repo.FindIncludingTenantByOrgId(1))
            .ReturnsAsync(personInvite.Organisation);

        await _useCase.Execute(command);

        personInvite.Person.Should().Be(_defaultPerson);
        personInvite.Person?.Tenants.Should().Contain(tenant);
        personInvite.Organisation.OrganisationPersons.Should().Contain(op =>
            op.Person == _defaultPerson && op.OrganisationId == personInvite.OrganisationId);
        personInvite.Scopes.Should().Equal(personInvite.Organisation.OrganisationPersons.First().Scopes);

        _mockPersonRepository.Verify(repo => repo.Save(_defaultPerson), Times.Once);
        _mockPersonInviteRepository.Verify(repo => repo.Save(personInvite), Times.Once);
    }
}
