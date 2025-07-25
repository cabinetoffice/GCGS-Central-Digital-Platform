namespace CO.CDP.Forms.WebApi.Model.Validation;

public enum DateValidationType
{
    None,
    PastOnly,
    FutureOnly,
    MinDate,
    MaxDate,
    DateRange
}
