using AutoMapper;
using Moq;
using FluentAssertions;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;

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
        var command = new PaginatedOrganisationQuery { Type = "buyer" };
        var organisations = new List<CO.CDP.OrganisationInformation.Persistence.Organisation>
        {
            new CO.CDP.OrganisationInformation.Persistence.Organisation
            {
                Guid = Guid.NewGuid(),
                Name = "Organisation 2",
                Tenant = null!
            }
        };
        organisations.Add(new CO.CDP.OrganisationInformation.Persistence.Organisation
        {
            Guid = Guid.NewGuid(),
            Name = "Organisation 1",
            Tenant = null!
        });
        var mappedOrganisations = new List<OrganisationExtended>
        {
            new OrganisationExtended{
                Id = default,
                Name = "Organisation 1",
                Identifier = null!,
                ContactPoint = null!,
                Roles = new List<PartyRole>(),
                Details = null!
            },
            new OrganisationExtended{
                Id = default,
                Name = "Organisation 2",
                Identifier = null!,
                ContactPoint = null!,
                Roles = new List<PartyRole>(),
                Details = null!
            }
        };

        _organisationRepositoryMock.Setup(repo => repo.Get(command.Type)).ReturnsAsync(organisations);
        _mapperMock.Setup(m => m.Map<IEnumerable<OrganisationExtended>>(organisations)).Returns(mappedOrganisations);

        var result = await _useCase.Execute(command);

        result.Should().BeEquivalentTo(mappedOrganisations);
        _organisationRepositoryMock.Verify(repo => repo.Get(command.Type), Times.Once);
        _mapperMock.Verify(m => m.Map<IEnumerable<OrganisationExtended>>(organisations), Times.Once);
    }

    [Fact]
    public async Task Execute_WhenNoOrganisationsExist_ReturnsEmptyList()
    {
        var command = new PaginatedOrganisationQuery { Type = "buyer" };
        var organisations = new List<CO.CDP.OrganisationInformation.Persistence.Organisation>();
        var mappedOrganisations = new List<OrganisationExtended>();

        _organisationRepositoryMock.Setup(repo => repo.Get(command.Type)).ReturnsAsync(organisations);
        _mapperMock.Setup(m => m.Map<IEnumerable<OrganisationExtended>>(organisations)).Returns(mappedOrganisations);

        var result = await _useCase.Execute(command);

        result.Should().BeEmpty();
        _organisationRepositoryMock.Verify(repo => repo.Get(command.Type), Times.Once);
        _mapperMock.Verify(m => m.Map<IEnumerable<OrganisationExtended>>(organisations), Times.Once);
    }
}
