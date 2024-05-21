using System.Text.Json.Serialization;

namespace CO.CDP.DataSharing.WebApi.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
internal enum QuestionType
{
    Boolean,
    Numeric,
    Text,
    Option,
    DateTimeRange
}