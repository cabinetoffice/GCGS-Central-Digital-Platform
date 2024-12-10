using System.Text.Json.Serialization;
using CO.CDP.OrganisationInformation.Serialisation;

namespace CO.CDP.OrganisationInformation;


[JsonConverter(typeof(LowerCamelCaseEnumConverter<OrganisationType>))]
public enum OrganisationType
{
    Organisation = 1,
    Consortium = 2
}