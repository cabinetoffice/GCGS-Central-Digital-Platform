using System.Text.Json.Serialization;

namespace CO.CDP.OrganisationApp.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InputWidthType
{
    Width2,
    Width3,
    Width4,
    Width5,
    Width10,
    Width20,
    OneThird,
    OneHalf,
    TwoThirds,
    Full
}
