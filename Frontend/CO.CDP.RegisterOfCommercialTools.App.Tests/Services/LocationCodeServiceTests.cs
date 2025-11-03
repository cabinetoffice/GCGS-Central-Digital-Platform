using CO.CDP.RegisterOfCommercialTools.App.Services;
using CO.CDP.RegisterOfCommercialTools.WebApiClient;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using CO.CDP.WebApi.Foundation;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WebApiCulture = CO.CDP.RegisterOfCommercialTools.WebApiClient.Models.Culture;

namespace CO.CDP.RegisterOfCommercialTools.App.Tests.Services;

public class LocationCodeServiceTests
{
    private readonly Mock<ICommercialToolsApiClient> _mockClient;
    private readonly LocationCodeService _service;

    public LocationCodeServiceTests()
    {
        _mockClient = new Mock<ICommercialToolsApiClient>();
        var mockLogger = new Mock<ILogger<LocationCodeService>>();
        _service = new LocationCodeService(_mockClient.Object, mockLogger.Object);
    }

    [Fact]
    public async Task GetRootLocationCodesAsync_WhenClientReturnsData_ShouldReturnMappedCodes()
    {
        var dtos = new List<NutsCodeDto>
        {
            new()
            {
                Code = "UKC", DescriptionEn = "North East (England)", DescriptionCy = "Gogledd Ddwyrain (Lloegr)",
                Level = 1
            },
            new()
            {
                Code = "UKD", DescriptionEn = "North West (England)", DescriptionCy = "Gogledd Orllewin (Lloegr)",
                Level = 1
            }
        };
        _mockClient.Setup(x => x.GetRootNutsCodesAsync(It.IsAny<WebApiCulture>())).ReturnsAsync(ApiResult<List<NutsCodeDto>>.Success(dtos));

        var result = await _service.GetRootLocationCodesAsync();

        result.Should().HaveCount(2);
        result[0].Code.Should().Be("UKC");
        result[0].DescriptionEn.Should().Be("North East (England)");
        result[0].DescriptionCy.Should().Be("Gogledd Ddwyrain (Lloegr)");
        result[0].Level.Should().Be(1);
    }

