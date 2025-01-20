using AutoMapper;
using Moq;
using FluentAssertions;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;

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
    public async Task Execute_WithValidCommand_ReturnsMappedOrganisations()
    {
        var command = new PaginatedOrganisationQuery { Type = "buyer", Limit = 10, Skip = 0};
        List<CO.CDP.OrganisationInformation.Persistence.Organisation> organisations =
        [
            new() {
                Guid = Guid.NewGuid(),
                Name = "Organisation 2",
                Type = OrganisationType.Organisation,
                Tenant = null!
            },
            new() {
                Guid = Guid.NewGuid(),
                Name = "Organisation 1",
                Type = OrganisationType.Organisation,
                Tenant = null!
            }
        ];
        List<OrganisationExtended> mappedOrganisations =
        [
            new (){
                Id = default,
                Name = "Organisation 1",
                Type = OrganisationType.Organisation,
                Identifier = null!,
                ContactPoint = null!,
                Roles = new List<PartyRole>(),
                Details = null!
            },
            new (){
                Id = default,
                Name = "Organisation 2",
                Type = OrganisationType.Organisation,
                Identifier = null!,
                ContactPoint = null!,
                Roles = new List<PartyRole>(),
                Details = null!
            }
        ];

        _organisationRepositoryMock.Setup(repo => repo.GetPaginated(command.Type, command.Limit, command.Skip)).ReturnsAsync(organisations);
        _mapperMock.Setup(m => m.Map<IEnumerable<OrganisationExtended>>(organisations)).Returns(mappedOrganisations);

        var result = await _useCase.Execute(command);

        result.Should().BeEquivalentTo(mappedOrganisations);
        _organisationRepositoryMock.Verify(repo => repo.GetPaginated(command.Type, command.Limit, command.Skip), Times.Once);
        _mapperMock.Verify(m => m.Map<IEnumerable<OrganisationExtended>>(organisations), Times.Once);
    }

    [Fact]
    public async Task Execute_WhenNoOrganisationsExist_ReturnsEmptyList()
    {
        var command = new PaginatedOrganisationQuery { Type = "buyer" };
        var organisations = new List<CO.CDP.OrganisationInformation.Persistence.Organisation>();
        var mappedOrganisations = new List<OrganisationExtended>();

        _organisationRepositoryMock.Setup(repo => repo.GetPaginated(command.Type, command.Limit, command.Skip)).ReturnsAsync(organisations);
        _mapperMock.Setup(m => m.Map<IEnumerable<OrganisationExtended>>(organisations)).Returns(mappedOrganisations);

        var result = await _useCase.Execute(command);

        result.Should().BeEmpty();
        _organisationRepositoryMock.Verify(repo => repo.GetPaginated(command.Type, command.Limit, command.Skip), Times.Once);
        _mapperMock.Verify(m => m.Map<IEnumerable<OrganisationExtended>>(organisations), Times.Once);
    }
}
