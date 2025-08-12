using System.Text.Json.Serialization;

namespace CO.CDP.Forms.WebApi.Model.Validation;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DateValidationType
{
    PastOrTodayOnly,
    FutureOrTodayOnly,
    MinDate,
    MaxDate,
    DateRange,
    PastOnly,
    FutureOnly
}
