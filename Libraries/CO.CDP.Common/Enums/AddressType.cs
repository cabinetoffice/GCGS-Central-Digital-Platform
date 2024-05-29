using System.Text.Json.Serialization;

namespace CO.CDP.Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AddressType
{
    Registered = 1,
    Postal
}