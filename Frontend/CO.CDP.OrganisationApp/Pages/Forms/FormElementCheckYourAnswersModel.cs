using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class FormElementCheckYourAnswersModel : FormElementModel
{
    public Guid OrganisationId { get; set; }
    public Guid FormId { get; set; }
    public Guid SectionId { get; set; }

    public List<QuestionAnswer> Answers { get; private set; } = new();

    public override FormAnswer? GetAnswer()
    {
        return null;
    }

    public override void SetAnswer(FormAnswer? answer)
    {
    }

    public string GetEditUrl(Guid questionId)
    {
        return $"/organisation/{OrganisationId}/form/{FormId}/section/{SectionId}/question/{questionId}/edit";
    }

    public void LoadAnswers(ITempDataDictionary tempData)
    {
        var answerStateKey = $"Forms_{OrganisationId}_{FormId}_{SectionId}";
        var state = tempData.Peek(answerStateKey) as FormQuestionAnswerState;
        if (state != null)
        {
            Answers = state.Answers;
        }
    }
}
