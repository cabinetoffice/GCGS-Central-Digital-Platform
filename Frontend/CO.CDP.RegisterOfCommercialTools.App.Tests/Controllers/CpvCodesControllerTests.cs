using CO.CDP.RegisterOfCommercialTools.App.Controllers;
using CO.CDP.RegisterOfCommercialTools.App.Services;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CO.CDP.RegisterOfCommercialTools.App.Tests.Controllers;

public class CpvCodesControllerTests
{
    private readonly Mock<ICpvCodeService> _mockService;
    private readonly CpvCodesController _controller;

    public CpvCodesControllerTests()
    {
        _mockService = new Mock<ICpvCodeService>();
        _controller = new CpvCodesController(_mockService.Object);
    }

    [Fact]
    public async Task GetRootCpvCodes_ReturnsOkWithData()
    {
        var dtos = new List<CpvCodeDto>
        {
            new()
            {
                Code = "03000000", DescriptionEn = "Agricultural products", DescriptionCy = "Cynhyrchion amaethyddol",
                Level = 1
            }
        };
        _mockService.Setup(s => s.GetRootCpvCodesAsync()).ReturnsAsync(dtos);

        var result = await _controller.GetRootCpvCodes();

        result.Should().BeOfType<OkObjectResult>();
        var ok = result as OkObjectResult;
        ok!.Value.Should().BeEquivalentTo(dtos);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetChildren_InvalidParent_ReturnsBadRequest(string parentCode)
    {
        var result = await _controller.GetChildren(parentCode);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetChildren_ValidParent_ReturnsOk()
    {
        const string parent = "03000000";
        var dtos = new List<CpvCodeDto>
        {
            new()
            {
                Code = "03100000", Level = 2, ParentCode = parent, DescriptionEn = "Live animals",
                DescriptionCy = "Anifeiliaid byw"
            }
        };
        _mockService.Setup(s => s.GetChildrenAsync(parent)).ReturnsAsync(dtos);

        var result = await _controller.GetChildren(parent);

        result.Should().BeOfType<OkObjectResult>();
        var ok = result as OkObjectResult;
        ok!.Value.Should().BeEquivalentTo(dtos);
    }

    [Fact]
    public async Task Search_InvalidQuery_ReturnsBadRequest()
    {
        var result = await _controller.Search("");
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Search_ValidQuery_ReturnsOk()
    {
        const string q = "agricultural";
        var dtos = new List<CpvCodeDto>
        {
            new()
            {
                Code = "03000000", DescriptionEn = "Agricultural products", DescriptionCy = "Cynhyrchion amaethyddol",
                Level = 1
            }
        };
        _mockService.Setup(s => s.SearchAsync(q)).ReturnsAsync(dtos);

        var result = await _controller.Search(q);

        result.Should().BeOfType<OkObjectResult>();
        var ok = result as OkObjectResult;
        ok!.Value.Should().BeEquivalentTo(dtos);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetByCode_Invalid_ReturnsBadRequest(string code)
    {
        var result = await _controller.GetByCode(code);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetByCode_NotFound_ReturnsNotFound()
    {
        const string code = "99999999";
        _mockService.Setup(s => s.GetByCodeAsync(code)).ReturnsAsync((CpvCodeDto?)null);

        var result = await _controller.GetByCode(code);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetByCode_Found_ReturnsOk()
    {
        const string code = "03000000";
        var dto = new CpvCodeDto
        {
            Code = code, DescriptionEn = "Agricultural products", DescriptionCy = "Cynhyrchion amaethyddol", Level = 1
        };
        _mockService.Setup(s => s.GetByCodeAsync(code)).ReturnsAsync(dto);

        var result = await _controller.GetByCode(code);

        result.Should().BeOfType<OkObjectResult>();
        var ok = result as OkObjectResult;
        ok!.Value.Should().BeEquivalentTo(dto);
    }
}