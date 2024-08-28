using CO.CDP.OrganisationInformation.Persistence.Forms;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CO.CDP.DataSharing.WebApi.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FormSectionType
{
    Standard,  //Standard Questionnaire Form
    Declaration //Declaration Form
}


public record FormSection
{
  
}