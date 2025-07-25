using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using Moq;
using FluentAssertions;
using Identifier = CO.CDP.OrganisationInformation.Persistence.Identifier;
using OrganisationPersistence = CO.CDP.OrganisationInformation.Persistence.Organisation;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class SearchOrganisationByPponUseCaseTests : IClassFixture<AutoMapperFixture>
{
    private readonly AutoMapperFixture _mapperFixture;
    private readonly Mock<IOrganisationRepository> _mockOrganisationRepository;

    public SearchOrganisationByPponUseCaseTests(AutoMapperFixture mapperFixture)
    {
        _mapperFixture = mapperFixture;
        _mockOrganisationRepository = new Mock<IOrganisationRepository>();
    }

    [Fact]
    public async Task Execute_WhenSearchingByPpon_ReturnsMatchingOrganisations()
    {
        // Arrange
        const string searchTerm = "PGWZ-1758-ABCD";
        const int limit = 10;
        const int skip = 0;
        const string orderBy = "asc";

        var organisationQuery = new OrganisationSearchByPponQuery(searchTerm, limit, skip, orderBy);

        var organisations = new List<OrganisationPersistence>
        {
            CreateOrganisation("Test Organisation", "PGWZ-1758-ABCD")
        };

        _mockOrganisationRepository
            .Setup(r => r.SearchByNameOrPpon(searchTerm, limit, skip, orderBy))
            .ReturnsAsync((organisations, organisations.Count));

        var useCase = new SearchOrganisationByPponUseCase(
            _mockOrganisationRepository.Object,
            _mapperFixture.Mapper);

        // Act
        var result = await useCase.Execute(organisationQuery);

        // Assert
        result.Results.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);

        var firstResult = result.Results.First();
        firstResult.Name.Should().Be("Test Organisation");
        firstResult.Identifiers.First().Id.Should().Be("PGWZ-1758-ABCD");
    }

    [Fact]
    public async Task Execute_WhenSearchingByPartialPpon_ReturnsMatchingOrganisations()
    {
        // Arrange
        const string searchTerm = "1758";
        const int limit = 10;
        const int skip = 0;
        const string orderBy = "asc";

        var organisationQuery = new OrganisationSearchByPponQuery(searchTerm, limit, skip, orderBy);

        var organisations = new List<OrganisationPersistence>
        {
            CreateOrganisation("First Organisation", "PGWZ-1758-ABCD"),
            CreateOrganisation("Second Organisation", "PGWZ-1758-EFGH")
        };

        _mockOrganisationRepository
            .Setup(r => r.SearchByNameOrPpon(searchTerm, limit, skip, orderBy))
            .ReturnsAsync((organisations, organisations.Count));

        var useCase = new SearchOrganisationByPponUseCase(
            _mockOrganisationRepository.Object,
            _mapperFixture.Mapper);

        // Act
        var result = await useCase.Execute(organisationQuery);

        // Assert
        result.Results.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);

        result.Results.First().Identifiers.First().Id.Should().Be("PGWZ-1758-ABCD");
        result.Results.Last().Identifiers.First().Id.Should().Be("PGWZ-1758-EFGH");
    }

    [Fact]
    public async Task Execute_WhenSearchingByName_ReturnsMatchingOrganisations()
    {
        // Arrange
        const string searchTerm = "Test";
        const int limit = 10;
        const int skip = 0;
        const string orderBy = "asc";

        var organisationQuery = new OrganisationSearchByPponQuery(searchTerm, limit, skip, orderBy);

        var organisations = new List<OrganisationPersistence>
        {
            CreateOrganisation("Test Organisation A", "PGWZ-1001-AAAA"),
            CreateOrganisation("Test Organisation B", "PGWZ-1002-BBBB")
        };

        _mockOrganisationRepository
            .Setup(r => r.SearchByNameOrPpon(searchTerm, limit, skip, orderBy))
            .ReturnsAsync((organisations, organisations.Count));

        var useCase = new SearchOrganisationByPponUseCase(
            _mockOrganisationRepository.Object,
            _mapperFixture.Mapper);

        // Act
        var result = await useCase.Execute(organisationQuery);

        // Assert
        result.Results.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);

        result.Results.First().Name.Should().Be("Test Organisation A");
        result.Results.Last().Name.Should().Be("Test Organisation B");
    }

    [Fact]
    public async Task Execute_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        const string searchTerm = "Organisation";
        const int limit = 1;
        const int skip = 1;
        const string orderBy = "asc";

        var organisationQuery = new OrganisationSearchByPponQuery(searchTerm, limit, skip, orderBy);

        var organisations = new List<OrganisationPersistence>
        {
            CreateOrganisation("Second Organisation", "PGWZ-2222-BBBB")
        };

        // Repository returns only the second item, but total count is 2
        _mockOrganisationRepository
            .Setup(r => r.SearchByNameOrPpon(searchTerm, limit, skip, orderBy))
            .ReturnsAsync((organisations, 2));

        var useCase = new SearchOrganisationByPponUseCase(
            _mockOrganisationRepository.Object,
            _mapperFixture.Mapper);

        // Act
        var result = await useCase.Execute(organisationQuery);

        // Assert
        result.Results.Should().HaveCount(1);
        result.TotalCount.Should().Be(2);
        result.Results.First().Name.Should().Be("Second Organisation");
    }

    [Fact]
    public async Task Execute_WithDescendingSort_ReturnsCorrectlySortedResults()
    {
        // Arrange
        const string searchTerm = "Organisation";
        const int limit = 10;
        const int skip = 0;
        const string orderBy = "desc";

        var organisationQuery = new OrganisationSearchByPponQuery(searchTerm, limit, skip, orderBy);

        var organisations = new List<OrganisationPersistence>
        {
            CreateOrganisation("Z Organisation", "PGWZ-9999-ZZZZ"),
            CreateOrganisation("A Organisation", "PGWZ-1111-AAAA")
        };

        _mockOrganisationRepository
            .Setup(r => r.SearchByNameOrPpon(searchTerm, limit, skip, orderBy))
            .ReturnsAsync((organisations, organisations.Count));

        var useCase = new SearchOrganisationByPponUseCase(
            _mockOrganisationRepository.Object,
            _mapperFixture.Mapper);

        // Act
        var result = await useCase.Execute(organisationQuery);

        // Assert
        result.Results.Should().HaveCount(2);
        result.Results.First().Name.Should().Be("Z Organisation");
        result.Results.Last().Name.Should().Be("A Organisation");
    }

    [Fact]
    public async Task Execute_WhenNoResults_ReturnsEmptyCollection()
    {
        // Arrange
        const string searchTerm = "NonExistentOrganisation";
        const int limit = 10;
        const int skip = 0;
        const string orderBy = "asc";

        var organisationQuery = new OrganisationSearchByPponQuery(searchTerm, limit, skip, orderBy);

        var organisations = new List<OrganisationPersistence>();

        _mockOrganisationRepository
            .Setup(r => r.SearchByNameOrPpon(searchTerm, limit, skip, orderBy))
            .ReturnsAsync((organisations, 0));

        var useCase = new SearchOrganisationByPponUseCase(
            _mockOrganisationRepository.Object,
            _mapperFixture.Mapper);

        // Act
        var result = await useCase.Execute(organisationQuery);

        // Assert
        result.Results.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    private static OrganisationPersistence CreateOrganisation(string name, string pponId)
    {
        return new OrganisationPersistence
        {
            Name = name,
            Guid = Guid.NewGuid(),
            Type = OrganisationType.Organisation,
            Tenant = new Tenant { Name = name, Guid = Guid.NewGuid() },
            Identifiers =
            [
                new Identifier
                {
                    Primary = true,
                    Scheme = "GB-PPON",
                    IdentifierId = pponId,
                    LegalName = name
                }
            ],
            Roles = { PartyRole.Buyer, PartyRole.Tenderer },
            Addresses =
            {
                new OrganisationInformation.Persistence.OrganisationAddress
                {
                    Type = AddressType.Registered,
                    Address = new OrganisationInformation.Persistence.Address
                    {
                        StreetAddress = "123 Test Street",
                        Locality = "London",
                        PostalCode = "SW1A 1AA",
                        Country = "GB",
                        CountryName = "United Kingdom"
                    }
                }
            }
        };
    }
}