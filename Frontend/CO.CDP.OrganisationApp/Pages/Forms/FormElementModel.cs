using CO.CDP.OrganisationApp.Models;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public interface IFormElementModel
{
    FormQuestionType? CurrentFormQuestionType { get; set; }

    void Initialize(FormQuestion question);

    FormAnswer? GetAnswer();

    void SetAnswer(FormAnswer? answer);
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