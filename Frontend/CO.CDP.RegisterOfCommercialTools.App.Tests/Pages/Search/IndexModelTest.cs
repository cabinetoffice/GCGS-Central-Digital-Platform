using CO.CDP.RegisterOfCommercialTools.App.Services;
using CO.CDP.UI.Foundation.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.ComponentModel.DataAnnotations;
using CO.CDP.RegisterOfCommercialTools.App.Models;
using CO.CDP.RegisterOfCommercialTools.App.Pages;

namespace CO.CDP.RegisterOfCommercialTools.App.Tests.Pages.Search;

public class IndexModelTest
{
    private readonly Mock<ISearchService> _mockSearchService;
    private readonly IndexModel _model;

    public IndexModelTest()
    {
        _mockSearchService = new Mock<ISearchService>();
        var mockSirsiUrlService = new Mock<ISirsiUrlService>();
        mockSirsiUrlService.Setup(s => s.BuildUrl("/", null, null)).Returns("https://sirsi.home/");
        _model = new IndexModel(_mockSearchService.Object, mockSirsiUrlService.Object);
    }

    [Fact]
    public async Task OnGet_ShouldPopulateSearchResults()
    {
        var searchResults = new List<SearchResult>
        {
            new(Guid.NewGuid(), "Test Result", "Test Caption", "Test Tool", SearchResultStatus.Active, "1%", "Yes", "2025-01-01",
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
        _model.SearchParams.SubmissionDeadlineFromDay.Should().BeNull();
        _model.SearchParams.SubmissionDeadlineFromMonth.Should().BeNull();
        _model.SearchParams.SubmissionDeadlineFromYear.Should().BeNull();
        _model.SearchParams.SubmissionDeadlineToDay.Should().BeNull();
        _model.SearchParams.SubmissionDeadlineToMonth.Should().BeNull();
        _model.SearchParams.SubmissionDeadlineToYear.Should().BeNull();
        _model.SearchParams.ContractStartDateFromDay.Should().BeNull();
        _model.SearchParams.ContractStartDateFromMonth.Should().BeNull();
        _model.SearchParams.ContractStartDateFromYear.Should().BeNull();
        _model.SearchParams.ContractStartDateToDay.Should().BeNull();
        _model.SearchParams.ContractStartDateToMonth.Should().BeNull();
        _model.SearchParams.ContractStartDateToYear.Should().BeNull();
        _model.SearchParams.ContractEndDateFromDay.Should().BeNull();
        _model.SearchParams.ContractEndDateFromMonth.Should().BeNull();
        _model.SearchParams.ContractEndDateFromYear.Should().BeNull();
        _model.SearchParams.ContractEndDateToDay.Should().BeNull();
        _model.SearchParams.ContractEndDateToMonth.Should().BeNull();
        _model.SearchParams.ContractEndDateToYear.Should().BeNull();

        var expectedOpenAccordions = new[]
        {
            "commercial-tool", "commercial-tool-status", "contracting-authority-usage", "award-method", "fees",
            "date-range"
        };
        _model.OpenAccordions.Should().BeEquivalentTo(expectedOpenAccordions);
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
            SubmissionDeadlineFromDay = "1",
            SubmissionDeadlineFromMonth = "1",
            SubmissionDeadlineFromYear = "2025",
            SubmissionDeadlineToDay = "31",
            SubmissionDeadlineToMonth = "1",
            SubmissionDeadlineToYear = "2025",
            ContractStartDateFromDay = "1",
            ContractStartDateFromMonth = "2",
            ContractStartDateFromYear = "2025",
            ContractStartDateToDay = "28",
            ContractStartDateToMonth = "2",
            ContractStartDateToYear = "2025",
            ContractEndDateFromDay = "1",
            ContractEndDateFromMonth = "1",
            ContractEndDateFromYear = "2026",
            ContractEndDateToDay = "31",
            ContractEndDateToMonth = "1",
            ContractEndDateToYear = "2026"
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
            SubmissionDeadlineFromDay = "1",
            SubmissionDeadlineFromMonth = "2",
            SubmissionDeadlineFromYear = "2025",
            SubmissionDeadlineToDay = "1",
            SubmissionDeadlineToMonth = "1",
            SubmissionDeadlineToYear = "2025",
            ContractStartDateFromDay = "1",
            ContractStartDateFromMonth = "2",
            ContractStartDateFromYear = "2025",
            ContractStartDateToDay = "1",
            ContractStartDateToMonth = "1",
            ContractStartDateToYear = "2025",
            ContractEndDateFromDay = "1",
            ContractEndDateFromMonth = "2",
            ContractEndDateFromYear = "2025",
            ContractEndDateToDay = "1",
            ContractEndDateToMonth = "1",
            ContractEndDateToYear = "2025"
        };

        var validationContext = new ValidationContext(searchParams);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(searchParams, validationContext, validationResults, true);

        isValid.Should().BeFalse();
        validationResults.Should().HaveCount(3);
        validationResults.Select(r => r.ErrorMessage).Should().Contain("Submission deadline to date must be after from date");
        validationResults.Select(r => r.ErrorMessage).Should().Contain("Contract start date to date must be after from date");
        validationResults.Select(r => r.ErrorMessage).Should().Contain("Contract end date to date must be after from date");
    }

    [Fact]
    public void DateRangeValidation_WhenToDateIsAfterFromDate_ShouldBeValid()
    {
        var searchParams = new SearchModel
        {
            SubmissionDeadlineFromDay = "1",
            SubmissionDeadlineFromMonth = "1",
            SubmissionDeadlineFromYear = "2025",
            SubmissionDeadlineToDay = "1",
            SubmissionDeadlineToMonth = "2",
            SubmissionDeadlineToYear = "2025",
            ContractStartDateFromDay = "1",
            ContractStartDateFromMonth = "1",
            ContractStartDateFromYear = "2025",
            ContractStartDateToDay = "1",
            ContractStartDateToMonth = "2",
            ContractStartDateToYear = "2025",
            ContractEndDateFromDay = "1",
            ContractEndDateFromMonth = "1",
            ContractEndDateFromYear = "2025",
            ContractEndDateToDay = "1",
            ContractEndDateToMonth = "2",
            ContractEndDateToYear = "2025"
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
            SubmissionDeadlineFromDay = "1",
            SubmissionDeadlineFromMonth = "1",
            SubmissionDeadlineFromYear = "2025",
            SubmissionDeadlineToDay = "1",
            SubmissionDeadlineToMonth = "1",
            SubmissionDeadlineToYear = "2025",
            ContractStartDateFromDay = "1",
            ContractStartDateFromMonth = "1",
            ContractStartDateFromYear = "2025",
            ContractStartDateToDay = "1",
            ContractStartDateToMonth = "1",
            ContractStartDateToYear = "2025",
            ContractEndDateFromDay = "1",
            ContractEndDateFromMonth = "1",
            ContractEndDateFromYear = "2025",
            ContractEndDateToDay = "1",
            ContractEndDateToMonth = "1",
            ContractEndDateToYear = "2025"
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
            SubmissionDeadlineFromDay = "1",
            SubmissionDeadlineFromMonth = "1",
            SubmissionDeadlineFromYear = "2025",
            // SubmissionDeadlineTo components left null
            // ContractStartDateFrom components left null
            ContractStartDateToDay = "1",
            ContractStartDateToMonth = "1",
            ContractStartDateToYear = "2025",
            ContractEndDateFromDay = "1",
            ContractEndDateFromMonth = "1",
            ContractEndDateFromYear = "2025"
            // ContractEndDateTo components left null
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
            SubmissionDeadlineFromDay = "1",
            SubmissionDeadlineFromMonth = "2",
            SubmissionDeadlineFromYear = "2025",
            SubmissionDeadlineToDay = "1",
            SubmissionDeadlineToMonth = "1",
            SubmissionDeadlineToYear = "2025",
            ContractStartDateFromDay = "1",
            ContractStartDateFromMonth = "1",
            ContractStartDateFromYear = "2025",
            ContractStartDateToDay = "1",
            ContractStartDateToMonth = "2",
            ContractStartDateToYear = "2025",
            ContractEndDateFromDay = "1",
            ContractEndDateFromMonth = "1",
            ContractEndDateFromYear = "2025",
            ContractEndDateToDay = "1",
            ContractEndDateToMonth = "2",
            ContractEndDateToYear = "2025"
        };

        var validationContext = new ValidationContext(searchParams);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(searchParams, validationContext, validationResults, true);

        isValid.Should().BeFalse();
        validationResults.Should().HaveCount(1);
        validationResults[0].ErrorMessage.Should().Be("Submission deadline to date must be after from date");
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

    [Fact]
    public void SearchModel_WhenNoFeesIsSelectedWithFeeFromAndFeeTo_ShouldHaveValidationError()
    {
        var searchParams = new SearchModel
        {
            NoFees = "true",
            FeeFrom = 10,
            FeeTo = 20
        };

        var validationContext = new ValidationContext(searchParams);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(searchParams, validationContext, validationResults, true);

        isValid.Should().BeFalse();
        validationResults.Should().HaveCount(1);
        validationResults[0].ErrorMessage.Should().Be("Fee from and to cannot be provided when 'No fees' is selected");
    }

    [Fact]
    public void SearchModel_WhenNoFeesIsSelectedWithFeeFrom_ShouldHaveValidationError()
    {
        var searchParams = new SearchModel
        {
            NoFees = "true",
            FeeFrom = 10
        };

        var validationContext = new ValidationContext(searchParams);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(searchParams, validationContext, validationResults, true);

        isValid.Should().BeFalse();
        validationResults.Should().HaveCount(1);
        validationResults[0].ErrorMessage.Should().Be("Fee from cannot be provided when 'No fees' is selected");
    }

    [Fact]
    public void SearchModel_WhenNoFeesIsSelectedWithFeeTo_ShouldHaveValidationError()
    {
        var searchParams = new SearchModel
        {
            NoFees = "true",
            FeeTo = 20
        };

        var validationContext = new ValidationContext(searchParams);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(searchParams, validationContext, validationResults, true);

        isValid.Should().BeFalse();
        validationResults.Should().HaveCount(1);
        validationResults[0].ErrorMessage.Should().Be("Fee to cannot be provided when 'No fees' is selected");
    }

    [Fact]
    public void SearchModel_WhenNoFeesIsNotSelected_ShouldBeValid()
    {
        var searchParams = new SearchModel
        {
            NoFees = null,
            FeeFrom = 10,
            FeeTo = 20
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

    [Fact]
    public async Task OnGetAsync_SetsSirsiHomeUrl()
    {
        await _model.OnGetAsync();
        _model.SirsiHomeUrl.Should().Be("https://sirsi.home/");
    }

    [Fact]
    public async Task OnGet_ShouldSetTotalCount()
    {
        var searchResults = new List<SearchResult>
        {
            new(Guid.NewGuid(), "Test Result", "Test Caption", "Test Tool", SearchResultStatus.Active, "1%", "Yes", "2025-01-01",
                "2025-01-01 to 2025-12-31", "Direct Award")
        };
        _mockSearchService.Setup(s => s.SearchAsync(It.IsAny<SearchModel>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((searchResults, 42));

        await _model.OnGetAsync();

        _model.TotalCount.Should().Be(42);
    }

    [Fact]
    public void DateComponentValidation_WhenAllComponentsEmpty_ShouldBeValid()
    {
        var searchParams = new SearchModel
        {
            SubmissionDeadlineFromDay = null,
            SubmissionDeadlineFromMonth = null,
            SubmissionDeadlineFromYear = null
        };

        var validationContext = new ValidationContext(searchParams);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(searchParams, validationContext, validationResults, true);

        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void DateComponentValidation_WhenOnlyYearProvided_ShouldHaveValidationError()
    {
        var searchParams = new SearchModel
        {
            SubmissionDeadlineFromDay = null,
            SubmissionDeadlineFromMonth = null,
            SubmissionDeadlineFromYear = "2025"
        };

        var validationContext = new ValidationContext(searchParams);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(searchParams, validationContext, validationResults, true);

        isValid.Should().BeFalse();
        validationResults.Should().HaveCount(2);
        validationResults.Select(r => r.ErrorMessage).Should().Contain("Submission deadline from must include a day and month");
    }

    [Fact]
    public void DateComponentValidation_WhenAllComponentsProvided_ShouldBeValid()
    {
        var searchParams = new SearchModel
        {
            SubmissionDeadlineFromDay = "15",
            SubmissionDeadlineFromMonth = "6",
            SubmissionDeadlineFromYear = "2025"
        };

        var validationContext = new ValidationContext(searchParams);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(searchParams, validationContext, validationResults, true);

        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void DateComponentValidation_WhenInvalidDay_ShouldHaveValidationError()
    {
        var searchParams = new SearchModel
        {
            SubmissionDeadlineFromDay = "32",
            SubmissionDeadlineFromMonth = "6",
            SubmissionDeadlineFromYear = "2025"
        };

        var validationContext = new ValidationContext(searchParams);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(searchParams, validationContext, validationResults, true);

        isValid.Should().BeFalse();
        validationResults.Should().HaveCount(1);
        validationResults[0].ErrorMessage.Should().Be("Submission deadline from must have a valid day");
    }

    [Fact]
    public void DateComponentValidation_WhenInvalidMonth_ShouldHaveValidationError()
    {
        var searchParams = new SearchModel
        {
            SubmissionDeadlineFromDay = "15",
            SubmissionDeadlineFromMonth = "13",
            SubmissionDeadlineFromYear = "2025"
        };

        var validationContext = new ValidationContext(searchParams);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(searchParams, validationContext, validationResults, true);

        isValid.Should().BeFalse();
        validationResults.Should().HaveCount(2);
        validationResults.Select(r => r.ErrorMessage).Should().Contain("Submission deadline from must have a valid month");
        validationResults.Select(r => r.ErrorMessage).Should().Contain("Submission deadline from must be a real date");
    }

    [Fact]
    public void DateComponentValidation_WhenInvalidYear_ShouldHaveValidationError()
    {
        var searchParams = new SearchModel
        {
            SubmissionDeadlineFromDay = "15",
            SubmissionDeadlineFromMonth = "6",
            SubmissionDeadlineFromYear = "25"
        };

        var validationContext = new ValidationContext(searchParams);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(searchParams, validationContext, validationResults, true);

        isValid.Should().BeFalse();
        validationResults.Should().HaveCount(1);
        validationResults[0].ErrorMessage.Should().Be("Submission deadline from year must include 4 numbers");
    }

    [Fact]
    public void DateComponentValidation_WhenInvalidDate_ShouldHaveValidationError()
    {
        var searchParams = new SearchModel
        {
            SubmissionDeadlineFromDay = "31",
            SubmissionDeadlineFromMonth = "2",
            SubmissionDeadlineFromYear = "2025"
        };

        var validationContext = new ValidationContext(searchParams);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(searchParams, validationContext, validationResults, true);

        isValid.Should().BeFalse();
        validationResults.Should().HaveCount(1);
        validationResults[0].ErrorMessage.Should().Be("Submission deadline from must be a real date");
    }
}