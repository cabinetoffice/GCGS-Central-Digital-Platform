using CO.CDP.RegisterOfCommercialTools.Persistence;
using CO.CDP.RegisterOfCommercialTools.WebApi.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Tests.Controllers;

public class CpvCodeControllerTests
{
    private readonly Mock<ICpvCodeRepository> _mockRepository;
    private readonly CpvCodeController _controller;

    public CpvCodeControllerTests()
    {
        _mockRepository = new Mock<ICpvCodeRepository>();
        _controller = new CpvCodeController(_mockRepository.Object);
    }

    [Fact]
    public async Task GetRootCodes_WhenRepositoryReturnsData_ShouldReturnOkWithCodes()
    {
        var expectedCodes = new List<CpvCode>
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
        _mockRepository.Setup(x => x.GetRootCodesAsync(Culture.English)).ReturnsAsync(expectedCodes);

        var result = await _controller.GetRootCodes();

        var okResult = result.Should().BeOfType<ActionResult<List<CpvCode>>>().Subject;
        var okObjectResult = okResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var codes = okObjectResult.Value.Should().BeOfType<List<CpvCode>>().Subject;
        codes.Should().HaveCount(2);
        codes[0].Code.Should().Be("03000000");
        codes[1].Code.Should().Be("09000000");
    }

    [Fact]
    public async Task GetRootCodes_WhenRepositoryReturnsEmptyList_ShouldReturnOkWithEmptyList()
    {
        _mockRepository.Setup(x => x.GetRootCodesAsync(Culture.English)).ReturnsAsync(new List<CpvCode>());

        var result = await _controller.GetRootCodes();

        var okResult = result.Should().BeOfType<ActionResult<List<CpvCode>>>().Subject;
        var okObjectResult = okResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var codes = okObjectResult.Value.Should().BeOfType<List<CpvCode>>().Subject;
        codes.Should().BeEmpty();
    }

    [Theory]
    [InlineData(Culture.English)]
    [InlineData(Culture.Welsh)]
    public async Task GetRootCodes_ShouldPassCultureToRepository(Culture culture)
    {
        _mockRepository.Setup(x => x.GetRootCodesAsync(culture)).ReturnsAsync(new List<CpvCode>());

        await _controller.GetRootCodes(culture);

        _mockRepository.Verify(x => x.GetRootCodesAsync(culture), Times.Once);
    }

    [Fact]
    public async Task GetChildren_WhenRepositoryReturnsData_ShouldReturnOkWithChildren()
    {
        const string parentCode = "03000000";
        var expectedChildren = new List<CpvCode>
        {
            new()
            {
                Code = "03100000", DescriptionEn = "Live animals", DescriptionCy = "Anifeiliaid byw", Level = 2,
                ParentCode = parentCode
            }
        };
        _mockRepository.Setup(x => x.GetChildrenAsync(parentCode, Culture.English)).ReturnsAsync(expectedChildren);

        var result = await _controller.GetChildren(parentCode);

        var okResult = result.Should().BeOfType<ActionResult<List<CpvCode>>>().Subject;
        var okObjectResult = okResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var codes = okObjectResult.Value.Should().BeOfType<List<CpvCode>>().Subject;
        codes.Should().HaveCount(1);
        codes[0].Code.Should().Be("03100000");
        codes[0].ParentCode.Should().Be(parentCode);
    }

    [Fact]
    public async Task GetChildren_WhenRepositoryReturnsEmptyList_ShouldReturnOkWithEmptyList()
    {
        const string parentCode = "99999999";
        _mockRepository.Setup(x => x.GetChildrenAsync(parentCode, Culture.English)).ReturnsAsync(new List<CpvCode>());

        var result = await _controller.GetChildren(parentCode);

        var okResult = result.Should().BeOfType<ActionResult<List<CpvCode>>>().Subject;
        var okObjectResult = okResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var codes = okObjectResult.Value.Should().BeOfType<List<CpvCode>>().Subject;
        codes.Should().BeEmpty();
    }

