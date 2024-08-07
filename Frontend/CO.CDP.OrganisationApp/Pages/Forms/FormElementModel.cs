using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public interface IFormElementModel
{
    string? Heading { get; set; }

    string? Description { get; set; }

    bool IsRequired { get; set; }

    FormQuestionType? CurrentFormQuestionType { get; set; }

    void Initialize(FormQuestion question);

    FormAnswer? GetAnswer();

    void SetAnswer(FormAnswer? answer);
}

public abstract class FormElementModel : IFormElementModel
{
    public string? Heading { get; set; }

    public string? Description { get; set; }

    [BindProperty]
    public FormQuestionType? CurrentFormQuestionType { get; set; }

    [BindProperty]
    public bool IsRequired { get; set; }

    [BindProperty]
    public FormQuestionOptions? Options { get; set; }

    public virtual void Initialize(FormQuestion question)
    {
        Heading = question.Title;
        Description = question.Description;
        CurrentFormQuestionType = question.Type;
        IsRequired = question.IsRequired;
        Options = question.Options;
    }

    public abstract FormAnswer? GetAnswer();

    public abstract void SetAnswer(FormAnswer? answer);
}