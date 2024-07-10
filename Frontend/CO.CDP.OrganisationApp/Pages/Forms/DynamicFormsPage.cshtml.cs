using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class DynamicFormsPageModel(IFormsEngine formsEngine, ITempDataService tempDataService) : PageModel
{
    public SectionQuestionsResponse? SectionWithQuestions { get; set; }
    public Models.FormQuestion? CurrentQuestion { get; set; }
    public Guid FormId { get; set; }
    public Guid SectionId { get; set; }
    public bool IsFirstQuestion => CurrentQuestion?.Id == SectionWithQuestions?.Questions.FirstOrDefault()?.Id;
    public Guid? PreviousQuestionId { get; private set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    private static readonly Dictionary<FormQuestionType, string> FormQuestionPartials = new()
    {
        { FormQuestionType.NoInput, "_FormElementNoInput" },
        { FormQuestionType.YesOrNo, "_FormElementYesNoInput" },
        { FormQuestionType.FileUpload, "_FormElementFileUpload" },
        { FormQuestionType.Date, "_FormElementDateInput" }
    };

    public async Task OnGetAsync(Guid formId, Guid sectionId, Guid? questionId)
    {
        FormId = formId;
        SectionId = sectionId;
        SectionWithQuestions = await formsEngine.LoadFormSectionAsync(formId, sectionId);

        if (questionId.HasValue)
        {
            CurrentQuestion = await formsEngine.GetCurrentQuestion(formId, sectionId, questionId.Value);
        }
        else
        {
            CurrentQuestion = SectionWithQuestions.Questions.FirstOrDefault();
        }

        SaveCurrentQuestionIdToTempData(CurrentQuestion?.Id);
        SetPreviousQuestionId();
    }

    public async Task<IActionResult> OnPostAsync(Guid formId, Guid sectionId, Guid currentQuestionId, string action)
    {
        FormId = formId;
        SectionId = sectionId;
        SectionWithQuestions = await formsEngine.LoadFormSectionAsync(formId, sectionId);

        if (action == "next")
        {
            CurrentQuestion = await formsEngine.GetNextQuestion(FormId, SectionId, currentQuestionId);
        }
        else if (action == "back")
        {
            CurrentQuestion = await formsEngine.GetPreviousQuestion(FormId, SectionId, currentQuestionId);
        }

        SaveCurrentQuestionIdToTempData(CurrentQuestion?.Id);
        SetPreviousQuestionId();
        return Page();
    }

    private void SaveCurrentQuestionIdToTempData(Guid? questionId)
    {
        var key = $"CurrentQuestionId_{FormId}_{SectionId}";
        tempDataService.Put(key, questionId);
    }

    private Guid GetCurrentQuestionIdFromTempData()
    {
        var key = $"CurrentQuestionId_{FormId}_{SectionId}";
        var questionId = tempDataService.Get<Guid>(key);
        return questionId;
    }

    private void SetPreviousQuestionId()
    {
        if (CurrentQuestion == null || SectionWithQuestions?.Questions == null)
        {
            PreviousQuestionId = null;
            return;
        }

        var currentIndex = SectionWithQuestions.Questions.FindIndex(q => q.Id == CurrentQuestion.Id);

        if (currentIndex > 0)
        {
            PreviousQuestionId = SectionWithQuestions.Questions[currentIndex - 1].Id;
        }
        else
        {
            PreviousQuestionId = null;
        }
    }

    public string? GetPartialViewName(FormQuestionType questionType)
    {
        return FormQuestionPartials.TryGetValue(questionType, out var partialView) ? partialView : null;
    }
}