    [Fact]
    public async Task Search_WithValidQuery_ShouldReturnOkWithResults()
    {
        const string query = "agricultural";
        var expectedResults = new List<CpvCode>
        {
            new()
            {
                Code = "03000000", DescriptionEn = "Agricultural products", DescriptionCy = "Cynhyrchion amaethyddol",
                Level = 1
            }
        };
        _mockRepository.Setup(x => x.SearchAsync(query, Culture.English)).ReturnsAsync(expectedResults);

        var result = await _controller.Search(query);

        var okResult = result.Should().BeOfType<ActionResult<List<CpvCode>>>().Subject;
        var okObjectResult = okResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var codes = okObjectResult.Value.Should().BeOfType<List<CpvCode>>().Subject;
        codes.Should().HaveCount(1);
        codes[0].Code.Should().Be("03000000");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Search_WithInvalidQuery_ShouldReturnBadRequest(string? query)
    {
        var result = await _controller.Search(query!);

        var badRequestResult = result.Should().BeOfType<ActionResult<List<CpvCode>>>().Subject;
        badRequestResult.Result.Should().BeOfType<BadRequestObjectResult>();

        _mockRepository.Verify(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<Culture>()), Times.Never);
    }

    [Fact]
    public async Task Search_WhenRepositoryReturnsEmptyList_ShouldReturnOkWithEmptyList()
    {
        const string query = "nonexistent";
        _mockRepository.Setup(x => x.SearchAsync(query, Culture.English)).ReturnsAsync(new List<CpvCode>());

        var result = await _controller.Search(query);

        var okResult = result.Should().BeOfType<ActionResult<List<CpvCode>>>().Subject;
        var okObjectResult = okResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var codes = okObjectResult.Value.Should().BeOfType<List<CpvCode>>().Subject;
        codes.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByCodes_WithValidCodes_ShouldReturnOkWithCodes()
    {
        var codes = new List<string> { "03000000", "09000000" };
        var expectedCodes = new List<CpvCode>
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
        _mockRepository.Setup(x => x.GetByCodesAsync(codes)).ReturnsAsync(expectedCodes);

        var result = await _controller.GetByCodes(codes);

        var okResult = result.Should().BeOfType<ActionResult<List<CpvCode>>>().Subject;
        var okObjectResult = okResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedCodes = okObjectResult.Value.Should().BeOfType<List<CpvCode>>().Subject;
        returnedCodes.Should().HaveCount(2);
        returnedCodes.Select(x => x.Code).Should().BeEquivalentTo(codes);
    }

    [Fact]
    public async Task GetByCodes_WithEmptyList_ShouldReturnBadRequest()
    {
        var emptyCodes = new List<string>();

        var result = await _controller.GetByCodes(emptyCodes);

        var badRequestResult = result.Should().BeOfType<ActionResult<List<CpvCode>>>().Subject;
        badRequestResult.Result.Should().BeOfType<BadRequestObjectResult>();

        _mockRepository.Verify(x => x.GetByCodesAsync(It.IsAny<List<string>>()), Times.Never);
    }

    [Fact]
    public async Task GetByCodes_WithSingleCode_ShouldReturnOkWithSingleCode()
    {
        var codes = new List<string> { "03000000" };
        var expectedCodes = new List<CpvCode>
        {
            new()
            {
                Code = "03000000", DescriptionEn = "Agricultural products", DescriptionCy = "Cynhyrchion amaethyddol",
                Level = 1
            }
        };
        _mockRepository.Setup(x => x.GetByCodesAsync(codes)).ReturnsAsync(expectedCodes);

        var result = await _controller.GetByCodes(codes);

        var okResult = result.Should().BeOfType<ActionResult<List<CpvCode>>>().Subject;
        var okObjectResult = okResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedCodes = okObjectResult.Value.Should().BeOfType<List<CpvCode>>().Subject;
        returnedCodes.Should().HaveCount(1);
        returnedCodes[0].Code.Should().Be("03000000");
    }

    [Fact]
    public async Task GetByCodes_WhenRepositoryReturnsPartialResults_ShouldReturnOkWithAvailableCodes()
    {
        var requestedCodes = new List<string> { "03000000", "09000000", "99999999" };
        var expectedCodes = new List<CpvCode>
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
        _mockRepository.Setup(x => x.GetByCodesAsync(requestedCodes)).ReturnsAsync(expectedCodes);

        var result = await _controller.GetByCodes(requestedCodes);

        var okResult = result.Should().BeOfType<ActionResult<List<CpvCode>>>().Subject;
        var okObjectResult = okResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedCodes = okObjectResult.Value.Should().BeOfType<List<CpvCode>>().Subject;
        returnedCodes.Should().HaveCount(2);
        returnedCodes.Select(x => x.Code).Should().BeEquivalentTo(new[] { "03000000", "09000000" });
    }

    [Fact]
    public async Task GetHierarchy_WithValidCode_ShouldReturnOkWithHierarchy()
    {
        const string code = "03110000";
        var expectedHierarchy = new List<CpvCode>
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
        _mockRepository.Setup(x => x.GetHierarchyAsync(code)).ReturnsAsync(expectedHierarchy);

        var result = await _controller.GetHierarchy(code);

        var okResult = result.Should().BeOfType<ActionResult<List<CpvCode>>>().Subject;
        var okObjectResult = okResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var hierarchy = okObjectResult.Value.Should().BeOfType<List<CpvCode>>().Subject;
        hierarchy.Should().HaveCount(3);
        hierarchy[0].Level.Should().Be(1);
        hierarchy[1].Level.Should().Be(2);
        hierarchy[2].Level.Should().Be(3);
        hierarchy[2].Code.Should().Be(code);
    }

    [Fact]
    public async Task GetHierarchy_WhenCodeNotFound_ShouldReturnOkWithEmptyList()
    {
        const string nonExistentCode = "99999999";
        _mockRepository.Setup(x => x.GetHierarchyAsync(nonExistentCode)).ReturnsAsync(new List<CpvCode>());

        var result = await _controller.GetHierarchy(nonExistentCode);

        var okResult = result.Should().BeOfType<ActionResult<List<CpvCode>>>().Subject;
        var okObjectResult = okResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var hierarchy = okObjectResult.Value.Should().BeOfType<List<CpvCode>>().Subject;
        hierarchy.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHierarchy_ShouldPassCorrectCodeToRepository()
    {
        const string code = "03110000";
        _mockRepository.Setup(x => x.GetHierarchyAsync(code)).ReturnsAsync(new List<CpvCode>());

        await _controller.GetHierarchy(code);

        _mockRepository.Verify(x => x.GetHierarchyAsync(code), Times.Once);
    }

    [Theory]
    [InlineData("03000000")]
    [InlineData("09000000")]
    [InlineData("15000000")]
    public async Task GetChildren_ShouldPassCorrectParentCodeToRepository(string parentCode)
    {
        _mockRepository.Setup(x => x.GetChildrenAsync(parentCode, It.IsAny<Culture>()))
            .ReturnsAsync(new List<CpvCode>());

        await _controller.GetChildren(parentCode);

        _mockRepository.Verify(x => x.GetChildrenAsync(parentCode, Culture.English), Times.Once);
    }

    [Theory]
    [InlineData("agriculture")]
    [InlineData("petroleum")]
    [InlineData("construction")]
    public async Task Search_ShouldPassCorrectQueryToRepository(string query)
    {
        _mockRepository.Setup(x => x.SearchAsync(query, It.IsAny<Culture>())).ReturnsAsync(new List<CpvCode>());

        await _controller.Search(query);

        _mockRepository.Verify(x => x.SearchAsync(query, Culture.English), Times.Once);
    }
}