using CO.CDP.MQ;
using CO.CDP.Organisation.WebApi.Events;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class UpdatePersonToOrganisationUseCaseTest : IClassFixture<AutoMapperFixture>
{
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Mock<Persistence.IOrganisationRepository> _organisationRepositoryMock = new();
    private readonly Guid _personId = Guid.NewGuid();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly UpdatePersonToOrganisationUseCase _useCase;

    public UpdatePersonToOrganisationUseCaseTest(AutoMapperFixture mapperFixture)
    {
        _useCase = new UpdatePersonToOrganisationUseCase(
            _organisationRepositoryMock.Object,
            _publisherMock.Object,
            NullLogger<UpdatePersonToOrganisationUseCase>.Instance);
    }

    private Persistence.Organisation Organisation =>
        new()
        {
            Guid = _organisationId,
            Name = "Test",
            Type = OrganisationType.Organisation,
            Tenant = It.IsAny<Persistence.Tenant>(),
            ContactPoints = [new Persistence.ContactPoint { Email = "test@test.com" }],
            SupplierInfo = new Persistence.SupplierInformation()
        };

    private Persistence.Person person =>
        new()
        {
            Guid = _personId,
            FirstName = "Test",
            LastName = "Test",
            Email = "Test@test.com",
            UserUrn = "urn:1234",
        };

    private Persistence.OrganisationPerson organisationPerson =>
        new()
        {
            Organisation = Organisation,
            OrganisationId = Organisation.Id,
            PersonId = person.Id,
            Person = person,
            Scopes = ["Viewer"]
        };

    [Fact]
    public async Task Execute_ShouldUpdatePerson_When_Organisation_Person_Exists()
    {
        var updatePersonToOrganisation = new UpdatePersonToOrganisation { Scopes = ["ADMIN", "EDITOR"] };
        var orgPerson = organisationPerson;
        _organisationRepositoryMock.Setup(repo => repo.FindOrganisationPerson(_organisationId, _personId))
            .ReturnsAsync(orgPerson);

        var result = await _useCase.Execute((_organisationId, _personId, updatePersonToOrganisation));

        result.Should().Be(true);
        _organisationRepositoryMock.Verify(repo => repo.TrackOrganisationPerson(orgPerson!), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowEmptyPersonRoleException_WhenPersonScopeIsEmpty()
    {
        var updatePersonToOrganisation = new UpdatePersonToOrganisation { Scopes = [] };
        _organisationRepositoryMock.Setup(repo => repo.FindOrganisationPerson(_organisationId, _personId))
            .ReturnsAsync(organisationPerson);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, _personId, updatePersonToOrganisation));

        await act.Should().ThrowAsync<EmptyPersonRoleException>()
            .WithMessage($"Empty Scope of Person {_personId}.");
    }

    [Fact]
    public async Task Execute_ShouldThrowUnknownPersonException_WhenPersonOrOrganisationNotFound()
    {
        var updatePersonToOrganisation = new UpdatePersonToOrganisation { Scopes = ["Viewer"] };
        _organisationRepositoryMock.Setup(repo => repo.FindOrganisationPerson(_organisationId, _personId))
            .ReturnsAsync((Persistence.OrganisationPerson?)null);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, _personId, updatePersonToOrganisation));

        await act.Should().ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {_organisationId} or Person {_personId}.");
    }

    [Fact]
    public async Task Execute_PublishesPersonScopesUpdated_WhenPersonFound()
    {
        var scopes = new List<string> { "ADMIN", "EDITOR" };
        var updatePersonToOrganisation = new UpdatePersonToOrganisation { Scopes = scopes };
        _organisationRepositoryMock.Setup(repo => repo.FindOrganisationPerson(_organisationId, _personId))
            .ReturnsAsync(organisationPerson);

        await _useCase.Execute((_organisationId, _personId, updatePersonToOrganisation));

        _publisherMock.Verify(p => p.Publish(It.Is<PersonScopesUpdated>(e =>
            e.OrganisationId == _organisationId.ToString() &&
            e.PersonId == _personId.ToString() &&
            e.Scopes.SequenceEqual(scopes))), Times.Once);
    }
}