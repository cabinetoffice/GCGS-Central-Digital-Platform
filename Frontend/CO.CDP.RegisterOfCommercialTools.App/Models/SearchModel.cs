using System.ComponentModel.DataAnnotations;
using CO.CDP.RegisterOfCommercialTools.App.Validation;

namespace CO.CDP.RegisterOfCommercialTools.App.Models;

public class SearchModel : IValidatableObject
{
    public string? Keywords { get; set; }
    public string? SortOrder { get; set; }
    public string? FrameworkOptions { get; set; }
    public string? DynamicMarketOptions { get; set; }
    public string? CommercialToolStatus { get; set; }
    public string? AwardMethod { get; set; }

    [Range(0, 100, ErrorMessage = "Enter a value between 0 and 100")]
    public decimal? FeeFrom { get; set; }

    [Range(0, 100, ErrorMessage = "Enter a value between 0 and 100")]
    [DecimalRange("FeeFrom", ErrorMessage = "To must be more than from")]
    public decimal? FeeTo { get; set; }

    public string? NoFees { get; set; }
    public List<string> Status { get; set; } = [];
    public string? ContractingAuthorityUsage { get; set; }

    public DateRange SubmissionDeadline { get; init; } = new("Submission deadline");
    public DateRange ContractStartDate { get; init; } = new("Contract start date");
    public DateRange ContractEndDate { get; init; } = new("Contract end date");

    public DateOnly? SubmissionDeadlineFrom => SubmissionDeadline.From.Value;
    public DateOnly? SubmissionDeadlineTo => SubmissionDeadline.To.Value;
    public DateOnly? ContractStartDateFrom => ContractStartDate.From.Value;
    public DateOnly? ContractStartDateTo => ContractStartDate.To.Value;
    public DateOnly? ContractEndDateFrom => ContractEndDate.From.Value;
    public DateOnly? ContractEndDateTo => ContractEndDate.To.Value;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrEmpty(NoFees))
        {
            var feeFromHasValue = FeeFrom.HasValue;
            var feeToHasValue = FeeTo.HasValue;

            switch (feeFromHasValue)
            {
                case true when feeToHasValue:
                    yield return new ValidationResult(
                        "Fee from and to cannot be provided when 'No fees' is selected",
                        [nameof(FeeFrom), nameof(FeeTo)]
                    );
                    break;
                case true:
                    yield return new ValidationResult(
                        "Fee from cannot be provided when 'No fees' is selected",
                        [nameof(FeeFrom)]
                    );
                    break;
                default:
                {
                    if (feeToHasValue)
                    {
                        yield return new ValidationResult(
                            "Fee to cannot be provided when 'No fees' is selected",
                            [nameof(FeeTo)]
                        );
                    }

                    break;
                }
            }
        }

        var submissionDeadlineValidation = SubmissionDeadline.Validate(validationContext);
        foreach (var result in submissionDeadlineValidation)
        {
            yield return result;
        }

        var contractStartDateValidation = ContractStartDate.Validate(validationContext);
        foreach (var result in contractStartDateValidation)
        {
            yield return result;
        }

        var contractEndDateValidation = ContractEndDate.Validate(validationContext);
        foreach (var result in contractEndDateValidation)
        {
            yield return result;
        }
    }
}