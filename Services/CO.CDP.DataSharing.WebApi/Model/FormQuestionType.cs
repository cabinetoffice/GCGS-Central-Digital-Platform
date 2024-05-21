using System.Text.Json.Serialization;

namespace CO.CDP.DataSharing.WebApi.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
internal enum FormQuestionType
{
    Boolean,
    Numeric,
    Text,
    Option,
    DateTimeRange
}