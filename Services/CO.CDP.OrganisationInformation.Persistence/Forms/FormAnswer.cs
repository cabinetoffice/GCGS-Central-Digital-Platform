using CO.CDP.EntityFrameworkCore.Timestamps;

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
    public required string BookingReference { get; init; }
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
    public required FormQuestion Question { get; init; }
    public required FormAnswerSet FormAnswerSet { get; init; }
    public bool? BoolValue { get; init; } = null;
    public double? NumericValue { get; init; } = null;
    public DateTime? DateValue { get; init; } = null;
    public DateTime? StartValue { get; init; } = null;
    public DateTime? EndValue { get; init; } = null;
    public string? TextValue { get; init; } = null;
    public string? OptionValue { get; init; } = null;
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}

public class FormAnswerSet : IEntityDate
{
    public int Id { get; set; }
    public required Guid Guid { get; set; }

    public required FormSection Section { get; init; }
    public required ICollection<FormAnswer> Answers { get; init; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}