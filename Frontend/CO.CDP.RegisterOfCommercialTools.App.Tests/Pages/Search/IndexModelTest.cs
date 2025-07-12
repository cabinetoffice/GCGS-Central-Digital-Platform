using CO.CDP.RegisterOfCommercialTools.App.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.ComponentModel.DataAnnotations;
using CO.CDP.RegisterOfCommercialTools.App.Pages;

namespace CO.CDP.RegisterOfCommercialTools.App.Tests.Pages.Search;

public class IndexModelTest
{
    private readonly Mock<ISearchService> _mockSearchService;
    private readonly IndexModel _model;

    public IndexModelTest()
    {
        _mockSearchService = new Mock<ISearchService>();
        _model = new IndexModel(_mockSearchService.Object);
    }

    [Fact]
    public async Task OnGet_ShouldPopulateSearchResults()
    {
        var searchResults = new List<SearchResult>
        {
            new(Guid.NewGuid(), "Test Result", "Test Caption", "/", "Test Tool", SearchResultStatus.Active, "1%", "Yes", "2025-01-01",
                "2025-01-01 to 2025-12-31", "Direct Award")
        };
        _mockSearchService.Setup(s => s.SearchAsync(It.IsAny<SearchModel>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((searchResults, 1));

        await _model.OnGetAsync();

        _model.SearchResults.Should().BeEquivalentTo(searchResults);
        _model.Pagination.Should().NotBeNull();
        _model.Pagination?.TotalItems.Should().Be(1);
    }

    [Fact]
    public void SearchParams_ShouldBeInitialized()
    {
        _model.SearchParams.Should().NotBeNull();
        _model.SearchParams.Keywords.Should().BeNull();
        _model.SearchParams.SortOrder.Should().BeNull();
        _model.SearchParams.FrameworkOptions.Should().BeNull();
        _model.SearchParams.DynamicMarketOptions.Should().BeNull();
        _model.SearchParams.AwardMethod.Should().BeNull();
        _model.SearchParams.Status.Should().BeEmpty();
        _model.SearchParams.ContractingAuthorityUsage.Should().BeNull();
        _model.SearchParams.FeeFrom.Should().BeNull();
        _model.SearchParams.FeeTo.Should().BeNull();
        _model.SearchParams.NoFees.Should().BeNull();
        _model.SearchParams.SubmissionDeadlineFrom.Should().BeNull();
        _model.SearchParams.SubmissionDeadlineTo.Should().BeNull();
        _model.SearchParams.ContractStartDateFrom.Should().BeNull();
        _model.SearchParams.ContractStartDateTo.Should().BeNull();
        _model.SearchParams.ContractEndDateFrom.Should().BeNull();
        _model.SearchParams.ContractEndDateTo.Should().BeNull();
    }

    [Fact]
    public async Task OnGet_WithSearchParams_ShouldRetainSearchParams()
    {
        var searchParams = new SearchModel
        {
            Keywords = "test",
            SortOrder = "a-z",
            FrameworkOptions = "open",
            DynamicMarketOptions = "utilities-only",
            AwardMethod = "direct-award",
            Status = ["upcoming", "active-buyers"],
            ContractingAuthorityUsage = "yes",
            FeeFrom = 0,
            FeeTo = 100,
            NoFees = "true",
            SubmissionDeadlineFrom = new DateOnly(2025, 1, 1),
            SubmissionDeadlineTo = new DateOnly(2025, 1, 31),
            ContractStartDateFrom = new DateOnly(2025, 2, 1),
            ContractStartDateTo = new DateOnly(2025, 2, 28),
            ContractEndDateFrom = new DateOnly(2026, 1, 1),
            ContractEndDateTo = new DateOnly(2026, 1, 31)
        };
        _model.SearchParams = searchParams;

        _mockSearchService.Setup(s => s.SearchAsync(searchParams, 1, 10))
            .ReturnsAsync((new List<SearchResult>(), 0));

        await _model.OnGetAsync();

        _model.SearchParams.Should().Be(searchParams);
    }

    [Fact]
    public void OnPostReset_ShouldClearModelStateAndRedirect()
    {
        _model.ModelState.AddModelError("test", "test error");

        var result = _model.OnPostReset();

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().BeNull();
        redirectResult.RouteValues.Should().BeNull();
    }

    [Fact]
    public void DateRangeValidation_WhenToDateIsBeforeFromDate_ShouldHaveValidationError()
    {
        var searchParams = new SearchModel
        {
            SubmissionDeadlineFrom = new DateOnly(2025, 2, 1),
            SubmissionDeadlineTo = new DateOnly(2025, 1, 1),
            ContractStartDateFrom = new DateOnly(2025, 2, 1),
            ContractStartDateTo = new DateOnly(2025, 1, 1),
            ContractEndDateFrom = new DateOnly(2025, 2, 1),
            ContractEndDateTo = new DateOnly(2025, 1, 1)
        };

        var validationContext = new ValidationContext(searchParams);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(searchParams, validationContext, validationResults, true);

        isValid.Should().BeFalse();
        validationResults.Should().HaveCount(3);
        validationResults.Select(r => r.ErrorMessage).Should().AllBe("To date must be after from date");
    }

    [Fact]
    public void DateRangeValidation_WhenToDateIsAfterFromDate_ShouldBeValid()
    {
        var searchParams = new SearchModel
        {
            SubmissionDeadlineFrom = new DateOnly(2025, 1, 1),
            SubmissionDeadlineTo = new DateOnly(2025, 2, 1),
            ContractStartDateFrom = new DateOnly(2025, 1, 1),
            ContractStartDateTo = new DateOnly(2025, 2, 1),
            ContractEndDateFrom = new DateOnly(2025, 1, 1),
            ContractEndDateTo = new DateOnly(2025, 2, 1)
        };

        var validationContext = new ValidationContext(searchParams);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(searchParams, validationContext, validationResults, true);

        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void DateRangeValidation_WhenToDateIsSameAsFromDate_ShouldBeValid()
    {
        var searchParams = new SearchModel
        {
            SubmissionDeadlineFrom = new DateOnly(2025, 1, 1),
            SubmissionDeadlineTo = new DateOnly(2025, 1, 1),
            ContractStartDateFrom = new DateOnly(2025, 1, 1),
            ContractStartDateTo = new DateOnly(2025, 1, 1),
            ContractEndDateFrom = new DateOnly(2025, 1, 1),
            ContractEndDateTo = new DateOnly(2025, 1, 1)
        };

        var validationContext = new ValidationContext(searchParams);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(searchParams, validationContext, validationResults, true);

        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void DateRangeValidation_WhenDatesArePartiallyPopulated_ShouldBeValid()
    {
        var searchParams = new SearchModel
        {
            SubmissionDeadlineFrom = new DateOnly(2025, 1, 1),
            SubmissionDeadlineTo = null,
            ContractStartDateFrom = null,
            ContractStartDateTo = new DateOnly(2025, 1, 1),
            ContractEndDateFrom = new DateOnly(2025, 1, 1),
            ContractEndDateTo = null
        };

        var validationContext = new ValidationContext(searchParams);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(searchParams, validationContext, validationResults, true);

        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void DateRangeValidation_WhenOnlyOneFieldHasError_ShouldOnlyReportThatError()
    {
        var searchParams = new SearchModel
        {
            SubmissionDeadlineFrom = new DateOnly(2025, 2, 1),
            SubmissionDeadlineTo = new DateOnly(2025, 1, 1),
            ContractStartDateFrom = new DateOnly(2025, 1, 1),
            ContractStartDateTo = new DateOnly(2025, 2, 1),
            ContractEndDateFrom = new DateOnly(2025, 1, 1),
            ContractEndDateTo = new DateOnly(2025, 2, 1)
        };

        var validationContext = new ValidationContext(searchParams);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(searchParams, validationContext, validationResults, true);

        isValid.Should().BeFalse();
        validationResults.Should().HaveCount(1);
        validationResults[0].ErrorMessage.Should().Be("To date must be after from date");
    }

    [Fact]
    public void FeeRangeValidation_WhenToIsLessThanFrom_ShouldHaveValidationError()
    {
        var searchParams = new SearchModel
        {
            FeeFrom = 10,
            FeeTo = 5
        };

        var validationContext = new ValidationContext(searchParams);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(searchParams, validationContext, validationResults, true);

        isValid.Should().BeFalse();
        validationResults.Should().HaveCount(1);
        validationResults[0].ErrorMessage.Should().Be("To must be more than from");
    }

    [Fact]
    public void FeeRangeValidation_WhenToIsEqualToFrom_ShouldBeValid()
    {
        var searchParams = new SearchModel
        {
            FeeFrom = 10,
            FeeTo = 10
        };

        var validationContext = new ValidationContext(searchParams);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(searchParams, validationContext, validationResults, true);

        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void FeeRangeValidation_WhenToIsGreaterThanFrom_ShouldBeValid()
    {
        var searchParams = new SearchModel
        {
            FeeFrom = 10,
            FeeTo = 20
        };

        var validationContext = new ValidationContext(searchParams);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(searchParams, validationContext, validationResults, true);

        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void SearchModel_WhenKeywordsIsPopulated_ShouldBeValid()
    {
        var searchParams = new SearchModel
        {
            Keywords = "test keywords"
        };

        var validationContext = new ValidationContext(searchParams);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(searchParams, validationContext, validationResults, true);

        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
    }

    public static IEnumerable<object?[]> SearchParamsBindingData => new List<object?[]>
    {
        new object[] { "test", "a-z", "open", "utilities-only", "direct-award", new[] { "upcoming", "active-buyers" }, "yes", 0m, 100m, "true" },
        new object[] { null!, null!, null!, null!, null!, null!, null!, null!, null!, null! }
    };

    [Theory]
    [MemberData(nameof(SearchParamsBindingData))]
    public void SearchParams_BindsValuesCorrectly(string? keywords, string? sortOrder, string? frameworkOptions, string? dynamicMarketOptions, string? awardMethod, string[]? status, string? contractingAuthorityUsage, decimal? feeFrom, decimal? feeTo, string? noFees)
    {
        var searchParams = new SearchModel
        {
            Keywords = keywords,
            SortOrder = sortOrder,
            FrameworkOptions = frameworkOptions,
            DynamicMarketOptions = dynamicMarketOptions,
            AwardMethod = awardMethod,
            Status = status?.ToList() ?? [],
            ContractingAuthorityUsage = contractingAuthorityUsage,
            FeeFrom = feeFrom,
            FeeTo = feeTo,
            NoFees = noFees
        };
        _model.SearchParams = searchParams;
        _model.SearchParams.Keywords.Should().Be(keywords);
        _model.SearchParams.SortOrder.Should().Be(sortOrder);
        _model.SearchParams.FrameworkOptions.Should().Be(frameworkOptions);
        _model.SearchParams.DynamicMarketOptions.Should().Be(dynamicMarketOptions);
        _model.SearchParams.AwardMethod.Should().Be(awardMethod);
        _model.SearchParams.Status.Should().BeEquivalentTo(status ?? []);
        _model.SearchParams.ContractingAuthorityUsage.Should().Be(contractingAuthorityUsage);
        _model.SearchParams.FeeFrom.Should().Be(feeFrom);
        _model.SearchParams.FeeTo.Should().Be(feeTo);
        _model.SearchParams.NoFees.Should().Be(noFees);
    }
}