using System.Text.Json.Serialization;

namespace CO.CDP.OrganisationInformation;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OperationType
{
    SmallOrMediumSized,
    NonGovernmental,
    SupportedEmploymentProvider,
    PublicService,
    None
}