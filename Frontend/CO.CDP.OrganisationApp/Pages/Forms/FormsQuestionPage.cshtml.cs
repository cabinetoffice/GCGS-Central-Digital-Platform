using CO.CDP.AwsServices;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static System.Net.Mime.MediaTypeNames;

namespace CO.CDP.OrganisationApp.Pages.Forms;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class FormsQuestionPageModel(
    IFormsEngine formsEngine,
    ITempDataService tempDataService,
    IFileHostManager fileHostManager,
    IChoiceProviderService choiceProviderService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid OrganisationId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid FormId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid SectionId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid CurrentQuestionId { get; set; }

    [BindProperty(SupportsGet = true, Name = "frm-chk-answer")]
    public bool? RedirectFromCheckYourAnswerPage { get; set; }

    [BindProperty]
    public FormElementNoInputModel? NoInputModel { get; set; }
    [BindProperty]
    public FormElementTextInputModel? TextInputModel { get; set; }
    [BindProperty]
    public FormElementFileUploadModel? FileUploadModel { get; set; }
    [BindProperty]
    public FormElementYesNoInputModel? YesNoInputModel { get; set; }
    [BindProperty]
    public FormElementSingleChoiceModel? SingleChoiceModel { get; set; }
    [BindProperty]
    public FormElementGroupedSingleChoiceModel? GroupedSingleChoiceModel { get; set; }
    [BindProperty]
    public FormElementDateInputModel? DateInputModel { get; set; }
    [BindProperty]
    public FormElementCheckBoxInputModel? CheckBoxModel { get; set; }
    [BindProperty]
    public FormElementAddressModel? AddressModel { get; set; }
    [BindProperty]
    public FormElementMultiLineInputModel? MultiLineInputModel { get; set; }
    [BindProperty]
    public FormElementUrlInputModel? UrlInputModel { get; set; }
    [BindProperty(SupportsGet = true)]
    public string? UkOrNonUk { get; set; }
    public FormQuestionType? CurrentFormQuestionType { get; private set; }
    public string? PartialViewName { get; private set; }
    public IFormElementModel? PartialViewModel { get; private set; }
    public FormQuestion? PreviousQuestion { get; private set; }
    public Guid? CheckYourAnswerQuestionId { get; private set; }
    public FormSectionType? FormSectionType { get; set; }

    public string EncType => CurrentFormQuestionType == FormQuestionType.FileUpload
        ? Multipart.FormData : Application.FormUrlEncoded;

    private string FormQuestionAnswerStateKey => $"Form_{OrganisationId}_{FormId}_{SectionId}_Answers";

    public async Task<IActionResult> OnGetAsync()
    {
        var previousUnansweredQuestionId = await ValidateIfNeedRedirect();
        if (previousUnansweredQuestionId != null)
        {
            return RedirectToPage("FormsQuestionPage", new { OrganisationId, FormId, SectionId, CurrentQuestionId = previousUnansweredQuestionId });
        }

        var currentQuestion = await InitModel(true);
        if (currentQuestion == null)
        {
            return Redirect("/page-not-found");
        }


        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var previousUnansweredQuestionId = await ValidateIfNeedRedirect();
        if (previousUnansweredQuestionId != null)
        {
            return RedirectToPage("FormsQuestionPage", new { OrganisationId, FormId, SectionId, CurrentQuestionId = previousUnansweredQuestionId });
        }

        var currentQuestion = await InitModel();
        if (currentQuestion == null)
        {
            return Redirect("/page-not-found");
        }

        if (PartialViewModel != null)
        {
            ModelState.Clear();
            if (!TryValidateModel(PartialViewModel))
            {
                return Page();
            }

            var answer = PartialViewModel.GetAnswer();

            if (PartialViewModel.CurrentFormQuestionType == FormQuestionType.FileUpload)
            {
                var response = FileUploadModel?.GetUploadedFileInfo();
                if (response != null)
                {
                    using var stream = response.Value.formFile.OpenReadStream();
                    await fileHostManager.UploadFile(stream, response.Value.filename, response.Value.contentType);
                    answer ??= new FormAnswer();
                    answer.TextValue = response.Value.filename;
                }
            }

            SaveAnswerToTempData(currentQuestion, answer);
        }

        if (currentQuestion.Id == CheckYourAnswerQuestionId)
        {
            var answerSet = tempDataService.PeekOrDefault<FormQuestionAnswerState>(FormQuestionAnswerStateKey);
            await formsEngine.SaveUpdateAnswers(FormId, SectionId, OrganisationId, answerSet);

            tempDataService.Remove(FormQuestionAnswerStateKey);

            if (FormSectionType == Models.FormSectionType.Declaration)
            {
                var shareCode = await formsEngine.CreateShareCodeAsync(FormId, OrganisationId);

                return RedirectToPage("/ShareInformation/ShareCodeConfirmation",
                            new { OrganisationId, FormId, SectionId, shareCode });
            }

            return RedirectToPage("FormsAnswerSetSummary", new { OrganisationId, FormId, SectionId });
        }

        Guid? nextQuestionId;

        if (RedirectFromCheckYourAnswerPage == true)
        {
            nextQuestionId = CheckYourAnswerQuestionId;
        }
        else
        {
            nextQuestionId = (await formsEngine.GetNextQuestion(OrganisationId, FormId, SectionId, currentQuestion.Id))?.Id;
        }

        return RedirectToPage("FormsQuestionPage", new { OrganisationId, FormId, SectionId, CurrentQuestionId = nextQuestionId });
    }

    public async Task<IEnumerable<AnswerSummary>> GetAnswers()
    {
        var answerSet = tempDataService.PeekOrDefault<FormQuestionAnswerState>(FormQuestionAnswerStateKey);

        var form = await formsEngine.GetFormSectionAsync(OrganisationId, FormId, SectionId);

        List<AnswerSummary> summaryList = [];
        foreach (QuestionAnswer answer in answerSet.Answers)
        {
            var question = form?.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);

            if (question != null && question.Type != FormQuestionType.NoInput && question.Type != FormQuestionType.CheckYourAnswers)
            {
                var summary = new AnswerSummary
                {
                    Title = question.SummaryTitle ?? question.Title,
                    Answer = await GetAnswerString(answer, question),
                    ChangeLink = $"/organisation/{OrganisationId}/forms/{FormId}/sections/{SectionId}/questions/{answer.QuestionId}?frm-chk-answer=true"
                };

                if (question.Type == FormQuestionType.Address && answer.Answer?.AddressValue != null
                    && answer.Answer.AddressValue.Country != Country.UKCountryCode)
                {
                    summary.ChangeLink += "&UkOrNonUk=non-uk";
                }

                summaryList.Add(summary);
            }
        }

        return summaryList;
    }

    private async Task<string> GetAnswerString(QuestionAnswer questionAnswer, FormQuestion question)
    {
        var answer = questionAnswer.Answer;
        if (answer == null)
            return "";

        async Task<string> singleChoiceString(FormAnswer a)
        {
            var choiceProviderStrategy = choiceProviderService.GetStrategy(question.Options.ChoiceProviderStrategy);
            return await choiceProviderStrategy.RenderOption(a) ?? "";
        }

        string boolAnswerString = answer.BoolValue.HasValue == true ? (answer.BoolValue == true ? "Yes" : "No") : "";

        string answerString = question.Type switch
        {
            FormQuestionType.Text => answer.TextValue ?? "",
            FormQuestionType.FileUpload => answer.TextValue ?? "",
            FormQuestionType.SingleChoice => await singleChoiceString(answer),
            FormQuestionType.Date => answer.DateValue.HasValue == true ? answer.DateValue.Value.ToString("dd/MM/yyyy") : "",
            FormQuestionType.CheckBox => answer.BoolValue == true ? question?.Options?.Choices?.Values.FirstOrDefault() ?? "" : "",
            FormQuestionType.Address => answer.AddressValue != null ? answer.AddressValue.ToHtmlString() : "",
            FormQuestionType.MultiLine => answer.TextValue ?? "",
            FormQuestionType.GroupedSingleChoice => answer.OptionValue ?? "",
            FormQuestionType.Url => answer.TextValue ?? "",
            _ => ""
        };

        string[] answers = [boolAnswerString, answerString];

        return string.Join(", ", answers.Where(s => !string.IsNullOrWhiteSpace(s)));
    }

    public bool PreviousQuestionHasNonUKAddressAnswer()
    {
        if (PreviousQuestion != null && PreviousQuestion.Type == FormQuestionType.Address)
        {
            var answer = GetAnswerFromTempData(PreviousQuestion);
            if (answer?.AddressValue != null)
            {
                return answer.AddressValue.Country != Constants.Country.UKCountryCode;
            }
        }

        return false;
    }

    private async Task<Guid?> ValidateIfNeedRedirect()
    {
        var form = await formsEngine.GetFormSectionAsync(OrganisationId, FormId, SectionId);

        var answerState = tempDataService.PeekOrDefault<FormQuestionAnswerState>(FormQuestionAnswerStateKey);

        var previousUnansweredQuestionId = formsEngine.GetPreviousUnansweredQuestionId(form.Questions, answerState);

        if (previousUnansweredQuestionId != null && previousUnansweredQuestionId != CurrentQuestionId)
        {
            return previousUnansweredQuestionId;
        }

        return null;
    }

    private async Task<FormQuestion?> InitModel(bool reset = false)
    {
        var form = await formsEngine.GetFormSectionAsync(OrganisationId, FormId, SectionId);

        var currentQuestion = await formsEngine.GetCurrentQuestion(OrganisationId, FormId, SectionId, CurrentQuestionId);

        if (currentQuestion == null)
            return null;

        FormSectionType = form.Section?.Type;
        CurrentFormQuestionType = currentQuestion.Type;
        PartialViewName = GetPartialViewName(currentQuestion);
        PartialViewModel = GetPartialViewModel(currentQuestion, reset);
        PreviousQuestion = await formsEngine.GetPreviousQuestion(OrganisationId, FormId, SectionId, currentQuestion.Id);
        CheckYourAnswerQuestionId = form.Questions.FirstOrDefault(q => q.Type == FormQuestionType.CheckYourAnswers)?.Id;

        return currentQuestion;
    }

    private static string? GetPartialViewName(FormQuestion question)
    {
        if (question.Type == FormQuestionType.CheckYourAnswers)
            return null;

        Dictionary<FormQuestionType, string> formQuestionPartials = new(){
            { FormQuestionType.NoInput, "_FormElementNoInput" },
            { FormQuestionType.Text, "_FormElementTextInput" },
            { FormQuestionType.FileUpload, "_FormElementFileUpload" },
            { FormQuestionType.YesOrNo, "_FormElementYesNoInput" },
            { FormQuestionType.SingleChoice, "_FormElementSingleChoice" },
            { FormQuestionType.Date, "_FormElementDateInput" },
            { FormQuestionType.CheckBox, "_FormElementCheckBoxInput" },
            { FormQuestionType.Address, "_FormElementAddress" },
            { FormQuestionType.MultiLine, "_FormElementMultiLineInput" },
            { FormQuestionType.GroupedSingleChoice, "_FormElementGroupedSingleChoice" },
            { FormQuestionType.Url, "_FormElementUrlInput" },
        };

        if (formQuestionPartials.TryGetValue(question.Type, out var partialView))
        {
            return partialView;
        }

        throw new NotImplementedException($"Forms question: {question.Type} is not supported");
    }

    private IFormElementModel? GetPartialViewModel(FormQuestion question, bool reset)
    {
        if (question.Type == FormQuestionType.CheckYourAnswers)
            return null;

        IFormElementModel model = question.Type switch
        {
            FormQuestionType.NoInput => NoInputModel ?? new FormElementNoInputModel(),
            FormQuestionType.Text => TextInputModel ?? new FormElementTextInputModel(),
            FormQuestionType.FileUpload => FileUploadModel ?? new FormElementFileUploadModel(),
            FormQuestionType.YesOrNo => YesNoInputModel ?? new FormElementYesNoInputModel(),
            FormQuestionType.Date => DateInputModel ?? new FormElementDateInputModel(),
            FormQuestionType.CheckBox => CheckBoxModel ?? new FormElementCheckBoxInputModel(),
            FormQuestionType.Address => AddressModel ?? new FormElementAddressModel(),
            FormQuestionType.SingleChoice => SingleChoiceModel ?? new FormElementSingleChoiceModel(),
            FormQuestionType.MultiLine => MultiLineInputModel ?? new FormElementMultiLineInputModel(),
            FormQuestionType.GroupedSingleChoice => GroupedSingleChoiceModel ?? new FormElementGroupedSingleChoiceModel(),
            FormQuestionType.Url => UrlInputModel ?? new FormElementUrlInputModel(),
            _ => throw new NotImplementedException($"Forms question: {question.Type} is not supported"),
        };

        model.Initialize(question);
        if (question.Type == FormQuestionType.Address && model is FormElementAddressModel addressModel)
        {
            addressModel.UkOrNonUk = UkOrNonUk ?? addressModel.UkOrNonUk;
        }

        if (reset)
            model.SetAnswer(GetAnswerFromTempData(question));

        return model;
    }

    private FormAnswer? GetAnswerFromTempData(FormQuestion question)
    {
        var state = tempDataService.PeekOrDefault<FormQuestionAnswerState>(FormQuestionAnswerStateKey);
        return state.Answers?.FirstOrDefault(a => a.QuestionId == question.Id)?.Answer;
    }

    private void SaveAnswerToTempData(FormQuestion question, FormAnswer? answer)
    {
        var state = tempDataService.PeekOrDefault<FormQuestionAnswerState>(FormQuestionAnswerStateKey);

        var questionAnswer = state.Answers.FirstOrDefault(a => a.QuestionId == question.Id);
        if (questionAnswer == null)
        {
            questionAnswer = new QuestionAnswer
            {
                QuestionId = question.Id,
                AnswerId = Guid.NewGuid()
            };
            state.Answers.Add(questionAnswer);
        }

        questionAnswer.Answer = answer;

        tempDataService.Put(FormQuestionAnswerStateKey, state);
    }
}

public class AnswerSummary
{
    public string? Title { get; set; }
    public required string Answer { get; set; }
    public string? ChangeLink { get; set; }
}