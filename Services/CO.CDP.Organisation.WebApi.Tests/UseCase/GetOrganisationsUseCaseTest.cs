using AutoMapper;
using Moq;
using FluentAssertions;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.Authentication;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class GetOrganisationsUseCaseTests
{
    private readonly Mock<IOrganisationRepository> _organisationRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetOrganisationsUseCase _useCase;

    public GetOrganisationsUseCaseTests()
    {
        _organisationRepositoryMock = new Mock<IOrganisationRepository>();
        _mapperMock = new Mock<IMapper>();
        _useCase = new GetOrganisationsUseCase(_organisationRepositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Execute_WithValidCommand_ReturnsMappedOrganisations_WithAdminPerson()
    {
        var command = new PaginatedOrganisationQuery(limit: 10, skip: 0, "buyer", "buyer");

        var guid = Guid.NewGuid();

        var adminPerson = new OrganisationInformation.Persistence.Person
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Guid = guid,
            Email = null!
        };

        var adminPersonWeb = new Model.Person
        {
            Id = guid,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@doe.com"
        };

        var organisationPersons = new List<OrganisationPerson>
        {
            new OrganisationPerson
            {
                Person = adminPerson,
                Scopes = new List<string> { Constants.OrganisationPersonScope.Admin }
            }
        };

        var organisations = new List<CO.CDP.OrganisationInformation.Persistence.Organisation>
        {
            new CO.CDP.OrganisationInformation.Persistence.Organisation
            {
                Guid = Guid.NewGuid(),
                Name = "Organisation 1",
                Type = OrganisationType.Organisation,
                Tenant = null!, // Ensure Tenant is initialized if required
                OrganisationPersons = organisationPersons, // Ensure OrganisationPersons is not null
                Identifiers = new List<CO.CDP.OrganisationInformation.Persistence.Organisation.Identifier>(),
                Addresses = new List<CO.CDP.OrganisationInformation.Persistence.Organisation.OrganisationAddress>(),
                ContactPoints = new List<CO.CDP.OrganisationInformation.Persistence.Organisation.ContactPoint>(),
                Roles = new List<PartyRole>(),
                PendingRoles = new List<PartyRole>()
            }
        };

        List<OrganisationExtended> mappedOrganisations =
        [
            new OrganisationExtended
            {
                Id = default,
                Name = "Organisation 1",
                Type = OrganisationType.Organisation,
                Identifier = null!,
                ContactPoint = null!,
                Roles = new List<PartyRole>(),
                Details = null!,
                AdminPerson = adminPersonWeb // Ensure AdminPerson is included
            }
        ];

        _organisationRepositoryMock
            .Setup(repo => repo.GetPaginated(command.Role, command.PendingRole, command.Limit, command.Skip))
            .ReturnsAsync(organisations);

        _mapperMock.Setup(m => m.Map<Model.Person>(adminPerson))
            .Returns(adminPersonWeb);

        _mapperMock.Setup(m => m.Map<OrganisationExtended>(organisations[0]))
            .Returns(mappedOrganisations[0]);

        var result = await _useCase.Execute(command);

        result.Should().BeEquivalentTo(mappedOrganisations, options => options.Including(o => o.AdminPerson));
        _organisationRepositoryMock.Verify(repo => repo.GetPaginated(command.Role, command.PendingRole, command.Limit, command.Skip), Times.Once);
        _mapperMock.Verify(m => m.Map<CO.CDP.Organisation.WebApi.Model.Person>(adminPerson), Times.Once);
    }


    [Fact]
    public async Task Execute_WhenNoOrganisationsExist_ReturnsEmptyList()
    {
        var command = new PaginatedOrganisationQuery(limit: 10, skip: 0, "buyer", "buyer");
        var organisations = new List<CO.CDP.OrganisationInformation.Persistence.Organisation>();

        _organisationRepositoryMock
            .Setup(repo => repo.GetPaginated(command.Role, command.PendingRole, command.Limit, command.Skip))
            .ReturnsAsync(organisations);

        var result = await _useCase.Execute(command);

        result.Should().BeEmpty();
        _organisationRepositoryMock.Verify(repo => repo.GetPaginated(command.Role, command.PendingRole, command.Limit, command.Skip), Times.Once);
    }
}
