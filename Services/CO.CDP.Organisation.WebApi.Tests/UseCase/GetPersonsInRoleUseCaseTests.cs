using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class GetPersonsInRoleUseCaseTests
{
    private readonly Mock<IPersonRepository> _mockPersonRepository;
    private readonly Mock<IOrganisationRepository> _mockOrganisationRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly GetPersonsInRoleUseCase _useCase;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _personId = Guid.NewGuid();

    public GetPersonsInRoleUseCaseTests()
    {
        _mockPersonRepository = new Mock<IPersonRepository>();
        _mockOrganisationRepository = new Mock<IOrganisationRepository>();
        _mockMapper = new Mock<IMapper>();

        _useCase = new GetPersonsInRoleUseCase(
            _mockPersonRepository.Object,
            _mockOrganisationRepository.Object,
            _mockMapper.Object
        );
    }

    [Fact]
    public async Task Execute_ShouldReturnEmptyList_WhenNoPersonsFound()
    {
        _mockOrganisationRepository.Setup(repo => repo.Find(_organisationId))
            .ReturnsAsync(Organisation);

        _mockPersonRepository.Setup(repo => repo.FindByOrganisation(_organisationId))
            .ReturnsAsync(new List<Persistence.Person>());

        var result = await _useCase.Execute((Organisation.Guid, "ADMIN"));

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Execute_ShouldReturnOnlyAdminPersons_WhenPersonsHaveAdminScope()
    {
        var organisationAdminPerson =
         new Persistence.OrganisationPerson
         {
             Organisation = Organisation,
             OrganisationId = 1,
             PersonId = 1,
             Person = new Persistence.Person() { Email = "", FirstName = "", Guid = Guid.Empty, LastName = "", UserUrn = "" },
             Scopes = ["Admin", "Viewer"]
         };

        var organisationViewerPerson =
         new Persistence.OrganisationPerson
         {
             Organisation = Organisation,
             OrganisationId = 1,
             PersonId = 2,
             Person = new Persistence.Person() { Email = "", FirstName = "", Guid = Guid.Empty, LastName = "", UserUrn = "" },
             Scopes = ["Viewer"]
         };

        var person_admin =
           new Persistence.Person
           {
               Id = 1,
               Guid = _personId,
               FirstName = "Test",
               LastName = "Test",
               Email = "Test@test.com",
               UserUrn = "urn:1234",
               PersonOrganisations = new List<OrganisationPerson>() { organisationAdminPerson }
           };

        var person_viewer =
           new Persistence.Person
           {
               Id = 2,
               Guid = _personId,
               FirstName = "TestViewer",
               LastName = "TestViewer",
               Email = "TestViewer@test.com",
               UserUrn = "urn:4321",
               PersonOrganisations = new List<OrganisationPerson>() { organisationViewerPerson }
           };

        var persons = new List<Persistence.Person>() { person_admin, person_viewer };

        _mockOrganisationRepository.Setup(repo => repo.Find(_organisationId))
            .ReturnsAsync(Organisation);

        _mockPersonRepository.Setup(repo => repo.FindByOrganisation(_organisationId))
            .ReturnsAsync(persons);

        _mockMapper.Setup(mapper => mapper.Map<Model.Person>(persons[0]))
            .Returns(new Model.Person() { Email = persons[0].Email, FirstName = persons[0].FirstName, Id = persons[0].Guid, LastName = persons[0].LastName, Scopes = persons[0].Scopes });
        _mockMapper.Setup(mapper => mapper.Map<Model.Person>(persons[1]))
            .Returns(new Model.Person() { Email = persons[1].Email, FirstName = persons[1].FirstName, Id = persons[1].Guid, LastName = persons[1].LastName, Scopes = persons[1].Scopes });

        var result = await _useCase.Execute((Organisation.Guid, "ADMIN"));

        result.Should().HaveCount(1);
        result.First().Id.Should().Be(person_admin.Guid);
        result.First().Scopes.Should().Contain("Admin");
    }

    private Persistence.Organisation Organisation =>
        new Persistence.Organisation
        {
            Id = 1,
            Guid = _organisationId,
            Name = "Test",
            Type = OrganisationInformation.OrganisationType.Organisation,
            Tenant = It.IsAny<Tenant>(),
            ContactPoints = [new Persistence.ContactPoint { Email = "test@test.com" }],
            SupplierInfo = new Persistence.SupplierInformation()
        };

}