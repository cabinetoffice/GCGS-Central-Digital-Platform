using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public interface IFormElementModel
{
    string? Heading { get; set; }

    string? Description { get; set; }

    bool IsRequired { get; set; }

    FormQuestionType? CurrentFormQuestionType { get; set; }

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

    public abstract FormAnswer? GetAnswer();

    public abstract void SetAnswer(FormAnswer? answer);
}