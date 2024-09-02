using System.Text.Json.Serialization;

namespace CO.CDP.DataSharing.WebApi.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FormQuestionType
{
    None,
    Boolean,
    Numeric,
    Text,
    Option,
    DateTimeRange,
    Date
}