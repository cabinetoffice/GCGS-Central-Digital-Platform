using System.Text.Json.Serialization;

namespace CO.CDP.OrganisationInformation;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OperationType
{
    SmallorMediumSized = 1,
    NonGovernmental,
    SupportedEmploymentProvider,
    PublicService,
    None
}