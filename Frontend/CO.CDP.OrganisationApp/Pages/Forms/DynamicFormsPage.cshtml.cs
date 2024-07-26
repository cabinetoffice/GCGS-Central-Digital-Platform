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

    public FormQuestionType? CurrentFormQuestionType { get; private set; }

    public string? PartialViewName { get; private set; }

    public IFormElementModel? PartialViewModel { get; private set; }

    public Guid? PreviousQuestionId { get; private set; }

    [BindProperty]
    public bool? RedirectToCheckYourAnswer { get; set; }

    public string EncType => CurrentFormQuestionType == FormQuestionType.FileUpload
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
        if (currentQuestion == null)
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

        var checkYourAnswerQuestionId = await CheckYourAnswerQuestionId();
        if (currentQuestion.Id == checkYourAnswerQuestionId)
        {
            // TODO: Call API Save

            return RedirectToPage("FormsAddAnotherAnswerSet", new { OrganisationId, FormId, SectionId });
        }

        Guid? nextQuestionId;
        if (RedirectToCheckYourAnswer == true)
        {
            nextQuestionId = await CheckYourAnswerQuestionId();
        }
        else
        {
            nextQuestionId = (await formsEngine.GetNextQuestion(OrganisationId, FormId, SectionId, currentQuestion.Id))?.Id;
        }

        if (nextQuestionId != null)
        {
            return RedirectToPage("DynamicFormsPage", new { OrganisationId, FormId, SectionId, CurrentQuestionId = nextQuestionId });
        }

        return Page();
    }

    public async Task<IEnumerable<AnswerSummary>> GetAnswers()
    {
        var answerSet = tempDataService.PeekOrDefault<FormQuestionAnswerState>(FormQuestionAnswerStateKey);

        var form = await formsEngine.LoadFormSectionAsync(OrganisationId, FormId, SectionId);

        List<AnswerSummary> summaryList = new();
        foreach (var answer in answerSet.Answers)
        {
            var question = form.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
            if (question != null && question.Type != FormQuestionType.NoInput && question.Type != FormQuestionType.CheckYourAnswers)
            {
                var answerString = question.Type switch
                {
                    FormQuestionType.Text => answer.Answer?.TextValue ?? "",
                    FormQuestionType.FileUpload => answer.Answer?.TextValue ?? "",
                    FormQuestionType.YesOrNo => question.IsRequired && answer.Answer?.BoolValue.HasValue == true ? (answer.Answer?.BoolValue == true ? "yes" : "no") : "",
                    FormQuestionType.Date => question.IsRequired && answer.Answer?.DateValue.HasValue == true ? answer.Answer?.DateValue.Value.ToString("dd/MM/yyyy") : "",
                    _ => ""
                };

                summaryList.Add(new AnswerSummary
                {
                    QuestionId = answer.QuestionId,
                    Title = question.Title,
                    Answer = answerString ?? ""
                });
            }
        }

        return summaryList;
    }

    public async Task<Guid?> CheckYourAnswerQuestionId()
    {
        var form = await formsEngine.LoadFormSectionAsync(OrganisationId, FormId, SectionId);

        return form.Questions.FirstOrDefault(q => q.Type == FormQuestionType.CheckYourAnswers)?.Id;
    }

    private async Task<FormQuestion?> InitModel(bool reset = false)
    {
        var currentQuestion = await formsEngine.GetCurrentQuestion(OrganisationId, FormId, SectionId, CurrentQuestionId);
        if (currentQuestion == null)
            return null;

        CurrentFormQuestionType = currentQuestion.Type;
        PartialViewName = GetPartialViewName(currentQuestion);
        PartialViewModel = GetPartialViewModel(currentQuestion, reset);
        PreviousQuestionId = (await formsEngine.GetPreviousQuestion(OrganisationId, FormId, SectionId, currentQuestion.Id))?.Id;

        return currentQuestion;
    }

    private static string? GetPartialViewName(FormQuestion currentQuestion)
    {
        if (currentQuestion.Type == FormQuestionType.CheckYourAnswers)
            return null;

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

    private IFormElementModel? GetPartialViewModel(FormQuestion currentQuestion, bool reset)
    {
        if (currentQuestion.Type == FormQuestionType.CheckYourAnswers)
            return null;

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
        if (question != null)
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
}

public class AnswerSummary
{
    public required Guid QuestionId { get; set; }
    public required string Answer { get; set; }
    public string? Title { get; set; }
}