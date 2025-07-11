using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using Moq;
using FluentAssertions;
using OrganisationAddress = CO.CDP.Organisation.WebApi.Model.OrganisationAddress;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class SearchOrganisationByPponUseCaseTests : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IOrganisationRepository> _organisationRepositoryMock;
    private readonly SearchOrganisationByPponUseCase _useCase;

    public SearchOrganisationByPponUseCaseTests(AutoMapperFixture mapperFixture)
    {
        _organisationRepositoryMock = new Mock<IOrganisationRepository>();
        _useCase = new SearchOrganisationByPponUseCase(_organisationRepositoryMock.Object, mapperFixture.Mapper);
    }

    [Fact]
    public async Task Execute_ShouldReturnMappedSearchResults_WhenOrganisationsFound()
    {
        List<CO.CDP.OrganisationInformation.Persistence.Organisation> organisations =
        [
            new()
            {
                Guid = Guid.NewGuid(),
                Name = "Organisation 1",
                Type = OrganisationType.Organisation,
                Tenant = null!,
                Roles = new List<PartyRole>()
                {
                    PartyRole.Buyer
                },
                Identifiers = new List<OrganisationInformation.Persistence.Identifier>()
                {
                    new()
                    {
                        Primary = true,
                        Scheme = "scheme",
                        IdentifierId = "123",
                        LegalName = "legal name"
                    },
                }
            },
            new()
            {
                Guid = Guid.NewGuid(),
                Name = "Organisation 2",
                Type = OrganisationType.Organisation,
                Tenant = null!,
                Roles = new List<PartyRole>()
                {
                    PartyRole.Buyer
                },
                Identifiers = new List<OrganisationInformation.Persistence.Identifier>()
                {
                    new()
                    {
                        Primary = true,
                        Scheme = "scheme",
                        IdentifierId = "456",
                        LegalName = "legal name 2"
                    },
                }
            }
        ];

        var expectedResults = new List<OrganisationSearchByPponResult>
        {
            new OrganisationSearchByPponResult
            {
                Id = organisations[0].Guid,
                Identifiers = new List<OrganisationInformation.Identifier>
                {
                    new()
                    {
                        Scheme = "scheme",
                        Id = "123",
                        LegalName = "legal name"
                    }
                },
                Name = "Organisation 1",
                Roles = new List<PartyRole>
                {
                    PartyRole.Buyer
                },
                Type = OrganisationType.Organisation,
                Addresses = new List<OrganisationAddress>()
            },
            new OrganisationSearchByPponResult
            {
                Id = organisations[1].Guid,
                Identifiers = new List<OrganisationInformation.Identifier>
                {
                    new()
                    {
                        Scheme = "scheme",
                        Id = "456",
                        LegalName = "legal name 2"
                    }
                },
                Name = "Organisation 2",
                Roles = new List<PartyRole>
                {
                    PartyRole.Buyer
                },
                Type = OrganisationType.Organisation,
                Addresses = new List<OrganisationAddress>()
            }
        };

        _organisationRepositoryMock
            .Setup(repo => repo.SearchByNameOrPpon(It.IsAny<string>(), 10, 0,"asc"))
            .ReturnsAsync((organisations,organisations.Count));

        var results = await _useCase.Execute(new OrganisationSearchByPponQuery("Test", 10, 0,"asc"));

        results.Results.Should().BeEquivalentTo(expectedResults,
            options => options.ComparingByMembers<OrganisationSearchByPponResult>());
    }

    [Fact]
    public async Task Execute_ShouldReturnEmptyList_WhenNoMatchingOrganisations()
    {
        _organisationRepositoryMock
            .Setup(repo => repo.SearchByNameOrPpon(It.IsAny<string>(), 20, 5,"asc"))
            .ReturnsAsync(([],0));

        var results = await _useCase.Execute(new OrganisationSearchByPponQuery("NonexistentOrg", 20, 5,"asc"));

        results.Results.Should().BeEmpty();
    }
}