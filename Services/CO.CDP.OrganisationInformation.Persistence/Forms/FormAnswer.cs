using CO.CDP.EntityFrameworkCore.Timestamps;
using System.ComponentModel.DataAnnotations.Schema;

namespace CO.CDP.OrganisationInformation.Persistence.Forms;

public class SharedConsent : IEntityDate
{
    public int Id { get; set; }
    public required Guid Guid { get; set; }

    [ForeignKey(nameof(Organisation))]
    public required int OrganisationId { get; set; }
    public required Organisation Organisation { get; set; }

    [ForeignKey(nameof(Form))]
    public required int FormId { get; set; }
    public required Form Form { get; init; }
    public ICollection<FormAnswerSet> AnswerSets { get; init; } = [];
    public required SubmissionState SubmissionState { get; set; } = SubmissionState.Draft;
    public DateTimeOffset? SubmittedAt { get; set; }
    public required string FormVersionId { get; init; }
    public string? ShareCode { get; set; }
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
    [ForeignKey(nameof(Question))]
    public required int QuestionId { get; set; }
    public required FormQuestion Question { get; set; }
    [ForeignKey(nameof(FormAnswerSet))]
    public required int FormAnswerSetId { get; set; }
    public FormAnswerSet? FormAnswerSet { get; set; }
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
    public required string Country { get; init; }
}

public class FormAnswerSet : IEntityDate
{
    public int Id { get; set; }
    public required Guid Guid { get; set; }
    [ForeignKey(nameof(SharedConsent))]
    public required int SharedConsentId { get; set; }
    public required SharedConsent SharedConsent { get; set; }
    [ForeignKey(nameof(Section))]
    public required int SectionId { get; set; }
    public required FormSection Section { get; init; }
    public ICollection<FormAnswer> Answers { get; set; } = [];
    public bool Deleted { get; set; } = false;
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}