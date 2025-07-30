using System.Text.Json.Serialization;

namespace CO.CDP.OrganisationApp.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DateValidationType
{
    None,
    PastOnly,
    FutureOnly,
    MinDate,
    MaxDate,
    DateRange
}
