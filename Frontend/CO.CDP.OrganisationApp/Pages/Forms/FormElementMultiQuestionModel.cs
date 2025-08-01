using CO.CDP.OrganisationApp.Models;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class RenderableQuestionItem
{
    public IFormElementModel QuestionModel { get; init; } = null!;
    public string PartialViewName { get; init; } = string.Empty;
    public bool IsFirstQuestion { get; init; }
}

public class FormElementMultiQuestionModel : IMultiQuestionFormElementModel
{
    public List<FormQuestion> Questions { get; private set; } = [];
    public string? PageTitleResourceKey { get; private set; }
    public string? SubmitButtonTextResourceKey { get; private set; }

    private readonly Dictionary<Guid, IFormElementModel> _questionModels = new();
    public IEnumerable<IFormElementModel> QuestionModels => _questionModels.Values;

    public void Initialize(MultiQuestionPageModel multiQuestionPage, Dictionary<Guid, FormAnswer> existingAnswers)
    {
        Questions = multiQuestionPage.Questions;
        PageTitleResourceKey = multiQuestionPage.PageTitleResourceKey;
        SubmitButtonTextResourceKey = multiQuestionPage.SubmitButtonTextResourceKey;

        foreach (var question in Questions)
        {
            var questionModel = CreateQuestionModel(question);
            if (questionModel is FormElementModel fem)
            {
                fem.QuestionId = question.Id;
            }
            questionModel.Initialize(question, Questions.IndexOf(question) == 0);

            if (existingAnswers.TryGetValue(question.Id, out var existingAnswer))
            {
                questionModel.SetAnswer(existingAnswer);
            }

            _questionModels[question.Id] = questionModel;
        }
    }

    public IFormElementModel? GetQuestionModel(Guid questionId)
    {
        return _questionModels.GetValueOrDefault(questionId);
    }

    public Dictionary<Guid, FormAnswer> GetAllAnswers()
    {
        var answers = new Dictionary<Guid, FormAnswer>();

        foreach (var (questionId, questionModel) in _questionModels)
        {
            var answer = questionModel.GetAnswer();
            if (answer != null)
            {
                answers[questionId] = answer;
            }
        }

        return answers;
    }

    public IEnumerable<RenderableQuestionItem> GetRenderableQuestions()
    {
        return Questions
            .Select((question, index) => new RenderableQuestionItem
            {
                QuestionModel = GetQuestionModel(question.Id)!,
                PartialViewName = GetPartialViewName(question.Type),
                IsFirstQuestion = index == 0
            })
            .Where(item => !string.IsNullOrEmpty(item.PartialViewName));
    }

    private static string GetPartialViewName(FormQuestionType questionType) =>
        questionType switch
        {
            FormQuestionType.Text => "_FormElementTextInput",
            FormQuestionType.YesOrNo => "_FormElementYesNoInput",
            FormQuestionType.Date => "_FormElementDateInput",
            FormQuestionType.SingleChoice => "_FormElementSingleChoice",
            FormQuestionType.GroupedSingleChoice => "_FormElementGroupedSingleChoice",
            FormQuestionType.MultiLine => "_FormElementMultiLineInput",
            FormQuestionType.Url => "_FormElementUrlInput",
            FormQuestionType.Address => "_FormElementAddress",
            FormQuestionType.CheckBox => "_FormElementCheckBoxInput",
            FormQuestionType.FileUpload => "_FormElementFileUpload",
            FormQuestionType.NoInput => "_FormElementNoInput",
            _ => throw new NotSupportedException($"Question type {questionType} is not supported in multi-question pages")
        };

    private static IFormElementModel CreateQuestionModel(FormQuestion question)
    {
        return question.Type switch
        {
            FormQuestionType.Text => new FormElementTextInputModel(),
            FormQuestionType.YesOrNo => new FormElementYesNoInputModel(),
            FormQuestionType.Date => new FormElementDateInputModel(),
            FormQuestionType.SingleChoice => new FormElementSingleChoiceModel(),
            FormQuestionType.GroupedSingleChoice => new FormElementGroupedSingleChoiceModel(),
            FormQuestionType.MultiLine => new FormElementMultiLineInputModel(),
            FormQuestionType.Url => new FormElementUrlInputModel(),
            FormQuestionType.Address => new FormElementAddressModel(),
            FormQuestionType.CheckBox => new FormElementCheckBoxInputModel(),
            FormQuestionType.FileUpload => new FormElementFileUploadModel(),
            FormQuestionType.NoInput => new FormElementNoInputModel(),
            _ => throw new NotSupportedException($"Question type {question.Type} is not supported in multi-question pages")
        };
    }
}