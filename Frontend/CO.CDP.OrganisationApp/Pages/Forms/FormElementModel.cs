using CO.CDP.OrganisationApp.Models;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public interface IFormElementModel
{
    FormQuestionType? CurrentFormQuestionType { get; set; }
    string? Heading { get; set; }

    void Initialize(FormQuestion question);

    FormAnswer? GetAnswer();

    void SetAnswer(FormAnswer? answer);
}

public interface IMultiQuestionFormElementModel
{
    List<FormQuestion> Questions { get; }
    string? PageTitleResourceKey { get; }
    string? SubmitButtonTextResourceKey { get; }

    void Initialize(MultiQuestionPageModel multiQuestionPage, Dictionary<Guid, FormAnswer> existingAnswers);

    IFormElementModel? GetQuestionModel(Guid questionId);

    Dictionary<Guid, FormAnswer> GetAllAnswers();
}

public abstract class FormElementModel : IFormElementModel
{
    public string? Heading { get; set; }

    public string? Description { get; set; }

    public string? Caption { get; set; }

    public FormQuestionType? CurrentFormQuestionType { get; set; }

    public bool IsRequired { get; set; }

    public FormQuestionOptions? Options { get; set; }

    public virtual void Initialize(FormQuestion question)
    {
        Heading = question.Title;
        Description = question.Description;
        Caption = question.Caption;
        CurrentFormQuestionType = question.Type;
        IsRequired = question.IsRequired;
        Options = question.Options;
    }

    public abstract FormAnswer? GetAnswer();

    public abstract void SetAnswer(FormAnswer? answer);
}