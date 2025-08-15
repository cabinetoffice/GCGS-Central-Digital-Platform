using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using Moq;
using FluentAssertions;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class SearchOrganisationUseCaseTests : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IOrganisationRepository> _organisationRepositoryMock;
    private readonly SearchOrganisationUseCase _useCase;

    public SearchOrganisationUseCaseTests(AutoMapperFixture mapperFixture)
    {
        _organisationRepositoryMock = new Mock<IOrganisationRepository>();
        _useCase = new SearchOrganisationUseCase(_organisationRepositoryMock.Object, mapperFixture.Mapper);
    }

    [Fact]
    public async Task Execute_ShouldReturnMappedSearchResults_WhenOrganisationFound()
    {
        var organisationId = Guid.NewGuid();


        List<CO.CDP.OrganisationInformation.Persistence.Organisation> organisations =
        [
            new() {
                Guid = Guid.NewGuid(),
                Name = "Organisation 1",
                Type = OrganisationType.Organisation,
                Tenant = null!,
                Roles = new List<PartyRole>() {
                    PartyRole.Buyer
                },
                Identifiers = new List<OrganisationInformation.Persistence.Identifier>() {
                    new() {
                        Primary = true,
                        Scheme = "scheme",
                        IdentifierId = "123",
                        LegalName = "legal name"
                    },
                }
            },
            new() {
                Guid = Guid.NewGuid(),
                Name = "Organisation 2",
                Type = OrganisationType.Organisation,
                Tenant = null!,
                Roles = new List<PartyRole>() {
                    PartyRole.Buyer
                },
                Identifiers = new List<OrganisationInformation.Persistence.Identifier>() {
                    new() {
                        Primary = true,
                        Scheme = "scheme",
                        IdentifierId = "123",
                        LegalName = "legal name"
                    },
                }
            }
        ];

        var searchResults = new List<OrganisationSearchResult>
        {
            new OrganisationSearchResult
            {
                Id = organisations[0].Guid,
                Identifier = new OrganisationInformation.Identifier
                {
                    Scheme = "scheme",
                    Id = "123",
                    LegalName = "legal name"
                },
                Name = "Organisation 1",
                Roles = new List<PartyRole>
                {
                    PartyRole.Buyer
                },
                Type = OrganisationType.Organisation
            },
            new OrganisationSearchResult
            {
                Id = organisations[1].Guid,
                Identifier = new OrganisationInformation.Identifier
                {
                    Scheme = "scheme",
                    Id = "123",
                    LegalName = "legal name"
                },
                Name = "Organisation 2",
                Roles = new List<PartyRole>
                {
                    PartyRole.Buyer
                },
                Type = OrganisationType.Organisation
            }
        };

        _organisationRepositoryMock
            .Setup(repo => repo.SearchByName(It.IsAny<string>(), null, 10, 0.5, false))
            .ReturnsAsync(organisations);

        var results = await _useCase.Execute(new OrganisationSearchQuery("Test org", 10, 0.5));

        results.Should().BeEquivalentTo(searchResults, options => options.ComparingByMembers<OrganisationSearchResult>());
    }

    [Fact]
    public async Task Execute_ShouldReturnEmptyList_WhenNoMatchingOrganisations()
    {
        var organisationId = Guid.NewGuid();

        _organisationRepositoryMock
            .Setup(repo => repo.SearchByName(It.IsAny<string>(), null, 10, 0.3, false))
            .ReturnsAsync([]);

        var results = await _useCase.Execute(new OrganisationSearchQuery("Test org", 10, 0.3));

        results.Should().BeEmpty();
    }
}
