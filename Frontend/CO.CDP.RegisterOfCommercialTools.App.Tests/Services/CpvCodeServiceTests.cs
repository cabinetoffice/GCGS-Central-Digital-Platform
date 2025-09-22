using CO.CDP.RegisterOfCommercialTools.App.Services;
using CO.CDP.RegisterOfCommercialTools.WebApiClient;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WebApiCulture = CO.CDP.RegisterOfCommercialTools.WebApiClient.Models.Culture;

namespace CO.CDP.RegisterOfCommercialTools.App.Tests.Services;

public class CpvCodeServiceTests
{
    private readonly Mock<ICommercialToolsApiClient> _mockClient;
    private readonly CpvCodeService _service;

    public CpvCodeServiceTests()
    {
        _mockClient = new Mock<ICommercialToolsApiClient>();
        var mockLogger = new Mock<ILogger<CpvCodeService>>();
        _service = new CpvCodeService(_mockClient.Object, mockLogger.Object);
    }

    [Fact]
    public async Task GetRootCpvCodesAsync_WhenClientReturnsData_ShouldReturnMappedCodes()
    {
        var dtos = new List<CpvCodeDto>
        {
            new()
            {
                Code = "03000000", DescriptionEn = "Agricultural products", DescriptionCy = "Cynhyrchion amaethyddol",
                Level = 1
            },
            new()
            {
                Code = "09000000", DescriptionEn = "Petroleum products", DescriptionCy = "Cynhyrchion petroleum",
                Level = 1
            }
        };
        _mockClient.Setup(x => x.GetRootCpvCodesAsync(It.IsAny<WebApiCulture>())).ReturnsAsync(dtos);

        var result = await _service.GetRootCpvCodesAsync();

        result.Should().HaveCount(2);
        result[0].Code.Should().Be("03000000");
        result[0].DescriptionEn.Should().Be("Agricultural products");
        result[0].DescriptionCy.Should().Be("Cynhyrchion amaethyddol");
        result[0].Level.Should().Be(1);
    }

