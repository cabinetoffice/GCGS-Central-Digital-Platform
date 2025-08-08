using System.Text.Json.Serialization;

namespace CO.CDP.OrganisationApp.Models;

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
