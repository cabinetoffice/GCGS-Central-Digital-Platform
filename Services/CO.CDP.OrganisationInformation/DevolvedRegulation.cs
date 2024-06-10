using System.Text.Json.Serialization;

namespace CO.CDP.OrganisationInformation;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DevolvedRegulation
{
    NorthernIreland = 1,
    Scotland,
    Wales
}