    [Fact]
    public async Task GetRootCpvCodesAsync_WhenClientReturnsNull_ShouldReturnEmptyList()
    {
        _mockClient.Setup(x => x.GetRootCpvCodesAsync(It.IsAny<WebApiCulture>())).ReturnsAsync((List<CpvCodeDto>?)null);

        var result = await _service.GetRootCpvCodesAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetChildrenAsync_WhenClientReturnsData_ShouldReturnMappedChildren()
    {
        const string parentCode = "03000000";
        var dtos = new List<CpvCodeDto>
        {
            new()
            {
                Code = "03100000", DescriptionEn = "Live animals", DescriptionCy = "Anifeiliaid byw", Level = 2,
                ParentCode = parentCode
            }
        };
        _mockClient.Setup(x => x.GetCpvChildrenAsync(parentCode, It.IsAny<WebApiCulture>())).ReturnsAsync(dtos);

        var result = await _service.GetChildrenAsync(parentCode);

        result.Should().HaveCount(1);
        result[0].Code.Should().Be("03100000");
        result[0].Level.Should().Be(2);
    }

    [Fact]
    public async Task SearchAsync_WhenClientReturnsData_ShouldReturnMappedResults()
    {
        const string query = "agricultural";
        var dtos = new List<CpvCodeDto>
        {
            new()
            {
                Code = "03000000", DescriptionEn = "Agricultural products", DescriptionCy = "Cynhyrchion amaethyddol",
                Level = 1
            }
        };
        _mockClient.Setup(x => x.SearchCpvCodesAsync(query, It.IsAny<WebApiCulture>())).ReturnsAsync(dtos);

        var result = await _service.SearchAsync(query);

        result.Should().HaveCount(1);
        result[0].Code.Should().Be("03000000");
        result[0].DescriptionEn.Should().Contain("Agricultural");
    }

    [Fact]
    public async Task GetByCodeAsync_WhenClientReturnsData_ShouldReturnMappedCode()
    {
        const string code = "03000000";
        var dtos = new List<CpvCodeDto>
        {
            new()
            {
                Code = code, DescriptionEn = "Agricultural products", DescriptionCy = "Cynhyrchion amaethyddol",
                Level = 1
            }
        };
        _mockClient.Setup(x => x.GetCpvCodesAsync(It.Is<List<string>>(l => l.Count == 1 && l[0] == code)))
            .ReturnsAsync(dtos);

        var result = await _service.GetByCodeAsync(code);

        result.Should().NotBeNull();
        result.Code.Should().Be(code);
        result.DescriptionEn.Should().Be("Agricultural products");
    }

    [Fact]
    public async Task GetByCodeAsync_WhenClientReturnsNull_ShouldReturnNull()
    {
        const string code = "99999999";
        _mockClient.Setup(x => x.GetCpvCodesAsync(It.Is<List<string>>(l => l.Count == 1 && l[0] == code)))
            .ReturnsAsync((List<CpvCodeDto>?)null);

        var result = await _service.GetByCodeAsync(code);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCodeAsync_WhenClientReturnsEmptyList_ShouldReturnNull()
    {
        const string code = "99999999";
        _mockClient.Setup(x => x.GetCpvCodesAsync(It.Is<List<string>>(l => l.Count == 1 && l[0] == code)))
            .ReturnsAsync(new List<CpvCodeDto>());

        var result = await _service.GetByCodeAsync(code);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCodesAsync_WhenClientReturnsData_ShouldReturnMappedCodes()
    {
        var codes = new List<string> { "03000000", "09000000" };
        var dtos = new List<CpvCodeDto>
        {
            new()
            {
                Code = "03000000", DescriptionEn = "Agricultural products", DescriptionCy = "Cynhyrchion amaethyddol",
                Level = 1
            },
            new()
            {
                Code = "09000000", DescriptionEn = "Petroleum products", DescriptionCy = "Cynhyrchion petroleum",
                Level = 1
            }
        };
        _mockClient.Setup(x => x.GetCpvCodesAsync(codes)).ReturnsAsync(dtos);

        var result = await _service.GetByCodesAsync(codes);

        result.Should().HaveCount(2);
        result.Select(x => x.Code).Should().BeEquivalentTo(codes);
    }

    [Fact]
    public async Task GetHierarchyAsync_WhenClientReturnsData_ShouldReturnMappedHierarchy()
    {
        const string code = "03110000";
        var dtos = new List<CpvCodeDto>
        {
            new()
            {
                Code = "03000000", DescriptionEn = "Agricultural products", DescriptionCy = "Cynhyrchion amaethyddol",
                Level = 1
            },
            new()
            {
                Code = "03100000", DescriptionEn = "Live animals", DescriptionCy = "Anifeiliaid byw", Level = 2,
                ParentCode = "03000000"
            },
            new()
            {
                Code = "03110000", DescriptionEn = "Live bovine animals", DescriptionCy = "Anifeiliaid gwartheg byw",
                Level = 3, ParentCode = "03100000"
            }
        };
        _mockClient.Setup(x => x.GetCpvHierarchyAsync(code)).ReturnsAsync(dtos);

        var result = await _service.GetHierarchyAsync(code);

        result.Should().HaveCount(3);
        result[0].Level.Should().Be(1);
        result[1].Level.Should().Be(2);
        result[2].Level.Should().Be(3);
    }

    [Fact]
    public async Task GetChildrenAsync_ShouldPassCorrectParameters()
    {
        const string parentCode = "03000000";
        _mockClient.Setup(x => x.GetCpvChildrenAsync(parentCode, WebApiCulture.English))
            .ReturnsAsync(new List<CpvCodeDto>());

        await _service.GetChildrenAsync(parentCode);

        _mockClient.Verify(x => x.GetCpvChildrenAsync(parentCode, Culture.English), Times.Once);
    }
}