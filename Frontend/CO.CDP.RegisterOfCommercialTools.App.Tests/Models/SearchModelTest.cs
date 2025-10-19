using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using CO.CDP.RegisterOfCommercialTools.App.Models;

namespace CO.CDP.RegisterOfCommercialTools.App.Tests.Models;

public class SearchModelTest
{
    [Fact]
    public void DateRangeValidation_WhenToDateIsBeforeFromDate_ShouldHaveValidationError()
    {
        var searchParams = new SearchModel
        {
            SubmissionDeadline = new DateRange("Submission deadline")
            {
                From = new DateComponent("Submission deadline from") { Day = "1", Month = "2", Year = "2025" },
                To = new DateComponent("Submission deadline to") { Day = "1", Month = "1", Year = "2025" }
            },
            ContractStartDate = new DateRange("Contract start date")
            {
                From = new DateComponent("Contract start date from") { Day = "1", Month = "2", Year = "2025" },
                To = new DateComponent("Contract start date to") { Day = "1", Month = "1", Year = "2025" }
            },
            ContractEndDate = new DateRange("Contract end date")
            {
                From = new DateComponent("Contract end date from") { Day = "1", Month = "2", Year = "2025" },
                To = new DateComponent("Contract end date to") { Day = "1", Month = "1", Year = "2025" }
            }
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
            SubmissionDeadline = new DateRange("Submission deadline")
            {
                From = new DateComponent("Submission deadline from") { Day = "1", Month = "1", Year = "2025" },
                To = new DateComponent("Submission deadline to") { Day = "1", Month = "2", Year = "2025" }
            },
            ContractStartDate = new DateRange("Contract start date")
            {
                From = new DateComponent("Contract start date from") { Day = "1", Month = "1", Year = "2025" },
                To = new DateComponent("Contract start date to") { Day = "1", Month = "2", Year = "2025" }
            },
            ContractEndDate = new DateRange("Contract end date")
            {
                From = new DateComponent("Contract end date from") { Day = "1", Month = "1", Year = "2025" },
                To = new DateComponent("Contract end date to") { Day = "1", Month = "2", Year = "2025" }
            }
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
            SubmissionDeadline = new DateRange("Submission deadline")
            {
                From = new DateComponent("Submission deadline from") { Day = "1", Month = "1", Year = "2025" },
                To = new DateComponent("Submission deadline to") { Day = "1", Month = "1", Year = "2025" }
            },
            ContractStartDate = new DateRange("Contract start date")
            {
                From = new DateComponent("Contract start date from") { Day = "1", Month = "1", Year = "2025" },
                To = new DateComponent("Contract start date to") { Day = "1", Month = "1", Year = "2025" }
            },
            ContractEndDate = new DateRange("Contract end date")
            {
                From = new DateComponent("Contract end date from") { Day = "1", Month = "1", Year = "2025" },
                To = new DateComponent("Contract end date to") { Day = "1", Month = "1", Year = "2025" }
            }
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
            SubmissionDeadline = new DateRange("Submission deadline")
            {
                From = new DateComponent("Submission deadline from") { Day = "1", Month = "1", Year = "2025" }
            },
            ContractStartDate = new DateRange("Contract start date")
            {
                To = new DateComponent("Contract start date to") { Day = "1", Month = "1", Year = "2025" }
            },
            ContractEndDate = new DateRange("Contract end date")
            {
                From = new DateComponent("Contract end date from") { Day = "1", Month = "1", Year = "2025" }
            }
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
            SubmissionDeadline = new DateRange("Submission deadline")
            {
                From = new DateComponent("Submission deadline from") { Day = "1", Month = "2", Year = "2025" },
                To = new DateComponent("Submission deadline to") { Day = "1", Month = "1", Year = "2025" }
            },
            ContractStartDate = new DateRange("Contract start date")
            {
                From = new DateComponent("Contract start date from") { Day = "1", Month = "1", Year = "2025" },
                To = new DateComponent("Contract start date to") { Day = "1", Month = "2", Year = "2025" }
            },
            ContractEndDate = new DateRange("Contract end date")
            {
                From = new DateComponent("Contract end date from") { Day = "1", Month = "1", Year = "2025" },
                To = new DateComponent("Contract end date to") { Day = "1", Month = "2", Year = "2025" }
            }
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
            FeeMin = 10,
            FeeMax = 5
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
            FeeMin = 10,
            FeeMax = 10
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
            FeeMin = 10,
            FeeMax = 20
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
            FeeMin = 10,
            FeeMax = 20
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
            FeeMin = 10
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
            FeeMax = 20
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
            FeeMin = 10,
            FeeMax = 20
        };

        var validationContext = new ValidationContext(searchParams);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(searchParams, validationContext, validationResults, true);

        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void DateComponent_WhenAllComponentsEmpty_ShouldBeValid()
    {
        var dateComponent = new DateComponent("Submission deadline from") { Day = null, Month = null, Year = null };
        var validationContext = new ValidationContext(dateComponent);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(dateComponent, validationContext, validationResults, true);

        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void DateComponent_WhenOnlyYearProvided_ShouldHaveValidationError()
    {
        var dateComponent = new DateComponent("Submission deadline from") { Day = null, Month = null, Year = "2025" };
        var validationContext = new ValidationContext(dateComponent);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(dateComponent, validationContext, validationResults, true);

        isValid.Should().BeFalse();
        validationResults.Should().HaveCount(1);
        validationResults.Select(r => r.ErrorMessage).Should().Contain("Submission deadline from must include a day and month");
    }

    [Fact]
    public void DateComponent_WhenAllComponentsProvided_ShouldBeValid()
    {
        var dateComponent = new DateComponent("Submission deadline from") { Day = "15", Month = "6", Year = "2025" };
        var validationContext = new ValidationContext(dateComponent);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(dateComponent, validationContext, validationResults, true);

        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void DateComponent_WhenInvalidDay_ShouldHaveValidationError()
    {
        var dateComponent = new DateComponent("Submission deadline from") { Day = "32", Month = "6", Year = "2025" };
        var validationContext = new ValidationContext(dateComponent);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(dateComponent, validationContext, validationResults, true);

        isValid.Should().BeFalse();
        validationResults.Should().HaveCount(1);
        validationResults[0].ErrorMessage.Should().Be("Submission deadline from must have a valid day");
    }

    [Fact]
    public void DateComponent_WhenInvalidMonth_ShouldHaveValidationError()
    {
        var dateComponent = new DateComponent("Submission deadline from") { Day = "15", Month = "13", Year = "2025" };
        var validationContext = new ValidationContext(dateComponent);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(dateComponent, validationContext, validationResults, true);

        isValid.Should().BeFalse();
        validationResults.Should().HaveCount(1);
        validationResults.Select(r => r.ErrorMessage).Should().Contain("Submission deadline from must have a valid month");
    }

    [Fact]
    public void DateComponent_WhenInvalidYear_ShouldHaveValidationError()
    {
        var dateComponent = new DateComponent("Submission deadline from") { Day = "15", Month = "6", Year = "25" };
        var validationContext = new ValidationContext(dateComponent);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(dateComponent, validationContext, validationResults, true);

        isValid.Should().BeFalse();
        validationResults.Should().HaveCount(1);
        validationResults[0].ErrorMessage.Should().Be("Submission deadline from year must include 4 numbers");
    }

    [Fact]
    public void DateComponent_WhenInvalidDate_ShouldHaveValidationError()
    {
        var dateComponent = new DateComponent("Submission deadline from") { Day = "31", Month = "2", Year = "2025" };
        var validationContext = new ValidationContext(dateComponent);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(dateComponent, validationContext, validationResults, true);

        isValid.Should().BeFalse();
        validationResults.Should().HaveCount(1);
        validationResults[0].ErrorMessage.Should().Be("Submission deadline from must be a real date");
    }

    [Fact]
    public void SearchModel_FilterFrameworks_DefaultsToFalse()
    {
        var searchParams = new SearchModel();

        searchParams.FilterFrameworks.Should().BeFalse();
    }

    [Fact]
    public void SearchModel_FilterFrameworks_CanBeSetToTrue()
    {
        var searchParams = new SearchModel
        {
            FilterFrameworks = true
        };

        searchParams.FilterFrameworks.Should().BeTrue();
    }

    [Fact]
    public void SearchModel_FilterDynamicMarkets_DefaultsToFalse()
    {
        var searchParams = new SearchModel();

        searchParams.FilterDynamicMarkets.Should().BeFalse();
    }

    [Fact]
    public void SearchModel_FilterDynamicMarkets_CanBeSetToTrue()
    {
        var searchParams = new SearchModel
        {
            FilterDynamicMarkets = true
        };

        searchParams.FilterDynamicMarkets.Should().BeTrue();
    }

    [Fact]
    public void SearchModel_IsOpenFrameworks_DefaultsToFalse()
    {
        var searchParams = new SearchModel();

        searchParams.IsOpenFrameworks.Should().BeFalse();
    }

    [Fact]
    public void SearchModel_IsOpenFrameworks_CanBeSetToTrue()
    {
        var searchParams = new SearchModel
        {
            IsOpenFrameworks = true
        };

        searchParams.IsOpenFrameworks.Should().BeTrue();
    }

    [Fact]
    public void SearchModel_IsUtilitiesOnly_DefaultsToFalse()
    {
        var searchParams = new SearchModel();

        searchParams.IsUtilitiesOnly.Should().BeFalse();
    }

    [Fact]
    public void SearchModel_IsUtilitiesOnly_CanBeSetToTrue()
    {
        var searchParams = new SearchModel
        {
            IsUtilitiesOnly = true
        };

        searchParams.IsUtilitiesOnly.Should().BeTrue();
    }

    [Fact]
    public void SearchModel_WhenFilterFrameworksIsTrue_ShouldBeValid()
    {
        var searchParams = new SearchModel
        {
            FilterFrameworks = true,
            IsOpenFrameworks = true
        };

        var validationContext = new ValidationContext(searchParams);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(searchParams, validationContext, validationResults, true);

        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void SearchModel_WhenFilterDynamicMarketsIsTrue_ShouldBeValid()
    {
        var searchParams = new SearchModel
        {
            FilterDynamicMarkets = true,
            IsUtilitiesOnly = true
        };

        var validationContext = new ValidationContext(searchParams);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(searchParams, validationContext, validationResults, true);

        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void SearchModel_WhenBothFiltersAreTrue_ShouldBeValid()
    {
        var searchParams = new SearchModel
        {
            FilterFrameworks = true,
            IsOpenFrameworks = true,
            FilterDynamicMarkets = true,
            IsUtilitiesOnly = true
        };

        var validationContext = new ValidationContext(searchParams);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(searchParams, validationContext, validationResults, true);

        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
    }
}