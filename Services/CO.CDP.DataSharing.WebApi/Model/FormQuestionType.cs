using System.Text.Json.Serialization;

namespace CO.CDP.DataSharing.WebApi.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FormQuestionType
{
    NoInput,
    Text,
    FileUpload,
    YesOrNo,
    SingleChoice,
    MultipleChoice,
    CheckYourAnswers,
    Date,
    CheckBox,
    Address
}