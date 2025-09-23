using System.ComponentModel.DataAnnotations;
using CO.CDP.RegisterOfCommercialTools.App.Validation;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.RegisterOfCommercialTools.App.Models;

public class SearchModel : IValidatableObject
{
    [FromQuery(Name = "q")] public string? Keywords { get; set; }
    [FromQuery(Name = "sort")] public string? SortOrder { get; set; }
    [FromQuery(Name = "framework")] public string? FrameworkOptions { get; set; }
    [FromQuery(Name = "market")] public string? DynamicMarketOptions { get; set; }

    [FromQuery(Name = "award")] public string? AwardMethod { get; set; }

    [FromQuery(Name = "fee_min")]
    [Range(0, 100, ErrorMessage = "Enter a value between 0 and 100")]
    public decimal? FeeMin { get; set; }

    [FromQuery(Name = "fee_max")]
    [Range(0, 100, ErrorMessage = "Enter a value between 0 and 100")]
    [DecimalRange("FeeMin", ErrorMessage = "To must be more than from")]
    public decimal? FeeMax { get; set; }

    [FromQuery(Name = "no_fees")] public string? NoFees { get; set; }
    [FromQuery(Name = "status")] public List<string> Status { get; set; } = [];
    [FromQuery(Name = "usage")] public string? ContractingAuthorityUsage { get; set; }

    [FromQuery(Name = "cpv")] public List<string> CpvCodes { get; set; } = [];

    public DateRange SubmissionDeadline { get; init; } = new("Submission deadline");
    public DateRange ContractStartDate { get; init; } = new("Contract start date");
    public DateRange ContractEndDate { get; init; } = new("Contract end date");

    [FromQuery(Name = "sub_from")] public DateOnly? SubmissionDeadlineFrom => SubmissionDeadline.From.Value;
    [FromQuery(Name = "sub_to")] public DateOnly? SubmissionDeadlineTo => SubmissionDeadline.To.Value;
    [FromQuery(Name = "start_from")] public DateOnly? ContractStartDateFrom => ContractStartDate.From.Value;
    [FromQuery(Name = "start_to")] public DateOnly? ContractStartDateTo => ContractStartDate.To.Value;
    [FromQuery(Name = "end_from")] public DateOnly? ContractEndDateFrom => ContractEndDate.From.Value;
    [FromQuery(Name = "end_to")] public DateOnly? ContractEndDateTo => ContractEndDate.To.Value;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrEmpty(NoFees))
        {
            var feeFromHasValue = FeeMin.HasValue;
            var feeToHasValue = FeeMax.HasValue;

            switch (feeFromHasValue)
            {
                case true when feeToHasValue:
                    yield return new ValidationResult(
                        "Fee from and to cannot be provided when 'No fees' is selected",
                        [nameof(FeeMin), nameof(FeeMax)]
                    );
                    break;
                case true:
                    yield return new ValidationResult(
                        "Fee from cannot be provided when 'No fees' is selected",
                        [nameof(FeeMin)]
                    );
                    break;
                default:
                {
                    if (feeToHasValue)
                    {
                        yield return new ValidationResult(
                            "Fee to cannot be provided when 'No fees' is selected",
                            [nameof(FeeMax)]
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