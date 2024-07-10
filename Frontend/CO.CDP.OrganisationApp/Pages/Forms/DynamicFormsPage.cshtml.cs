using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class DynamicFormsPageModel(IFormsEngine formsEngine) : PageModel
{
    public SectionQuestionsResponse? SectionWithQuestions { get; set; }

    private static readonly Dictionary<FormQuestionType, string> FormQuestionPartials = new()
    {
        { FormQuestionType.NoInput, "_FormElementNoInput" },
        { FormQuestionType.YesOrNo, "_FormElementYesNoInput" },
        { FormQuestionType.FileUpload, "_FormElementFileUpload" },
        { FormQuestionType.Date, "_FormElementDateInput" }
    };

    public async Task OnGetAsync(Guid formId, Guid sectionId)
    {
        SectionWithQuestions = await formsEngine.LoadFormSectionAsync(formId, sectionId);
    }

    public IActionResult OnPost()
    {
        return RedirectToPage("/Forms/Confirmation");
    }

    public string? GetPartialViewName(FormQuestionType questionType)
    {
        return FormQuestionPartials.TryGetValue(questionType, out var partialView) ? partialView : null;
    }
}
