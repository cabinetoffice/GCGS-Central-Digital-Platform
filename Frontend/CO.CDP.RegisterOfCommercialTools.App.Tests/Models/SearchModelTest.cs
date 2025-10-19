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
            ContractStartDateToYear = "2025"
        };

        var validationContext = new ValidationContext(searchParams);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(searchParams, validationContext, validationResults, true);

        isValid.Should().BeFalse();
        validationResults.Should().HaveCount(2);
        validationResults.Select(r => r.ErrorMessage).Should().Contain("Submission deadline to date must be after from date");
        validationResults.Select(r => r.ErrorMessage).Should().Contain("Contract start date to date must be after from date");
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
            ContractStartDateToYear = "2025"
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
            ContractStartDateToYear = "2025"
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
            ContractStartDateToDay = "1",
            ContractStartDateToMonth = "1",
            ContractStartDateToYear = "2025"
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
            ContractStartDateToYear = "2025"
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
    public void SearchModel_WhenAllDateComponentsEmpty_ShouldBeValid()
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
    public void SearchModel_WhenOnlyYearProvided_ShouldHaveValidationError()
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
        validationResults.Should().HaveCount(1);
        validationResults.Select(r => r.ErrorMessage).Should().Contain("Submission deadline from must include a day and month");
    }

    [Fact]
    public void SearchModel_WhenAllDateComponentsProvided_ShouldBeValid()
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
    public void SearchModel_WhenInvalidDay_ShouldHaveValidationError()
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
    public void SearchModel_WhenInvalidMonth_ShouldHaveValidationError()
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
        validationResults.Should().HaveCount(1);
        validationResults.Select(r => r.ErrorMessage).Should().Contain("Submission deadline from must have a valid month");
    }

    [Fact]
    public void SearchModel_WhenInvalidYear_ShouldHaveValidationError()
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
    public void SearchModel_WhenInvalidDate_ShouldHaveValidationError()
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