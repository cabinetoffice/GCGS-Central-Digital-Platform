using System.Text.Json.Serialization;

namespace CO.CDP.Forms.WebApi.Model.Validation;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TextValidationType
{
    Year,
    Number,
    Percentage,
    Decimal
}