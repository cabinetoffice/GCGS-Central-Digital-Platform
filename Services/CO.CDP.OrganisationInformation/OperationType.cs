using System.Text.Json.Serialization;

namespace CO.CDP.OrganisationInformation;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OperationType
{
    None,
    SmallOrMediumSized,
    NonGovernmental,
    SupportedEmploymentProvider,
    PublicService,
    NoneOfAbove
}