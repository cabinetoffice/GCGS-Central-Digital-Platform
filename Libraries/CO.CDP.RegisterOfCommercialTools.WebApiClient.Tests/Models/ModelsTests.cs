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
        request.Keywords.Should().BeNull();
        request.SearchMode.Should().Be(KeywordSearchMode.Any);
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
            Keywords = ["test"],
            Status = "Active",
            SortBy = "Title",
            AwardMethod = ["With competition"],
            MinFees = 100m,
            MaxFees = 1000m,
            PageNumber = 2,
            SubmissionDeadlineFrom = testDate,
            SubmissionDeadlineTo = testDate.AddDays(30),
            ContractStartDateFrom = testDate.AddDays(60),
            ContractStartDateTo = testDate.AddDays(90),
            ContractEndDateFrom = testDate.AddDays(365),
            ContractEndDateTo = testDate.AddDays(730)
        };

        request.Keywords.Should().ContainSingle().Which.Should().Be("test");
        request.Status.Should().Be("Active");
        request.SortBy.Should().Be("Title");
        request.AwardMethod.Should().ContainSingle().Which.Should().Be("With competition");
        request.MinFees.Should().Be(100m);
        request.MaxFees.Should().Be(1000m);
        request.PageNumber.Should().Be(2);
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
            PublishedDate = testDate,
            SubmissionDeadline = testDate.AddDays(30).ToString("dd MMMM yyyy"),
            Status = CommercialToolStatus.Active,
            MaximumFee = "10%",
            AwardMethod = "With competition",
            OtherContractingAuthorityCanUse = "Yes"
        };

        result.Id.Should().Be("test-id");
        result.Title.Should().Be("Test Tool");
        result.Description.Should().Be("Test Description");
        result.PublishedDate.Should().Be(testDate);
        result.SubmissionDeadline.Should().Be(testDate.AddDays(30).ToString("dd MMMM yyyy"));
        result.Status.Should().Be(CommercialToolStatus.Active);
        result.MaximumFee.Should().Be("10%");
        result.AwardMethod.Should().Be("With competition");
        result.OtherContractingAuthorityCanUse.Should().Be("Yes");
    }

    [Fact]
    public void SearchResultDto_AllowsNullableProperties()
    {
        var result = new SearchResultDto
        {
            Id = "test-id",
            Title = "Test Tool",
            Description = "Test Description",
            PublishedDate = DateTime.UtcNow,
            Status = CommercialToolStatus.Active,
            MaximumFee = "Unknown",
            AwardMethod = "With competition",
            SubmissionDeadline = "Unknown",
            OtherContractingAuthorityCanUse = "Unknown"
        };

        result.SubmissionDeadline.Should().Be("Unknown");
        result.OtherContractingAuthorityCanUse.Should().Be("Unknown");
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
                PublishedDate = DateTime.UtcNow,
                Status = CommercialToolStatus.Active,
                MaximumFee = "10%",
                AwardMethod = "With competition"
            },
            new SearchResultDto
            {
                Id = "2",
                Title = "Tool 2",
                Description = "Description 2",
                PublishedDate = DateTime.UtcNow,
                Status = CommercialToolStatus.Expired,
                MaximumFee = "20%",
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
    [InlineData(CommercialToolStatus.Expired)]
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
        CommercialToolStatus.Expired.ToString().Should().Be("Expired");
        CommercialToolStatus.Awarded.ToString().Should().Be("Awarded");
        CommercialToolStatus.Upcoming.ToString().Should().Be("Upcoming");
        CommercialToolStatus.Unknown.ToString().Should().Be("Unknown");
    }
}