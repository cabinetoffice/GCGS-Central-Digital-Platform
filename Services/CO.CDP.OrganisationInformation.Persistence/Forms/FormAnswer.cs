using CO.CDP.EntityFrameworkCore.Timestamps;
using System.ComponentModel.DataAnnotations.Schema;

namespace CO.CDP.OrganisationInformation.Persistence.Forms;

public class SharedConsent : IEntityDate
{
    public int Id { get; set; }
    public required Guid Guid { get; set; }
    public required Organisation Organisation { get; init; }
    public required Form Form { get; init; }
    public required ICollection<FormAnswerSet> AnswerSets { get; init; }
    public required SubmissionState SubmissionState { get; set; } = SubmissionState.Draft;
    public required DateTimeOffset SubmittedAt { get; set; }
    public required string FormVersionId { get; init; }
    public required string BookingReference { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}

public enum SubmissionState
{
    Draft,
    Submitted
}

public class FormAnswer : IEntityDate
{
    public int Id { get; set; }
    public required Guid Guid { get; set; }
    public required FormQuestion Question { get; set; }
    public required FormAnswerSet FormAnswerSet { get; set; }
    public bool? BoolValue { get; set; }
    public double? NumericValue { get; set; }
    public DateTime? DateValue { get; set; }
    public DateTime? StartValue { get; set; }
    public DateTime? EndValue { get; set; }
    public string? TextValue { get; set; }
    public string? OptionValue { get; set; }
    public FormAddress? AddressValue { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}

public class FormAddress
{
    public required string StreetAddress { get; init; }
    public required string Locality { get; init; }
    public string? Region { get; init; }
    public required string PostalCode { get; init; }
    public required string CountryName { get; init; }
}

public class FormAnswerSet : IEntityDate
{
    public int Id { get; set; }
    public required Guid Guid { get; set; }

    [ForeignKey(nameof(Organisation))]
    public required int OrganisationId { get; set; }
    public required Organisation Organisation { get; set; }

    public required FormSection Section { get; init; }
    public required ICollection<FormAnswer> Answers { get; set; }
    public bool Deleted { get; set; } = false;
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}