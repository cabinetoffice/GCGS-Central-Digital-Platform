using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public interface IFormElementModel
{
    string? Heading { get; set; }

    string? Description { get; set; }

    bool IsRequired { get; set; }

    FormQuestionType? FormQuestionType { get; set; }

    string? GetAnswer();

    void SetAnswer(string? answer);
}

public abstract class FormElementModel : IFormElementModel
{
    public string? Heading { get; set; }

    public string? Description { get; set; }

    [BindProperty]
    public FormQuestionType? FormQuestionType { get; set; }

    [BindProperty]
    public bool IsRequired { get; set; }

    public abstract string? GetAnswer();

    public abstract void SetAnswer(string? answer);
}