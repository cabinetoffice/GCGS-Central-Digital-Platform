using CO.CDP.RegisterOfCommercialTools.WebApi.Controllers;
using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using CO.CDP.RegisterOfCommercialTools.WebApi.Services;
using CO.CDP.WebApi.Foundation;
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
            Keywords = ["IT", "services"],
            Status = "Active",
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
        };

        _mockSearchService.Setup(x => x.Search(request)).ReturnsAsync(ApiResult<SearchResponse>.Success(expectedResponse));

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
            Keywords = ["nonexistent"],
            PageNumber = 1
        };

        var expectedResponse = new SearchResponse
        {
            Results = new List<SearchResultDto>(),
            TotalCount = 0,
            PageNumber = 1,
        };
        _mockSearchService.Setup(x => x.Search(request)).ReturnsAsync(ApiResult<SearchResponse>.Success(expectedResponse));

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

        var act = async () => await _controller.Search(request);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Service error");
        _mockSearchService.Verify(x => x.Search(request), Times.Once);
    }


    [Fact]
    public async Task Search_WithComplexRequest_ShouldPassAllParametersToService()
    {
        var request = new SearchRequestDto
        {
            Keywords = ["complex", "search"],
            Status = "Active",
            SubmissionDeadlineFrom = new DateTime(2025, 1, 1),
            SubmissionDeadlineTo = new DateTime(2025, 12, 31),
            ContractStartDate = new DateTime(2025, 6, 1),
            ContractEndDate = new DateTime(2026, 12, 31),
            MinFees = 100.50m,
            MaxFees = 999.99m,
            AwardMethod = ["With competition"],
            SortBy = "title",
            PageNumber = 3,
        };

        var expectedResponse = new SearchResponse
        {
            Results = new List<SearchResultDto>
            {
                new() { Id = "complex-001", Title = "Complex Framework" }
            },
            TotalCount = 50,
            PageNumber = 3,
        };

        _mockSearchService.Setup(x => x.Search(It.Is<SearchRequestDto>(r =>
            r.Keywords == request.Keywords &&
            r.Status == request.Status &&
            r.SubmissionDeadlineFrom == request.SubmissionDeadlineFrom &&
            r.SubmissionDeadlineTo == request.SubmissionDeadlineTo &&
            r.ContractStartDate == request.ContractStartDate &&
            r.ContractEndDate == request.ContractEndDate &&
            r.MinFees == request.MinFees &&
            r.MaxFees == request.MaxFees &&
            r.AwardMethod == request.AwardMethod &&
            r.SortBy == request.SortBy &&
            r.PageNumber == request.PageNumber)))
            .ReturnsAsync(ApiResult<SearchResponse>.Success(expectedResponse));

        var result = await _controller.Search(request);

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
        _mockSearchService.Verify(x => x.Search(It.IsAny<SearchRequestDto>()), Times.Once);
    }
}