    [Fact]
    public async Task GetRootLocationCodesAsync_WhenClientReturnsNull_ShouldReturnEmptyList()
    {
        _mockClient.Setup(x => x.GetRootNutsCodesAsync(It.IsAny<WebApiCulture>()))
            .ReturnsAsync(ApiResult<List<NutsCodeDto>>.Failure(new ClientError("Not found", System.Net.HttpStatusCode.NotFound)));

        var result = await _service.GetRootLocationCodesAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetChildrenAsync_WhenClientReturnsData_ShouldReturnMappedChildren()
    {
        const string parentCode = "UKC";
        var dtos = new List<NutsCodeDto>
        {
            new()
            {
                Code = "UKC1", DescriptionEn = "Tees Valley and Durham", DescriptionCy = "Cwm Tees a Durham", Level = 2,
                ParentCode = parentCode
            }
        };
        _mockClient.Setup(x => x.GetNutsChildrenAsync(parentCode, It.IsAny<WebApiCulture>())).ReturnsAsync(ApiResult<List<NutsCodeDto>>.Success(dtos));

        var result = await _service.GetChildrenAsync(parentCode);

        result.Should().HaveCount(1);
        result[0].Code.Should().Be("UKC1");
        result[0].Level.Should().Be(2);
    }

    [Fact]
    public async Task SearchAsync_WhenClientReturnsData_ShouldReturnMappedResults()
    {
        const string query = "north";
        var dtos = new List<NutsCodeDto>
        {
            new()
            {
                Code = "UKC", DescriptionEn = "North East (England)", DescriptionCy = "Gogledd Ddwyrain (Lloegr)",
                Level = 1
            }
        };
        _mockClient.Setup(x => x.SearchNutsCodesAsync(query, It.IsAny<WebApiCulture>())).ReturnsAsync(ApiResult<List<NutsCodeDto>>.Success(dtos));

        var result = await _service.SearchAsync(query);

        result.Should().HaveCount(1);
        result[0].Code.Should().Be("UKC");
        result[0].DescriptionEn.Should().Contain("North");
    }

    [Fact]
    public async Task GetByCodeAsync_WhenClientReturnsData_ShouldReturnMappedCode()
    {
        const string code = "UKC";
        var dtos = new List<NutsCodeDto>
        {
            new()
            {
                Code = code, DescriptionEn = "North East (England)", DescriptionCy = "Gogledd Ddwyrain (Lloegr)",
                Level = 1
            }
        };
        _mockClient.Setup(x => x.GetNutsCodesAsync(It.Is<List<string>>(l => l.Count == 1 && l[0] == code)))
            .ReturnsAsync(ApiResult<List<NutsCodeDto>>.Success(dtos));

        var result = await _service.GetByCodeAsync(code);

        result.Should().NotBeNull();
        result.Code.Should().Be(code);
        result.DescriptionEn.Should().Be("North East (England)");
    }

    [Fact]
    public async Task GetByCodeAsync_WhenClientReturnsNull_ShouldReturnNull()
    {
        const string code = "INVALID";
        _mockClient.Setup(x => x.GetNutsCodesAsync(It.Is<List<string>>(l => l.Count == 1 && l[0] == code)))
            .ReturnsAsync(ApiResult<List<NutsCodeDto>>.Failure(new ClientError("Not found", System.Net.HttpStatusCode.NotFound)));

        var result = await _service.GetByCodeAsync(code);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCodeAsync_WhenClientReturnsEmptyList_ShouldReturnNull()
    {
        const string code = "INVALID";
        _mockClient.Setup(x => x.GetNutsCodesAsync(It.Is<List<string>>(l => l.Count == 1 && l[0] == code)))
            .ReturnsAsync(ApiResult<List<NutsCodeDto>>.Success(new List<NutsCodeDto>()));

        var result = await _service.GetByCodeAsync(code);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCodesAsync_WhenClientReturnsData_ShouldReturnMappedCodes()
    {
        var codes = new List<string> { "UKC", "UKD" };
        var dtos = new List<NutsCodeDto>
        {
            new()
            {
                Code = "UKC", DescriptionEn = "North East (England)", DescriptionCy = "Gogledd Ddwyrain (Lloegr)",
                Level = 1
            },
            new()
            {
                Code = "UKD", DescriptionEn = "North West (England)", DescriptionCy = "Gogledd Orllewin (Lloegr)",
                Level = 1
            }
        };
        _mockClient.Setup(x => x.GetNutsCodesAsync(codes)).ReturnsAsync(ApiResult<List<NutsCodeDto>>.Success(dtos));

        var result = await _service.GetByCodesAsync(codes);

        result.Should().HaveCount(2);
        result.Select(x => x.Code).Should().BeEquivalentTo(codes);
    }

    [Fact]
    public async Task GetHierarchyAsync_WhenClientReturnsData_ShouldReturnMappedHierarchy()
    {
        const string code = "UKC11";
        var dtos = new List<NutsCodeDto>
        {
            new()
            {
                Code = "UKC", DescriptionEn = "North East (England)", DescriptionCy = "Gogledd Ddwyrain (Lloegr)",
                Level = 1
            },
            new()
            {
                Code = "UKC1", DescriptionEn = "Tees Valley and Durham", DescriptionCy = "Cwm Tees a Durham", Level = 2,
                ParentCode = "UKC"
            },
            new()
            {
                Code = "UKC11", DescriptionEn = "Hartlepool and Stockton-on-Tees",
                DescriptionCy = "Hartlepool a Stockton-on-Tees",
                Level = 3, ParentCode = "UKC1"
            }
        };
        _mockClient.Setup(x => x.GetNutsHierarchyAsync(code)).ReturnsAsync(ApiResult<List<NutsCodeDto>>.Success(dtos));

        var result = await _service.GetHierarchyAsync(code);

        result.Should().HaveCount(3);
        result[0].Level.Should().Be(1);
        result[1].Level.Should().Be(2);
        result[2].Level.Should().Be(3);
    }

    [Fact]
    public async Task GetChildrenAsync_ShouldPassCorrectParameters()
    {
        const string parentCode = "UKC";
        _mockClient.Setup(x => x.GetNutsChildrenAsync(parentCode, WebApiCulture.English))
            .ReturnsAsync(ApiResult<List<NutsCodeDto>>.Success(new List<NutsCodeDto>()));

        await _service.GetChildrenAsync(parentCode);

        _mockClient.Verify(x => x.GetNutsChildrenAsync(parentCode, Culture.English), Times.Once);
    }
}