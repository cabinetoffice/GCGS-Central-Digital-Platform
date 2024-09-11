using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Organisation.WebApi.UseCase;

using Moq;
using PersistenceOrganisation = CO.CDP.OrganisationInformation.Persistence.Organisation;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class GetPersonInvitesUseCaseTest
{
    private readonly Mock<IPersonInviteRepository> _personInviteRepositoryMock;
    private readonly Mock<IOrganisationRepository> _organisationRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetPersonInvitesUseCase _useCase;
    private readonly Mock<PersistenceOrganisation> _organisationMock;

    public GetPersonInvitesUseCaseTest()
    {
        _personInviteRepositoryMock = new Mock<IPersonInviteRepository>();
        _organisationRepositoryMock = new Mock<IOrganisationRepository>();
        _mapperMock = new Mock<IMapper>();
        _useCase = new GetPersonInvitesUseCase(_personInviteRepositoryMock.Object, _mapperMock.Object);
        _organisationMock = new Mock<PersistenceOrganisation>();
    }

    [Fact]
    public async Task Execute_OrganisationHasNoPersonInvites_ReturnsEmptyList()
    {
        var organisationId = Guid.NewGuid();
        _organisationRepositoryMock.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync(_organisationMock.Object);
        _personInviteRepositoryMock.Setup(repo => repo.FindByOrganisation(organisationId))
            .ReturnsAsync(new List<PersonInvite>());

        var result = await _useCase.Execute(organisationId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Execute_OrganisationHasInvites_ReturnsMappedPersonInviteModels()
    {
        var organisationId = Guid.NewGuid();
        var personInvites = new List<PersonInvite>
    {
        new PersonInvite {
            Id = 1,
            Guid = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Johnson",
            Email = "john@johnson.com",
            Organisation = null!,
            Person = null,
            Scopes = ["ADMIN", "VIEWER"],
            InviteSentOn = default,
            CreatedOn = default,
            UpdatedOn = default
        },
        new PersonInvite {
            Id = 2,
            Guid = Guid.NewGuid(),
            FirstName = "Bill",
            LastName = "Billson",
            Email = "bill@billson.com",
            Organisation = null!,
            Person = null,
            Scopes = ["EDITOR"],
            InviteSentOn = default,
            CreatedOn = default,
            UpdatedOn = default
        }
    };

        var personInviteModels = new List<PersonInviteModel>
    {
        new PersonInviteModel
        {
            Id = personInvites[0].Guid,
            Email = personInvites[0].Email,
            FirstName = personInvites[0].FirstName,
            LastName = personInvites[0].LastName,
            Scopes = personInvites[0].Scopes
        },
        new PersonInviteModel
        {
            Id = personInvites[1].Guid,
            Email = personInvites[1].Email,
            FirstName = personInvites[1].FirstName,
            LastName = personInvites[1].LastName,
            Scopes = personInvites[1].Scopes
        }
    };

        _personInviteRepositoryMock.Setup(repo => repo.FindByOrganisation(organisationId))
            .ReturnsAsync(personInvites);

        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<PersonInviteModel>>(personInvites))
            .Returns(personInviteModels);

        var result = await _useCase.Execute(organisationId);

        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count());
        Assert.Equal(personInviteModels, result);
        _personInviteRepositoryMock.Verify(repo => repo.FindByOrganisation(organisationId), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<IEnumerable<PersonInviteModel>>(personInvites), Times.Once);
    }
}