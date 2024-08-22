using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Organisation.WebApi.UseCase;
using FluentAssertions;
using Moq;
using Organisation = CO.CDP.OrganisationInformation.Persistence.Organisation;
using Person = CO.CDP.OrganisationInformation.Persistence.Person;

public class GetPersonsUseCaseTest
{
    private readonly Mock<IPersonRepository> _personRepositoryMock;
    private readonly Mock<IOrganisationRepository> _organisationRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetPersonsUseCase _useCase;
    private readonly Mock<Organisation> _organisationMock;

    public GetPersonsUseCaseTest()
    {
        _personRepositoryMock = new Mock<IPersonRepository>();
        _organisationRepositoryMock = new Mock<IOrganisationRepository>();
        _mapperMock = new Mock<IMapper>();
        _useCase = new GetPersonsUseCase(_personRepositoryMock.Object, _organisationRepositoryMock.Object, _mapperMock.Object);
        _organisationMock = new Mock<Organisation>();
    }

    [Fact]
    public async Task Execute_OrganisationDoesNotExist_ReturnsEmptyList()
    {
        var organisationId = Guid.NewGuid();
        _organisationRepositoryMock.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync((Organisation)null!);

        Func<Task> act = async () => await _useCase.Execute(organisationId);

        await act.Should().ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {organisationId}.");
    }

    [Fact]
    public async Task Execute_OrganisationExistsButNoPersons_ReturnsEmptyList()
    {
        var organisationId = Guid.NewGuid();
        _organisationRepositoryMock.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync(_organisationMock.Object);
        _personRepositoryMock.Setup(repo => repo.FindByOrganisation(organisationId))
            .ReturnsAsync(new List<Person>());

        var result = await _useCase.Execute(organisationId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Execute_OrganisationExistsAndHasPersons_ReturnsPersonModels()
    {
        var organisationGuid = Guid.NewGuid();
        var organisation = new Organisation
        {
            Id = 1,
            Guid = organisationGuid,
            Tenant = null!,
            Name = null!
        };

        var persons = new List<Person>
        {
            new Person
            {
                Id = 1,
                PersonOrganisations = new List<OrganisationPerson>
                {
                    new OrganisationPerson
                    {
                        OrganisationId = 1,
                        Scopes = ["Scope1"],
                        Person = null!,
                        Organisation = organisation
                    }
                },
                Guid = default,
                FirstName = "Bill",
                LastName = "Billson",
                Email = "bill@billson.com"
            },
            new Person
            {
                Id = 2,
                PersonOrganisations = new List<OrganisationPerson>
                {
                    new OrganisationPerson
                    {
                        OrganisationId = 1,
                        Scopes = ["Scope2"],
                        Person = null!,
                        Organisation = organisation
                    }
                },
                Guid = default,
                FirstName = "John",
                LastName = "Johnson",
                Email = "john@johnson.com"
            },
        };

        _organisationRepositoryMock.Setup(repo => repo.Find(organisationGuid))
            .ReturnsAsync(organisation);

        _personRepositoryMock.Setup(repo => repo.FindByOrganisation(organisationGuid))
            .ReturnsAsync(persons);

        _mapperMock.Setup(mapper => mapper.Map<CO.CDP.Organisation.WebApi.Model.Person>(It.IsAny<Person>()))
            .Returns((Person source) => new CO.CDP.Organisation.WebApi.Model.Person
            {
                Id = Guid.NewGuid(),
                FirstName = null!,
                LastName = null!,
                Email = null!
            });

        var result = await _useCase.Execute(organisationGuid);

        Assert.Equal(2, result.Count());
        result.First().Scopes.Should().Contain("Scope1");
        result.Last().Scopes.Should().Contain("Scope2");
    }
}
