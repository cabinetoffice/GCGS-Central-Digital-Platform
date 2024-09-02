using System.Text.Json.Serialization;

namespace CO.CDP.DataSharing.WebApi.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FormQuestionType
{
    Boolean,
    Numeric,
    Text,
    Option,
    DateTimeRange,
    Date
}