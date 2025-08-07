using CO.CDP.RegisterOfCommercialTools.WebApiClient.Models;
using FluentAssertions;

namespace CO.CDP.RegisterOfCommercialTools.WebApiClient.Tests.Models;

public class SearchRequestDtoTests
{
    [Fact]
    public void SearchRequestDto_HasCorrectDefaultValues()
    {
        var request = new SearchRequestDto();

        request.PageNumber.Should().Be(1);
        request.PageSize.Should().Be(10);
        request.Keyword.Should().BeNull();
        request.Status.Should().BeNull();
        request.SortBy.Should().BeNull();
        request.AwardMethod.Should().BeNull();
        request.MinFees.Should().BeNull();
        request.MaxFees.Should().BeNull();
        request.SubmissionDeadlineFrom.Should().BeNull();
        request.SubmissionDeadlineTo.Should().BeNull();
        request.ContractStartDateFrom.Should().BeNull();
        request.ContractStartDateTo.Should().BeNull();
        request.ContractEndDateFrom.Should().BeNull();
        request.ContractEndDateTo.Should().BeNull();
    }

    [Fact]
    public void SearchRequestDto_CanSetAllProperties()
    {
        var testDate = DateTime.UtcNow;
        var request = new SearchRequestDto
        {
            Keyword = "test",
            Status = "Active",
            SortBy = "Title",
            AwardMethod = "Open",
            MinFees = 100m,
            MaxFees = 1000m,
            PageNumber = 2,
            PageSize = 20,
            SubmissionDeadlineFrom = testDate,
            SubmissionDeadlineTo = testDate.AddDays(30),
            ContractStartDateFrom = testDate.AddDays(60),
            ContractStartDateTo = testDate.AddDays(90),
            ContractEndDateFrom = testDate.AddDays(365),
            ContractEndDateTo = testDate.AddDays(730)
        };

        request.Keyword.Should().Be("test");
        request.Status.Should().Be("Active");
        request.SortBy.Should().Be("Title");
        request.AwardMethod.Should().Be("Open");
        request.MinFees.Should().Be(100m);
        request.MaxFees.Should().Be(1000m);
        request.PageNumber.Should().Be(2);
        request.PageSize.Should().Be(20);
        request.SubmissionDeadlineFrom.Should().Be(testDate);
        request.SubmissionDeadlineTo.Should().Be(testDate.AddDays(30));
        request.ContractStartDateFrom.Should().Be(testDate.AddDays(60));
        request.ContractStartDateTo.Should().Be(testDate.AddDays(90));
        request.ContractEndDateFrom.Should().Be(testDate.AddDays(365));
        request.ContractEndDateTo.Should().Be(testDate.AddDays(730));
    }
}

public class SearchResultDtoTests
{
    [Fact]
    public void SearchResultDto_CanBeCreated()
    {
        var testDate = DateTime.UtcNow;
        var result = new SearchResultDto
        {
            Id = "test-id",
            Title = "Test Tool",
            Description = "Test Description",
            Link = "https://example.com",
            PublishedDate = testDate,
            SubmissionDeadline = testDate.AddDays(30),
            Status = CommercialToolStatus.Active,
            Fees = 1000m,
            AwardMethod = "Open",
            ReservedParticipation = "Yes"
        };

        result.Id.Should().Be("test-id");
        result.Title.Should().Be("Test Tool");
        result.Description.Should().Be("Test Description");
        result.Link.Should().Be("https://example.com");
        result.PublishedDate.Should().Be(testDate);
        result.SubmissionDeadline.Should().Be(testDate.AddDays(30));
        result.Status.Should().Be(CommercialToolStatus.Active);
        result.Fees.Should().Be(1000m);
        result.AwardMethod.Should().Be("Open");
        result.ReservedParticipation.Should().Be("Yes");
    }

    [Fact]
    public void SearchResultDto_AllowsNullableProperties()
    {
        var result = new SearchResultDto
        {
            Id = "test-id",
            Title = "Test Tool",
            Description = "Test Description",
            Link = "https://example.com",
            PublishedDate = DateTime.UtcNow,
            Status = CommercialToolStatus.Active,
            Fees = 0m,
            AwardMethod = "Open",
            SubmissionDeadline = null,
            ReservedParticipation = null
        };

        result.SubmissionDeadline.Should().BeNull();
        result.ReservedParticipation.Should().BeNull();
    }
}

public class SearchResponseTests
{
    [Fact]
    public void SearchResponse_HasCorrectDefaultValues()
    {
        var response = new SearchResponse();

        response.Results.Should().NotBeNull();
        response.Results.Should().BeEmpty();
        response.TotalCount.Should().Be(0);
        response.PageNumber.Should().Be(0);
        response.PageSize.Should().Be(0);
    }

    [Fact]
    public void SearchResponse_CanSetAllProperties()
    {
        var results = new[]
        {
            new SearchResultDto
            {
                Id = "1",
                Title = "Tool 1",
                Description = "Description 1",
                Link = "https://example.com/1",
                PublishedDate = DateTime.UtcNow,
                Status = CommercialToolStatus.Active,
                Fees = 1000m,
                AwardMethod = "Open"
            },
            new SearchResultDto
            {
                Id = "2",
                Title = "Tool 2",
                Description = "Description 2",
                Link = "https://example.com/2",
                PublishedDate = DateTime.UtcNow,
                Status = CommercialToolStatus.Closed,
                Fees = 2000m,
                AwardMethod = "Framework"
            }
        };

        var response = new SearchResponse
        {
            Results = results,
            TotalCount = 25,
            PageNumber = 2,
            PageSize = 10
        };

        response.Results.Should().HaveCount(2);
        response.Results.Should().BeEquivalentTo(results);
        response.TotalCount.Should().Be(25);
        response.PageNumber.Should().Be(2);
        response.PageSize.Should().Be(10);
    }
}

public class CommercialToolStatusTests
{
    [Theory]
    [InlineData(CommercialToolStatus.Unknown)]
    [InlineData(CommercialToolStatus.Active)]
    [InlineData(CommercialToolStatus.Closed)]
    [InlineData(CommercialToolStatus.Awarded)]
    [InlineData(CommercialToolStatus.Upcoming)]
    public void CommercialToolStatus_HasAllExpectedValues(CommercialToolStatus status)
    {
        Enum.IsDefined(typeof(CommercialToolStatus), status).Should().BeTrue();
    }

    [Fact]
    public void CommercialToolStatus_HasCorrectNumberOfValues()
    {
        var values = Enum.GetValues<CommercialToolStatus>();
        values.Should().HaveCount(5);
    }

    [Fact]
    public void CommercialToolStatus_CanConvertToString()
    {
        CommercialToolStatus.Active.ToString().Should().Be("Active");
        CommercialToolStatus.Closed.ToString().Should().Be("Closed");
        CommercialToolStatus.Awarded.ToString().Should().Be("Awarded");
        CommercialToolStatus.Upcoming.ToString().Should().Be("Upcoming");
        CommercialToolStatus.Unknown.ToString().Should().Be("Unknown");
    }
}