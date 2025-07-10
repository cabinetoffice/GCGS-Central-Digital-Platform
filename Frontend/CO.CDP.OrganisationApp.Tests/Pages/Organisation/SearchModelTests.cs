using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Organisation;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using AddressType = CO.CDP.Organisation.WebApiClient.AddressType;
using Identifier = CO.CDP.Organisation.WebApiClient.Identifier;
using OrganisationType = CO.CDP.Organisation.WebApiClient.OrganisationType;
using PartyRole = CO.CDP.Organisation.WebApiClient.PartyRole;

namespace CO.CDP.OrganisationApp.Tests.Pages.Organisation;

public class SearchModelTests
{
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly Mock<ISession> _mockSession;
    private readonly SearchModel _sut;

    public SearchModelTests()
    {
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        _mockSession = new Mock<ISession>();

        _sut = new SearchModel(_mockOrganisationClient.Object, _mockSession.Object);
    }

    [Fact]
    public async Task OnGet_WhenCalledWithoutSearchText_ShouldReturnPageWithEmptyResults()
    {
        string type = "buyer";
        int pageNumber = 1;
        string? searchText = null;

        var result = await _sut.OnGet(type, pageNumber, searchText);

        result.Should().BeOfType<PageResult>();
        _sut.Organisations.Should().BeEmpty();
        _sut.TotalOrganisations.Should().Be(0);
        _sut.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task OnGet_WhenCalledWithWhitespaceSearchText_ShouldReturnEmptyResults()
    {
        string type = "buyer";
        int pageNumber = 1;
        string searchText = "  ";

        var result = await _sut.OnGet(type, pageNumber, searchText);

        result.Should().BeOfType<PageResult>();
        _sut.Organisations.Should().BeEmpty();
        _sut.TotalOrganisations.Should().Be(0);
        _sut.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task OnGet_WhenCalledWithSpecialCharactersOnly_ShouldReturnEmptyResults()
    {
        string type = "buyer";
        int pageNumber = 1;
        string searchText = "@#$%";

        var result = await _sut.OnGet(type, pageNumber, searchText);

        result.Should().BeOfType<PageResult>();
        _sut.Organisations.Should().BeEmpty();
        _sut.TotalOrganisations.Should().Be(0);
        _sut.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task OnGet_WhenCalledWithValidSearchText_ShouldReturnResults()
    {
        string type = "buyer";
        int pageNumber = 1;
        string searchText = "Test Organisation";
        int pageSize = 50;
        int skip = 0;

        var mockResults = new List<OrganisationSearchByPponResult>
        {
            new(
                new List<OrganisationAddress>
                {
                    new OrganisationAddress(
                        country: "",
                        streetAddress: "123 Test Street",
                        locality: "Test City",
                        region: "Test Region",
                        postalCode: "AB12 3CD",
                        countryName: "United Kingdom",
                        type: AddressType.Postal)
                },
                Guid.NewGuid(),
                new List<Identifier>
                {
                    new Identifier(id: "123456", scheme: "GB-PPON", legalName: "Test Organisation 1",
                        uri: new Uri("https://www.example.com"))
                },
                "Test Organisation 1",
                new List<PartyRole> { PartyRole.Buyer },
                OrganisationType.Organisation
            ),
            new(
                new List<OrganisationAddress>
                {
                    new OrganisationAddress(
                        country: "",
                        streetAddress: "123 Test Street",
                        locality: "Test City",
                        region: "Test Region",
                        postalCode: "AB12 3CD",
                        countryName: "United Kingdom",
                        type: AddressType.Postal)
                },
                Guid.NewGuid(),
                new List<Identifier>
                {
                    new Identifier(id: "123456", scheme: "GB-PPON", legalName: "Test Organisation 2",
                        uri: new Uri("https://www.example.com"))
                },
                "Test Organisation 2",
                new List<PartyRole> { PartyRole.Supplier },
                OrganisationType.Organisation
            )
        };

        _mockOrganisationClient.Setup(m => m.SearchByNameOrPponAsync(
                It.Is<string>(s => s == searchText.Trim()),
                It.Is<int>(i => i == pageSize),
                It.Is<int>(i => i == skip)))
            .ReturnsAsync(mockResults);

        var result = await _sut.OnGet(type, pageNumber, searchText);

        result.Should().BeOfType<PageResult>();
        _sut.Organisations.Should().HaveCount(2);
        _sut.TotalOrganisations.Should().Be(2);
        _sut.TotalPages.Should().Be(1);
        _sut.Organisations.ElementAt(0).Name.Should().Be("Test Organisation 1");
        _sut.Organisations.ElementAt(1).Name.Should().Be("Test Organisation 2");
    }

    [Theory]
    [InlineData("ascending")]
    [InlineData("descending")]
    [InlineData("relevance")]
    public async Task OnGet_ShouldSortResultsBasedOnSortOrder(string sortOrder)
    {
        string type = "buyer";
        int pageNumber = 1;
        string searchText = "Test Organisation";

        var mockResults = new List<OrganisationSearchByPponResult>
        {
            new(
                new List<OrganisationAddress>(),
                Guid.NewGuid(),
                new List<Identifier>(),
                "C Organisation",
                new List<PartyRole>(),
                OrganisationType.Organisation
            ),
            new(
                new List<OrganisationAddress>(),
                Guid.NewGuid(),
                new List<Identifier>(),
                "A Organisation",
                new List<PartyRole>(),
                OrganisationType.Organisation
            ),
            new(
                new List<OrganisationAddress>(),
                Guid.NewGuid(),
                new List<Identifier>(),
                "B Organisation",
                new List<PartyRole>(),
                OrganisationType.Organisation
            )
        };

        _mockOrganisationClient.Setup(m => m.SearchByNameOrPponAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync(mockResults);

        var result = await _sut.OnGet(type, pageNumber, searchText, sortOrder);

        result.Should().BeOfType<PageResult>();

        if (sortOrder == "ascending")
        {
            _sut.Organisations.Select(o => o.Name).Should().BeInAscendingOrder();
            _sut.Organisations.ElementAt(0).Name.Should().Be("A Organisation");
            _sut.Organisations.ElementAt(2).Name.Should().Be("C Organisation");
        }
        else if (sortOrder == "descending")
        {
            _sut.Organisations.Select(o => o.Name).Should().BeInDescendingOrder();
            _sut.Organisations.ElementAt(0).Name.Should().Be("C Organisation");
            _sut.Organisations.ElementAt(2).Name.Should().Be("A Organisation");
        }
        else
        {
            _sut.Organisations.ElementAt(0).Name.Should().Be("C Organisation");
            _sut.Organisations.ElementAt(1).Name.Should().Be("A Organisation");
            _sut.Organisations.ElementAt(2).Name.Should().Be("B Organisation");
        }
    }

    [Fact]
    public void FormatAddresses_WhenAddressesIsEmpty_ShouldReturnNotApplicable()
    {
        var addresses = new List<OrganisationAddress>();
        var result = _sut.FormatAddresses(addresses);
        result.Should().Be("N/A");
    }

    [Fact]
    public void FormatAddresses_WithCompleteAddress_ShouldFormatCorrectly()
    {
        var addresses = new List<OrganisationAddress>
        {
            new OrganisationAddress(
                country: "United Kingdom",
                streetAddress: "123 Test Street",
                locality: "Test City",
                region: "Test Region",
                postalCode: "AB12 3CD",
                countryName: "",
                type: AddressType.Postal)
        };

        var result = _sut.FormatAddresses(addresses);
        result.Should().Be("123 Test Street, Test City, Test Region, AB12 3CD, United Kingdom");
    }

    [Fact]
    public void FormatAddresses_WithPartialAddress_ShouldFormatWithAvailableParts()
    {
        var addresses = new List<OrganisationAddress>
        {
            new(
                country: "",
                streetAddress: "123 Test Street",
                locality: null,
                region: "Test Region",
                postalCode: null,
                countryName: "United Kingdom",
                type: AddressType.Postal)
        };

        var result = _sut.FormatAddresses(addresses);
        result.Should().Be("123 Test Street, Test Region");
    }

    [Fact]
    public void FormatAddresses_WithEmptyAddressParts_ShouldReturnNotApplicable()
    {
        var addresses = new List<OrganisationAddress>
        {
            new(
                country: "",
                streetAddress: null,
                locality: null,
                region: null,
                postalCode: null,
                countryName: null,
                type: AddressType.Postal)
        };

        var result = _sut.FormatAddresses(addresses);
        result.Should().Be("N/A");
    }

    [Theory]
    [InlineData(PartyRole.Buyer, "govuk-tag--blue")]
    [InlineData(PartyRole.Supplier, "govuk-tag--purple")]
    [InlineData(PartyRole.Tenderer, "govuk-tag--yellow")]
    [InlineData(PartyRole.Funder, "govuk-tag--red")]
    public void GetTagClassForRole_ShouldReturnCorrectTagClass(PartyRole role, string expectedTagClass)
    {
        var result = _sut.GetTagClassForRole(role);
        result.Should().Be(expectedTagClass);
    }
}