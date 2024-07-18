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

    public FormQuestion? CurrentQuestion { get; set; }

    public IFormElementModel? FormElementModel { get; set; }

    public Guid? PreviousQuestionId { get; private set; }

    public string EncType => CurrentQuestion?.Type == FormQuestionType.FileUpload ? "multipart/form-data" : "application/x-www-form-urlencoded";

    private string FormQuestionAnswerStateKey => $"Forms_{OrganisationId}_{FormId}_{SectionId}";

    public async Task<IActionResult> OnGetAsync()
    {
        await InitModel(true);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var success = await InitModel();
        if (!success || !ModelState.IsValid)
        {
            return Page();
        }

        var nextQuestion = await formsEngine.GetNextQuestion(FormId, SectionId, CurrentQuestionId);
        if (nextQuestion != null)
        {
            return RedirectToPage("DynamicFormsPage", new { OrganisationId, FormId, SectionId, CurrentQuestionId = nextQuestion.Id });
        }

        return Page();
    }

    private async Task<bool> InitModel(bool reset = false)
    {
        CurrentQuestion = await formsEngine.GetCurrentQuestion(FormId, SectionId, CurrentQuestionId);

        if (CurrentQuestion != null)
        {
            await SetPartialViewModel(CurrentQuestion.Type, reset);

            PreviousQuestionId = (await formsEngine.GetPreviousQuestion(FormId, SectionId, CurrentQuestionId))?.Id;

            return true;
        }

        return false;
    }

    public (string?, IFormElementModel?) GetPartialView(FormQuestionType questionType)
    {
        Dictionary<FormQuestionType, string> formQuestionPartials = new(){
            { FormQuestionType.NoInput, "_FormElementNoInput" },
            { FormQuestionType.YesOrNo, "_FormElementYesNoInput" },
            { FormQuestionType.FileUpload, "_FormElementFileUpload" },
            { FormQuestionType.Date, "_FormElementDateInput" },
            { FormQuestionType.Text, "_FormElementTextInput" }
        };

        if (formQuestionPartials.TryGetValue(questionType, out var partialView))
        {
            return (partialView, FormElementModel);
        }
        else
        {
            return (null, FormElementModel);
        }
    }

    private async Task SetPartialViewModel(FormQuestionType questionType, bool reset = false)
    {
        IFormElementModel model = questionType switch
        {
            FormQuestionType.NoInput => NoInputModel ?? new FormElementNoInputModel(),
            FormQuestionType.Text => TextInputModel ?? new FormElementTextInputModel(),
            FormQuestionType.FileUpload => FileUploadModel ?? new FormElementFileUploadModel(),
            FormQuestionType.YesOrNo => YesNoInputModel ?? new FormElementYesNoInputModel(),
            FormQuestionType.Date => DateInputModel ?? new FormElementDateInputModel(),
            _ => throw new NotImplementedException(),
        };

        model.FormQuestionType = questionType;
        model.Heading = CurrentQuestion!.Title;
        model.Description = CurrentQuestion.Description;
        model.IsRequired = CurrentQuestion.IsRequired;

        if (reset)
        {
            model.SetAnswer(RetrieveAnswerFromTempData());
        }
        else
        {
            var answer = model.GetAnswer();

            if (questionType == FormQuestionType.FileUpload)
            {
                var response = FileUploadModel?.GetUploadedFileInfo();
                if (response != null)
                {
                    using var stream = response.Value.formFile.OpenReadStream();
                    await fileHostManager.UploadFile(stream, response.Value.filename, response.Value.contentType);
                    answer = new FormAnswer { TextValue = response.Value.filename };
                }
            }

            SaveAnswerToTempData(answer);
        }

        FormElementModel = model;
    }

    private void SaveAnswerToTempData(FormAnswer? answer)
    {
        if (CurrentQuestion != null)
        {
            var state = tempDataService.PeekOrDefault<FormQuestionAnswerState>(FormQuestionAnswerStateKey);

            var questionAnswer = state.Answers.FirstOrDefault(a => a.QuestionId == CurrentQuestion.Id);
            if (questionAnswer == null)
            {
                questionAnswer = new QuestionAnswer { QuestionId = CurrentQuestion.Id };
                state.Answers.Add(questionAnswer);
            }

            questionAnswer.Answer = answer;

            tempDataService.Put(FormQuestionAnswerStateKey, state);
        }
    }

    private FormAnswer? RetrieveAnswerFromTempData()
    {
        var state = tempDataService.PeekOrDefault<FormQuestionAnswerState>(FormQuestionAnswerStateKey);
        return state.Answers?.FirstOrDefault(a => a.QuestionId == CurrentQuestion?.Id)?.Answer;
    }
}