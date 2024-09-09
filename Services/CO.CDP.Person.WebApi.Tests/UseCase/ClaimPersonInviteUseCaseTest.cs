using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Person.WebApi.Model;
using CO.CDP.Person.WebApi.UseCase;
using Moq;

namespace CO.CDP.Person.WebApi.Tests.UseCase;

public class ClaimPersonInviteUseCaseTests
{
    private readonly Mock<IPersonRepository> _mockPersonRepository;
    private readonly Mock<IPersonInviteRepository> _mockPersonInviteRepository;
    private readonly ClaimPersonInviteUseCase _useCase;
    private Guid _defaultPersonGuid;
    private Guid _defaultPersonInviteGuid;
    private OrganisationInformation.Persistence.Person _defaultPerson;

    public ClaimPersonInviteUseCaseTests()
    {
        _mockPersonRepository = new Mock<IPersonRepository>();
        _mockPersonInviteRepository = new Mock<IPersonInviteRepository>();
        _useCase = new ClaimPersonInviteUseCase(_mockPersonRepository.Object, _mockPersonInviteRepository.Object);
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

        var exception = await Assert.ThrowsAsync<UnknownPersonException>(() => _useCase.Execute(command));
        Assert.Equal($"Unknown person {command.personId}.", exception.Message);
    }

    [Fact]
    public async Task Execute_Throws_UnknownPersonInviteException_When_PersonInvite_Not_Found()
    {
        var command = (personId: _defaultPerson.Guid, claimPersonInvite: new ClaimPersonInvite { PersonInviteId = Guid.NewGuid() });

        _mockPersonRepository.Setup(repo => repo.Find(_defaultPerson.Guid))
            .ReturnsAsync(_defaultPerson);
        _mockPersonInviteRepository.Setup(repo => repo.Find(command.claimPersonInvite.PersonInviteId))
            .ReturnsAsync((PersonInvite)null!);

        var exception = await Assert.ThrowsAsync<UnknownPersonInviteException>(() => _useCase.Execute(command));
        Assert.Equal($"Unknown personInvite {command.claimPersonInvite.PersonInviteId}.", exception.Message);
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
            Organisation = null!,
            Scopes = null!
        };
        var command = (personId: _defaultPerson.Guid, claimPersonInvite: new ClaimPersonInvite { PersonInviteId = _defaultPersonInviteGuid });

        _mockPersonRepository.Setup(repo => repo.Find(_defaultPerson.Guid))
            .ReturnsAsync(_defaultPerson);
        _mockPersonInviteRepository.Setup(repo => repo.Find(command.claimPersonInvite.PersonInviteId))
            .ReturnsAsync(claimedPersonInvite);

        var exception = await Assert.ThrowsAsync<PersonInviteAlreadyClaimedException>(() => _useCase.Execute(command));
        Assert.Equal($"PersonInvite {command.claimPersonInvite.PersonInviteId} has already been claimed.", exception.Message);
    }

    [Fact]
    public async Task Execute_Adds_Person_To_Organisation_And_Claims_Invite()
    {
        var personInvite = new PersonInvite
        {
            Id = 0,
            Person = null,
            Guid = _defaultPersonInviteGuid,
            FirstName = null!,
            LastName = null!,
            Email = null!,
            Organisation = new Organisation
            {
                OrganisationPersons = new List<OrganisationPerson>(),
                Id = 0,
                Name = "Test Organisation",
                Guid = new Guid(),
                Tenant = null!
            },
            Scopes = new List<string> { "EDITOR", "ADMIN" }
        };
        var command = (personId: _defaultPerson.Guid, claimPersonInvite: new ClaimPersonInvite { PersonInviteId = personInvite.Guid });

        _mockPersonRepository.Setup(repo => repo.Find(_defaultPerson.Guid))
            .ReturnsAsync(_defaultPerson);
        _mockPersonInviteRepository.Setup(repo => repo.Find(command.claimPersonInvite.PersonInviteId))
            .ReturnsAsync(personInvite);

        await _useCase.Execute(command);

        Assert.Equal(_defaultPerson, personInvite.Person);
        Assert.Contains(personInvite.Organisation.OrganisationPersons, op => op.Person == _defaultPerson && op.Organisation == personInvite.Organisation);
        Assert.Equal(personInvite.Scopes, personInvite.Organisation.OrganisationPersons.First().Scopes);

        _mockPersonRepository.Verify(repo => repo.Save(_defaultPerson), Times.Once);
        _mockPersonInviteRepository.Verify(repo => repo.Save(personInvite), Times.Once);
    }
}
