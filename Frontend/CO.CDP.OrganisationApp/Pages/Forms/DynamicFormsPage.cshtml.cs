using CO.CDP.AwsServices;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class DynamicFormsPageModel(
    IFormsEngine formsEngine,
    ITempDataService tempDataService,
    IFileHostManager fileHostManager) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid OrganisationId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid FormId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid SectionId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? CurrentQuestionId { get; set; }

    [BindProperty]
    public FormElementDateInputModel? DateInputModel { get; set; }

    [BindProperty]
    public FormElementFileUploadModel? FileUploadModel { get; set; }

    [BindProperty]
    public FormElementNoInputModel? NoInputModel { get; set; }

    [BindProperty]
    public FormElementTextInputModel? TextInputModel { get; set; }

    [BindProperty]
    public FormElementYesNoInputModel? YesNoInputModel { get; set; }

    public string? PartialViewName { get; private set; }

    public IFormElementModel? PartialViewModel { get; private set; }

    public Guid? PreviousQuestionId { get; private set; }

    public string EncType => PartialViewModel?.CurrentFormQuestionType == FormQuestionType.FileUpload
        ? "multipart/form-data" : "application/x-www-form-urlencoded";

    private string FormQuestionAnswerStateKey => $"Form_{OrganisationId}_{FormId}_{SectionId}_Answers";

    public async Task<IActionResult> OnGetAsync()
    {
        var currentQuestion = await InitModel(true);
        if (currentQuestion == null)
        {
            return Redirect("/page-not-found");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var currentQuestion = await InitModel();
        if (currentQuestion == null || PartialViewModel == null)
        {
            return Redirect("/page-not-found");
        }

        ModelState.Clear();
        if (!TryValidateModel(this))
        {
            return Page();
        }

        if (PartialViewModel != null)
        {
            var answer = PartialViewModel.GetAnswer();

            if (PartialViewModel.CurrentFormQuestionType == FormQuestionType.FileUpload)
            {
                var response = FileUploadModel?.GetUploadedFileInfo();
                if (response != null)
                {
                    using var stream = response.Value.formFile.OpenReadStream();
                    await fileHostManager.UploadFile(stream, response.Value.filename, response.Value.contentType);
                    answer = new FormAnswer { TextValue = response.Value.filename };
                }
            }

            SaveAnswerToTempData(currentQuestion, answer);
        }

        var nextQuestion = await formsEngine.GetNextQuestion(OrganisationId, FormId, SectionId, CurrentQuestionId);
        if (nextQuestion != null)
        {
            return RedirectToPage("DynamicFormsPage", new { OrganisationId, FormId, SectionId, CurrentQuestionId = nextQuestion.Id });
        }

        return Page();
    }

    private async Task<FormQuestion?> InitModel(bool reset = false)
    {
        var currentQuestion = await formsEngine.GetCurrentQuestion(OrganisationId, FormId, SectionId, CurrentQuestionId);
        if (currentQuestion == null) return null;

        PartialViewName = GetPartialViewName(currentQuestion);

        PartialViewModel = GetPartialViewModel(currentQuestion, reset);

        PreviousQuestionId = (await formsEngine.GetPreviousQuestion(OrganisationId, FormId, SectionId, CurrentQuestionId))?.Id;

        return currentQuestion;
    }

    private static string GetPartialViewName(FormQuestion currentQuestion)
    {
        Dictionary<FormQuestionType, string> formQuestionPartials = new(){
            { FormQuestionType.NoInput, "_FormElementNoInput" },
            { FormQuestionType.YesOrNo, "_FormElementYesNoInput" },
            { FormQuestionType.FileUpload, "_FormElementFileUpload" },
            { FormQuestionType.Date, "_FormElementDateInput" },
            { FormQuestionType.Text, "_FormElementTextInput" }
        };

        if (formQuestionPartials.TryGetValue(currentQuestion.Type, out var partialView))
        {
            return partialView;
        }

        throw new NotImplementedException($"Forms question: {currentQuestion.Type} is not supported");
    }

    private IFormElementModel GetPartialViewModel(FormQuestion currentQuestion, bool reset)
    {
        IFormElementModel model = currentQuestion.Type switch
        {
            FormQuestionType.NoInput => NoInputModel ?? new FormElementNoInputModel(),
            FormQuestionType.Text => TextInputModel ?? new FormElementTextInputModel(),
            FormQuestionType.FileUpload => FileUploadModel ?? new FormElementFileUploadModel(),
            FormQuestionType.YesOrNo => YesNoInputModel ?? new FormElementYesNoInputModel(),
            FormQuestionType.Date => DateInputModel ?? new FormElementDateInputModel(),
            _ => throw new NotImplementedException($"Forms question: {currentQuestion.Type} is not supported"),
        };

        model.CurrentFormQuestionType = currentQuestion.Type;
        model.Heading = currentQuestion.Title;
        model.Description = currentQuestion.Description;
        model.IsRequired = currentQuestion.IsRequired;

        if (reset)
        {
            var state = tempDataService.PeekOrDefault<FormQuestionAnswerState>(FormQuestionAnswerStateKey);
            var answer = state.Answers?.FirstOrDefault(a => a.QuestionId == currentQuestion.Id)?.Answer;
            model.SetAnswer(answer);
        }

        return model;
    }

    private void SaveAnswerToTempData(FormQuestion question, FormAnswer? answer)
    {
        var state = tempDataService.PeekOrDefault<FormQuestionAnswerState>(FormQuestionAnswerStateKey);

        var questionAnswer = state.Answers.FirstOrDefault(a => a.QuestionId == question.Id);
        if (questionAnswer == null)
        {
            questionAnswer = new QuestionAnswer { QuestionId = question.Id };
            state.Answers.Add(questionAnswer);
        }

        questionAnswer.Answer = answer;

        tempDataService.Put(FormQuestionAnswerStateKey, state);
    }
}