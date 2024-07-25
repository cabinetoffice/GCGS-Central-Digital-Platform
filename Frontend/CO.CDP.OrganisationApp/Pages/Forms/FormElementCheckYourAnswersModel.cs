using CO.CDP.OrganisationApp.Models;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class FormElementCheckYourAnswersModel : FormElementModel
{
    public Guid OrganisationId { get; set; }
    public Guid FormId { get; set; }
    public Guid SectionId { get; set; }

    public List<QuestionAnswer>? ListOfAnswers { get; set; }

    public override FormAnswer? GetAnswer()
    {
        return null;
    }

    public override void SetAnswers(List<QuestionAnswer>? answers)
    {
        Answers = answers;
    }

    public string GetEditUrl(Guid questionId)
    {
        return $"/organisation/{OrganisationId}/form/{FormId}/section/{SectionId}/question/{questionId}/edit";
    }

    public override void SetAnswer(FormAnswer? answer)
    {

    }
}
