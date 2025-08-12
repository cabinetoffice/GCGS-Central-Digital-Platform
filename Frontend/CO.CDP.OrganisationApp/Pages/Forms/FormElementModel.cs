using CO.CDP.OrganisationApp.Models;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public interface IFormElementModel
{
    Guid? QuestionId { get; set; }
    FormQuestionType? CurrentFormQuestionType { get; set; }
    string? Heading { get; set; }
    FormQuestionOptions? Options { get; set; }

    void Initialize(FormQuestion question, bool isFirst);

    FormAnswer? GetAnswer();

    void SetAnswer(FormAnswer? answer);
}

public interface IMultiQuestionFormElementModel
{
    List<FormQuestion> Questions { get; }
    IEnumerable<IFormElementModel> QuestionModels { get; }
    ButtonOptions? Button { get; }

    void Initialize(MultiQuestionPageModel multiQuestionPage, Dictionary<Guid, FormAnswer> existingAnswers);

    IFormElementModel? GetQuestionModel(Guid questionId);

    Dictionary<Guid, FormAnswer> GetAllAnswers();

    IEnumerable<RenderableQuestionItem> GetRenderableQuestions();
}

public abstract class FormElementModel : IFormElementModel
{
    public Guid? QuestionId { get; set; }

    public string? Heading { get; set; }

    public string? Description { get; set; }

    public string? Caption { get; set; }

    public FormQuestionType? CurrentFormQuestionType { get; set; }

    public bool IsRequired { get; set; }

    public FormQuestionOptions? Options { get; set; }

    public bool IsFirstQuestion { get; set; }

    public string GetFieldName(string propertyName)
    {
        return QuestionId.HasValue ? $"Q_{QuestionId.Value}_{propertyName}" : propertyName;
    }

    public string? GetValidationMessage(string propertyName, Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState)
    {
        var fieldName = GetFieldName(propertyName);
        if (modelState.TryGetValue(fieldName, out var modelStateEntry) && modelStateEntry.Errors.Count > 0)
        {
            return modelStateEntry.Errors[0].ErrorMessage;
        }

        return null;
    }

    public bool HasValidationError(string propertyName, Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState)
    {
        var fieldName = GetFieldName(propertyName);
        return modelState.TryGetValue(fieldName, out var modelStateEntry) && modelStateEntry.Errors.Count > 0;
    }

    public virtual void Initialize(FormQuestion question, bool isFirst)
    {
        Heading = question.Title;
        Description = question.Description;
        Caption = question.Caption;
        CurrentFormQuestionType = question.Type;
        IsRequired = question.IsRequired;
        Options = question.Options;
        IsFirstQuestion = isFirst;
    }

    public abstract FormAnswer? GetAnswer();

    public abstract void SetAnswer(FormAnswer? answer);
}