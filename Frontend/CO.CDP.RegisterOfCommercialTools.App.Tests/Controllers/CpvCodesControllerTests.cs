using CO.CDP.RegisterOfCommercialTools.App.Controllers;
using CO.CDP.RegisterOfCommercialTools.App.Models;
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
    public async Task GetTreeFragment_ValidRequest_ReturnsPartialView()
    {
        var selectedCodes = new[] { "03000000" };
        const string expandedCode = "03000000";
        var rootCodes = new List<CpvCodeDto>
        {
            new() { Code = "03000000", DescriptionEn = "Agricultural products", DescriptionCy = "Cynhyrchion amaethyddol", Level = 1 }
        };
        var children = new List<CpvCodeDto>
        {
            new() { Code = "03100000", DescriptionEn = "Live animals", DescriptionCy = "Anifeiliaid byw", Level = 2, ParentCode = "03000000" }
        };

        _mockService.Setup(s => s.GetRootCpvCodesAsync()).ReturnsAsync(rootCodes);
        _mockService.Setup(s => s.GetChildrenAsync(expandedCode)).ReturnsAsync(children);

        var result = await _controller.GetTreeFragment(selectedCodes, expandedCode);

        result.Should().BeOfType<PartialViewResult>();
        var partialView = result as PartialViewResult;
        partialView!.ViewName.Should().Be("_CpvTreeFragment");
        partialView.Model.Should().BeOfType<CpvSelectionViewModel>();

        var model = partialView.Model as CpvSelectionViewModel;
        model!.SelectedCodes.Should().BeEquivalentTo(selectedCodes);
        model.ExpandedCode.Should().Be(expandedCode);
    }

    [Fact]
    public async Task GetTreeFragment_ServiceThrowsException_ReturnsErrorModel()
    {
        _mockService.Setup(s => s.GetRootCpvCodesAsync()).ThrowsAsync(new Exception("Service error"));

        var result = await _controller.GetTreeFragment();

        result.Should().BeOfType<PartialViewResult>();
        var partialView = result as PartialViewResult;
        var model = partialView!.Model as CpvSelectionViewModel;
        model!.ErrorMessage.Should().Be("There is a problem loading CPV codes. Try refreshing the page.");
    }

    [Fact]
    public async Task GetSearchFragment_EmptyQuery_ReturnsEmptyModel()
    {
        var result = await _controller.GetSearchFragment("");

        result.Should().BeOfType<PartialViewResult>();
        var partialView = result as PartialViewResult;
        partialView!.ViewName.Should().Be("_CpvSearchFragment");

        var model = partialView.Model as CpvSelectionViewModel;
        model!.SearchResults.Should().BeEmpty();
        model.SearchQuery.Should().Be("");
    }

    [Fact]
    public async Task GetSearchFragment_ShortQuery_ReturnsEmptyModel()
    {
        var result = await _controller.GetSearchFragment("a");

        result.Should().BeOfType<PartialViewResult>();
        var partialView = result as PartialViewResult;
        var model = partialView!.Model as CpvSelectionViewModel;
        model!.SearchResults.Should().BeEmpty();
        model.SearchQuery.Should().Be("a");
    }

    [Fact]
    public async Task GetSearchFragment_ValidQuery_ReturnsSearchResults()
    {
        const string query = "agricultural";
        var selectedCodes = new[] { "03000000" };
        var searchResults = new List<CpvCodeDto>
        {
            new() { Code = "03000000", DescriptionEn = "Agricultural products", DescriptionCy = "Cynhyrchion amaethyddol", Level = 1 }
        };

        _mockService.Setup(s => s.SearchAsync(query)).ReturnsAsync(searchResults);

        var result = await _controller.GetSearchFragment(query, selectedCodes);

        result.Should().BeOfType<PartialViewResult>();
        var partialView = result as PartialViewResult;
        var model = partialView!.Model as CpvSelectionViewModel;
        model!.SearchResults.Should().BeEquivalentTo(searchResults);
        model.SelectedCodes.Should().BeEquivalentTo(selectedCodes);
        model.SearchQuery.Should().Be(query);
    }

    [Fact]
    public async Task GetSearchFragment_ServiceThrowsException_ReturnsErrorModel()
    {
        const string query = "agricultural";
        _mockService.Setup(s => s.SearchAsync(query)).ThrowsAsync(new Exception("Service error"));

        var result = await _controller.GetSearchFragment(query);

        result.Should().BeOfType<PartialViewResult>();
        var partialView = result as PartialViewResult;
        var model = partialView!.Model as CpvSelectionViewModel;
        model!.ErrorMessage.Should().Be("There is a problem with search. Try again or browse the categories below.");
    }

    [Fact]
    public async Task UpdateSelectionFragment_ValidCodes_ReturnsPartialView()
    {
        var selectedCodes = new[] { "03000000", "03100000" };
        var allCodes = new List<CpvCodeDto>
        {
            new() { Code = "03000000", DescriptionEn = "Agricultural products", DescriptionCy = "Cynhyrchion amaethyddol", Level = 1 },
            new() { Code = "03100000", DescriptionEn = "Live animals", DescriptionCy = "Anifeiliaid byw", Level = 2 }
        };

        _mockService.Setup(s => s.GetByCodesAsync(It.IsAny<List<string>>())).ReturnsAsync(allCodes);

        var result = await _controller.UpdateSelectionFragment(selectedCodes);

        result.Should().BeOfType<PartialViewResult>();
        var partialView = result as PartialViewResult;
        partialView!.ViewName.Should().Be("_CpvSelectionFragment");

        var model = partialView.Model as CpvSelectionViewModel;
        model!.SelectedCodes.Should().BeEquivalentTo(selectedCodes);
    }

    [Fact]
    public async Task UpdateSelectionFragment_ServiceThrowsException_ReturnsErrorModel()
    {
        var selectedCodes = new[] { "03000000" };
        _mockService.Setup(s => s.GetByCodesAsync(It.IsAny<List<string>>())).ThrowsAsync(new Exception("Service error"));

        var result = await _controller.UpdateSelectionFragment(selectedCodes);

        result.Should().BeOfType<PartialViewResult>();
        var partialView = result as PartialViewResult;
        var model = partialView!.Model as CpvSelectionViewModel;
        model!.ErrorMessage.Should().Be("There is a problem updating your selection. Try again.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetChildrenFragment_InvalidParentCode_ReturnsBadRequest(string parentCode)
    {
        var result = await _controller.GetChildrenFragment(parentCode);

        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().Be("Parent code is required");
    }

    [Fact]
    public async Task GetChildrenFragment_ValidParentCode_ReturnsPartialView()
    {
        const string parentCode = "03000000";
        var selectedCodes = new[] { "03100000" };
        var children = new List<CpvCodeDto>
        {
            new() { Code = "03100000", DescriptionEn = "Live animals", DescriptionCy = "Anifeiliaid byw", Level = 2, ParentCode = parentCode }
        };

        _mockService.Setup(s => s.GetChildrenAsync(parentCode)).ReturnsAsync(children);

        var result = await _controller.GetChildrenFragment(parentCode, selectedCodes);

        result.Should().BeOfType<PartialViewResult>();
        var partialView = result as PartialViewResult;
        partialView!.ViewName.Should().Be("_CpvChildrenFragment");

        var model = partialView.Model as CpvSelectionViewModel;
        model!.RootCodes.Should().BeEquivalentTo(children);
        model.SelectedCodes.Should().BeEquivalentTo(selectedCodes);
    }

    [Fact]
    public async Task GetChildrenFragment_ServiceThrowsException_ReturnsErrorModel()
    {
        const string parentCode = "03000000";
        _mockService.Setup(s => s.GetChildrenAsync(parentCode)).ThrowsAsync(new Exception("Service error"));

        var result = await _controller.GetChildrenFragment(parentCode);

        result.Should().BeOfType<PartialViewResult>();
        var partialView = result as PartialViewResult;
        var model = partialView!.Model as CpvSelectionViewModel;
        model!.ErrorMessage.Should().Be("There is a problem loading the subcategories. Try again.");
    }
}