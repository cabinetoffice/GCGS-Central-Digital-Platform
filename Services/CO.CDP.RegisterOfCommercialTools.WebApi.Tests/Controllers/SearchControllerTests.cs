using CO.CDP.RegisterOfCommercialTools.WebApi.Controllers;
using CO.CDP.RegisterOfCommercialTools.WebApi.Models;
using CO.CDP.RegisterOfCommercialTools.WebApi.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Tests.Controllers;

public class SearchControllerTests
{
    private readonly Mock<ISearchService> _mockSearchService;
    private readonly SearchController _controller;

    public SearchControllerTests()
    {
        _mockSearchService = new Mock<ISearchService>();
        _controller = new SearchController(_mockSearchService.Object);
    }

    [Fact]
    public async Task Search_ShouldReturnOkWithResults()
    {
        var request = new SearchRequestDto
        {
            Keyword = "IT services",
            Status = "Active",
            PageSize = 10,
            PageNumber = 1
        };

        var expectedResponse = new SearchResponse
        {
            Results = new List<SearchResultDto>
            {
                new() { Id = "003033-2025", Title = "Test Framework 1" },
                new() { Id = "004044-2025", Title = "Test Framework 2" }
            },
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };

        _mockSearchService.Setup(x => x.Search(request)).ReturnsAsync(expectedResponse);

        var result = await _controller.Search(request);

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        _mockSearchService.Verify(x => x.Search(request), Times.Once);
    }

    [Fact]
    public async Task Search_WhenNoResults_ShouldReturnOkWithEmptyList()
    {
        var request = new SearchRequestDto
        {
            Keyword = "nonexistent",
            PageSize = 10,
            PageNumber = 1
        };

        var expectedResponse = new SearchResponse
        {
            Results = new List<SearchResultDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };
        _mockSearchService.Setup(x => x.Search(request)).ReturnsAsync(expectedResponse);

        var result = await _controller.Search(request);

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        _mockSearchService.Verify(x => x.Search(request), Times.Once);
    }

    [Fact]
    public async Task Search_WhenServiceThrows_ShouldPropagateException()
    {
        var request = new SearchRequestDto();
        var expectedException = new InvalidOperationException("Service error");
        _mockSearchService.Setup(x => x.Search(request)).ThrowsAsync(expectedException);

        var action = async () => await _controller.Search(request);
        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("Service error");
        _mockSearchService.Verify(x => x.Search(request), Times.Once);
    }

    [Fact]
    public async Task GetById_WhenResultExists_ShouldReturnOkWithResult()
    {
        var id = "003033-2025";
        var expectedResult = new SearchResultDto
        {
            Id = id,
            Title = "Test Framework",
            Description = "Test Description"
        };

        _mockSearchService.Setup(x => x.GetById(id)).ReturnsAsync(expectedResult);

        var result = await _controller.GetById(id);

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResult);
        _mockSearchService.Verify(x => x.GetById(id), Times.Once);
    }

    [Fact]
    public async Task GetById_WhenResultNotFound_ShouldReturnNotFound()
    {
        var id = "nonexistent-id";
        _mockSearchService.Setup(x => x.GetById(id)).ReturnsAsync((SearchResultDto?)null);

        var result = await _controller.GetById(id);

        result.Result.Should().BeOfType<NotFoundResult>();
        _mockSearchService.Verify(x => x.GetById(id), Times.Once);
    }

    [Fact]
    public async Task GetById_WhenServiceThrows_ShouldPropagateException()
    {
        var id = "test-id";
        var expectedException = new InvalidOperationException("Service error");
        _mockSearchService.Setup(x => x.GetById(id)).ThrowsAsync(expectedException);

        var action = async () => await _controller.GetById(id);
        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("Service error");
        _mockSearchService.Verify(x => x.GetById(id), Times.Once);
    }

    [Fact]
    public async Task Search_WithComplexRequest_ShouldPassAllParametersToService()
    {
        var request = new SearchRequestDto
        {
            Keyword = "complex search",
            Status = "Active",
            SubmissionDeadlineFrom = new DateTime(2025, 1, 1),
            SubmissionDeadlineTo = new DateTime(2025, 12, 31),
            ContractStartDateFrom = new DateTime(2025, 6, 1),
            ContractStartDateTo = new DateTime(2025, 6, 30),
            ContractEndDateFrom = new DateTime(2026, 1, 1),
            ContractEndDateTo = new DateTime(2026, 12, 31),
            MinFees = 100.50m,
            MaxFees = 999.99m,
            AwardMethod = "Competitive",
            SortBy = "title",
            PageNumber = 3,
            PageSize = 25
        };

        var expectedResponse = new SearchResponse
        {
            Results = new List<SearchResultDto>
            {
                new() { Id = "complex-001", Title = "Complex Framework" }
            },
            TotalCount = 50,
            PageNumber = 3,
            PageSize = 25
        };

        _mockSearchService.Setup(x => x.Search(It.Is<SearchRequestDto>(r =>
            r.Keyword == request.Keyword &&
            r.Status == request.Status &&
            r.SubmissionDeadlineFrom == request.SubmissionDeadlineFrom &&
            r.SubmissionDeadlineTo == request.SubmissionDeadlineTo &&
            r.ContractStartDateFrom == request.ContractStartDateFrom &&
            r.ContractStartDateTo == request.ContractStartDateTo &&
            r.ContractEndDateFrom == request.ContractEndDateFrom &&
            r.ContractEndDateTo == request.ContractEndDateTo &&
            r.MinFees == request.MinFees &&
            r.MaxFees == request.MaxFees &&
            r.AwardMethod == request.AwardMethod &&
            r.SortBy == request.SortBy &&
            r.PageNumber == request.PageNumber &&
            r.PageSize == request.PageSize)))
            .ReturnsAsync(expectedResponse);

        var result = await _controller.Search(request);

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        _mockSearchService.Verify(x => x.Search(It.IsAny<SearchRequestDto>()), Times.Once);
    }
}