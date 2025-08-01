using CO.CDP.AwsServices;
using CO.CDP.Localization;
using CO.CDP.MQ;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using static System.Net.Mime.MediaTypeNames;
using OrganisationType = CO.CDP.Organisation.WebApiClient.OrganisationType;
using WebApiClientOrganisation = CO.CDP.Organisation.WebApiClient.Organisation;
using Address = CO.CDP.OrganisationApp.Models.Address;

namespace CO.CDP.OrganisationApp.Pages.Forms;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class FormsQuestionPageModel(
    IPublisher publisher,
    IFormsEngine formsEngine,
    ITempDataService tempDataService,
    IFileHostManager fileHostManager,
    IChoiceProviderService choiceProviderService,
    IOrganisationClient organisationClient,
    IUserInfoService userInfoService,
    IStringLocalizer<StaticTextResource> localizer) : PageModel
{
    [BindProperty(SupportsGet = true)] public Guid OrganisationId { get; set; }
    [BindProperty(SupportsGet = true)] public Guid FormId { get; set; }
    [BindProperty(SupportsGet = true)] public Guid SectionId { get; set; }
    [BindProperty(SupportsGet = true)] public Guid CurrentQuestionId { get; set; }

    [BindProperty(SupportsGet = true, Name = "frm-chk-answer")]
    public bool? RedirectFromCheckYourAnswerPage { get; set; }

    [BindProperty(SupportsGet = true)] public string? UkOrNonUk { get; set; }
    [BindProperty] public FormElementNoInputModel? NoInputModel { get; set; }
    [BindProperty] public FormElementTextInputModel? TextInputModel { get; set; }
    [BindProperty] public FormElementFileUploadModel? FileUploadModel { get; set; }
    [BindProperty] public FormElementYesNoInputModel? YesNoInputModel { get; set; }
    [BindProperty] public FormElementSingleChoiceModel? SingleChoiceModel { get; set; }
    [BindProperty] public FormElementGroupedSingleChoiceModel? GroupedSingleChoiceModel { get; set; }
    [BindProperty] public FormElementDateInputModel? DateInputModel { get; set; }
    [BindProperty] public FormElementCheckBoxInputModel? CheckBoxModel { get; set; }
    [BindProperty] public FormElementAddressModel? AddressModel { get; set; }
    [BindProperty] public FormElementMultiLineInputModel? MultiLineInputModel { get; set; }
    [BindProperty] public FormElementUrlInputModel? UrlInputModel { get; set; }
    [BindProperty] public FormElementMultiQuestionModel? MultiQuestionModel { get; set; }
    public FormQuestionType? CurrentFormQuestionType { get; private set; }
    public string? PartialViewName { get; private set; }
    public IFormElementModel? PartialViewModel { get; private set; }
    public IMultiQuestionFormElementModel? MultiQuestionViewModel { get; set; }
    public FormQuestion? PreviousQuestion { get; private set; }
    public Guid? CheckYourAnswerQuestionId { get; private set; }
    public FormSectionType? FormSectionType { get; set; }
    public bool IsMultiQuestionPage { get; private set; }
    public bool IsInformalConsortium { get; set; }

    public string EncType =>
        (CurrentFormQuestionType == FormQuestionType.FileUpload) ||
        (IsMultiQuestionPage &&
         MultiQuestionViewModel?.Questions.Any(q => q.Type == FormQuestionType.FileUpload) == true)
            ? Multipart.FormData
            : Application.FormUrlEncoded;

    private string FormQuestionAnswerStateKey => $"Form_{OrganisationId}_{FormId}_{SectionId}_Answers";

    private async Task<ValidationResult> ValidateWithOrganisationLoad<T>(T? viewModel) where T : class
    {
        ModelState.Clear();

        if (viewModel != null)
        {
            var isValid = TryValidateModel(viewModel);

            if (viewModel is FormElementMultiQuestionModel multiQuestionModel && IsMultiQuestionPage)
            {
                var questionModelsList = multiQuestionModel.QuestionModels.ToList();
                for (int i = 0; i < questionModelsList.Count; i++)
                {
                    var questionModel = questionModelsList[i];
                    var questionIsValid = TryValidateModel(questionModel, $"QuestionModels[{i}]");

                    if (!questionIsValid)
                    {
                        isValid = false;
                    }
                }
            }

            if (isValid)
            {
                return ValidationResult.Success();
            }
        }

        if (viewModel is FormElementMultiQuestionModel && IsMultiQuestionPage)
        {
            MapMultiQuestionValidationErrors(ModelState);
        }

        var organisation = await LoadOrganisation();
        return ValidationResult.Failed(organisation);
    }

    private async Task<WebApiClientOrganisation?> LoadOrganisation()
    {
        var organisation = await organisationClient.GetOrganisationAsync(OrganisationId);
        if (organisation != null)
        {
            IsInformalConsortium = organisation.Type == OrganisationType.InformalConsortium;
        }

        return organisation;
    }

    private async Task<FormAnswer?> ProcessAnswer(IFormElementModel questionModel) =>
        questionModel switch
        {
            FormElementFileUploadModel fileUploadModel => await HandleFileUpload(fileUploadModel),
            _ => questionModel.GetAnswer()
        };

    private async Task ProcessQuestions(
        IEnumerable<IFormElementModel> questionModels,
        Func<IFormElementModel, Task<FormQuestion?>> questionLookup,
        Action<FormQuestion, FormAnswer?> saveAnswer)
    {
        foreach (var questionModel in questionModels)
        {
            var questionInSequence = await questionLookup(questionModel);
            if (questionInSequence == null) continue;

            var answer = await ProcessAnswer(questionModel);
            saveAnswer(questionInSequence, answer);
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var (currentQuestion, sectionDetails) = await InitializeForPost();
        if (currentQuestion == null) return Redirect("/page-not-found");

        if (IsMultiQuestionPage)
        {
            await ProcessMultiQuestionPageSubmission(currentQuestion);
        }
        else
        {
            PartialViewModel = GetPartialViewModel(currentQuestion, false, isFirst: true);
        }

        var submissionResult = await ProcessQuestionSubmission(currentQuestion, sectionDetails);
        if (submissionResult != null) return submissionResult;

        return await RedirectToPreviousQuestion() ?? RedirectToNextQuestion(currentQuestion);
    }

    private async Task ProcessMultiQuestionPageSubmission(FormQuestion currentQuestion)
    {
        var multiQuestionPage =
            await formsEngine.GetMultiQuestionPage(OrganisationId, FormId, SectionId, currentQuestion.Id);
        var existingAnswers = GetExistingAnswersForMultiQuestion(multiQuestionPage.Questions,
            tempDataService.PeekOrDefault<FormQuestionAnswerState>(FormQuestionAnswerStateKey));

        MultiQuestionModel = new FormElementMultiQuestionModel();
        MultiQuestionModel.Initialize(multiQuestionPage, existingAnswers);

        MultiQuestionViewModel = MultiQuestionModel;

        foreach (var questionModel in MultiQuestionModel.QuestionModels)
        {
            FormAnswer? newAnswer = null;
            var question = multiQuestionPage.Questions.FirstOrDefault(q => q.Id == questionModel.QuestionId);

            if (question == null) continue;

            switch (question.Type)
            {
                case FormQuestionType.SingleChoice:
                    var singleChoiceModel = (FormElementSingleChoiceModel)questionModel;
                    var singleChoiceFieldName =
                        singleChoiceModel.GetFieldName(nameof(singleChoiceModel.SelectedOption));
                    if (Request.Form.ContainsKey(singleChoiceFieldName))
                    {
                        singleChoiceModel.SelectedOption = Request.Form[singleChoiceFieldName].ToString();
                        newAnswer = singleChoiceModel.GetAnswer();
                    }

                    break;
                case FormQuestionType.GroupedSingleChoice:
                    var groupedSingleChoiceModel = (FormElementGroupedSingleChoiceModel)questionModel;
                    var groupedSingleChoiceFieldName =
                        groupedSingleChoiceModel.GetFieldName(nameof(groupedSingleChoiceModel.SelectedOption));
                    if (Request.Form.ContainsKey(groupedSingleChoiceFieldName))
                    {
                        groupedSingleChoiceModel.SelectedOption = Request.Form[groupedSingleChoiceFieldName].ToString();
                        newAnswer = groupedSingleChoiceModel.GetAnswer();
                    }

                    break;
                case FormQuestionType.Text:
                    var textInputModel = (FormElementTextInputModel)questionModel;
                    var textInputFieldName = textInputModel.GetFieldName(nameof(textInputModel.TextInput));
                    if (Request.Form.ContainsKey(textInputFieldName))
                    {
                        textInputModel.TextInput = Request.Form[textInputFieldName].ToString();
                        newAnswer = textInputModel.GetAnswer();
                    }

                    break;
                case FormQuestionType.MultiLine:
                    var multiLineInputModel = (FormElementMultiLineInputModel)questionModel;
                    var multiLineFieldName = multiLineInputModel.GetFieldName(nameof(multiLineInputModel.TextInput));
                    if (Request.Form.ContainsKey(multiLineFieldName))
                    {
                        multiLineInputModel.TextInput = Request.Form[multiLineFieldName].ToString();
                        newAnswer = multiLineInputModel.GetAnswer();
                    }

                    break;
                case FormQuestionType.Url:
                    var urlInputModel = (FormElementUrlInputModel)questionModel;
                    var urlFieldName = urlInputModel.GetFieldName(nameof(urlInputModel.TextInput));
                    if (Request.Form.ContainsKey(urlFieldName))
                    {
                        urlInputModel.TextInput = Request.Form[urlFieldName].ToString();
                        newAnswer = urlInputModel.GetAnswer();
                    }

                    break;
                case FormQuestionType.Date:
                    var dateInputModel = (FormElementDateInputModel)questionModel;
                    var dayFieldName = dateInputModel.GetFieldName(nameof(dateInputModel.Day));
                    var monthFieldName = dateInputModel.GetFieldName(nameof(dateInputModel.Month));
                    var yearFieldName = dateInputModel.GetFieldName(nameof(dateInputModel.Year));

                    if (Request.Form.ContainsKey(dayFieldName))
                        dateInputModel.Day = Request.Form[dayFieldName].ToString();
                    if (Request.Form.ContainsKey(monthFieldName))
                        dateInputModel.Month = Request.Form[monthFieldName].ToString();
                    if (Request.Form.ContainsKey(yearFieldName))
                        dateInputModel.Year = Request.Form[yearFieldName].ToString();
                    newAnswer = dateInputModel.GetAnswer();
                    break;
                case FormQuestionType.YesOrNo:
                    var yesNoInputModel = (FormElementYesNoInputModel)questionModel;
                    var yesNoFieldName = yesNoInputModel.GetFieldName(nameof(yesNoInputModel.YesNoInput));
                    if (Request.Form.ContainsKey(yesNoFieldName))
                    {
                        yesNoInputModel.YesNoInput = Request.Form[yesNoFieldName].ToString();
                        newAnswer = yesNoInputModel.GetAnswer();
                    }

                    break;
                case FormQuestionType.CheckBox:
                    var checkBoxInputModel = (FormElementCheckBoxInputModel)questionModel;
                    var checkBoxFieldName = checkBoxInputModel.GetFieldName(nameof(checkBoxInputModel.CheckBoxInput));
                    if (Request.Form.ContainsKey(checkBoxFieldName))
                    {
                        checkBoxInputModel.CheckBoxInput = bool.Parse(Request.Form[checkBoxFieldName].ToString());
                        newAnswer = checkBoxInputModel.GetAnswer();
                    }

                    break;
                case FormQuestionType.Address:
                    var addressModel = (FormElementAddressModel)questionModel;
                    var addressLine1FieldName = addressModel.GetFieldName(nameof(addressModel.AddressLine1));
                    var townOrCityFieldName = addressModel.GetFieldName(nameof(addressModel.TownOrCity));
                    var postcodeFieldName = addressModel.GetFieldName(nameof(addressModel.Postcode));
                    var countryFieldName = addressModel.GetFieldName(nameof(addressModel.Country));

                    if (Request.Form.ContainsKey(addressLine1FieldName))
                        addressModel.AddressLine1 = Request.Form[addressLine1FieldName].ToString();
                    if (Request.Form.ContainsKey(townOrCityFieldName))
                        addressModel.TownOrCity = Request.Form[townOrCityFieldName].ToString();
                    if (Request.Form.ContainsKey(postcodeFieldName))
                        addressModel.Postcode = Request.Form[postcodeFieldName].ToString();
                    if (Request.Form.ContainsKey(countryFieldName))
                        addressModel.Country = Request.Form[countryFieldName].ToString();
                    newAnswer = addressModel.GetAnswer();
                    break;
                case FormQuestionType.FileUpload:
                    var fileUploadModel = (FormElementFileUploadModel)questionModel;
                    var uploadedFileNameFieldName =
                        fileUploadModel.GetFieldName(nameof(fileUploadModel.UploadedFileName));
                    var uploadedFileFieldName = fileUploadModel.GetFieldName(nameof(fileUploadModel.UploadedFile));
                    var hasValueFieldName = fileUploadModel.GetFieldName(nameof(fileUploadModel.HasValue));

                    if (Request.Form.ContainsKey(uploadedFileNameFieldName))
                    {
                        fileUploadModel.UploadedFileName = Request.Form[uploadedFileNameFieldName].ToString();
                    }

                    if (Request.Form.ContainsKey(hasValueFieldName))
                    {
                        fileUploadModel.HasValue = bool.Parse(Request.Form[hasValueFieldName].ToString());
                    }

                    if (Request.Form.Files.Any(f => f.Name == uploadedFileFieldName))
                    {
                        fileUploadModel.UploadedFile = Request.Form.Files.GetFile(uploadedFileFieldName);
                    }

                    newAnswer = await HandleFileUpload(fileUploadModel);
                    break;
                case FormQuestionType.NoInput:
                default:
                    break;
            }

            if (newAnswer != null)
            {
                SaveAnswerToTempData(question, newAnswer);
            }
            else if (question.Type != FormQuestionType.NoInput)
            {
                SaveAnswerToTempData(question, null);
            }
        }
    }


    private void MapMultiQuestionValidationErrors(ModelStateDictionary modelState)
    {
        if (MultiQuestionViewModel?.QuestionModels == null || modelState.ErrorCount == 0)
        {
            return;
        }

        var errorsToRemap = new List<(string originalKey, string newKey, ModelError error)>();

        foreach (var modelStateEntry in modelState)
        {
            var originalKey = modelStateEntry.Key;

            foreach (var error in modelStateEntry.Value.Errors)
            {
                var matchingQuestionModel = FindMatchingQuestionModel(originalKey, MultiQuestionViewModel.QuestionModels);

                if (matchingQuestionModel != null)
                {
                    var newKey = GetMappedFieldName(originalKey, matchingQuestionModel);
                    errorsToRemap.Add((originalKey, newKey, error));
                }
            }
        }

        foreach (var (originalKey, newKey, error) in errorsToRemap)
        {
            modelState.Remove(originalKey);
            modelState.AddModelError(newKey, error.ErrorMessage);
        }
    }

    private IFormElementModel? FindMatchingQuestionModel(string errorKey, IEnumerable<IFormElementModel> questionModels)
    {
        var formElementModels = questionModels.ToList();
        if (errorKey.StartsWith("QuestionModels["))
        {
            var questionModelsList = formElementModels.ToList();
            var startIndex = errorKey.IndexOf('[') + 1;
            var endIndex = errorKey.IndexOf(']');

            if (startIndex > 0 && endIndex > startIndex)
            {
                var indexString = errorKey.Substring(startIndex, endIndex - startIndex);
                if (int.TryParse(indexString, out var index) && index >= 0 && index < questionModelsList.Count)
                {
                    return questionModelsList[index];
                }
            }
        }

        return formElementModels.FirstOrDefault(q => IsErrorForQuestionModel(errorKey, q));
    }

    private bool IsErrorForQuestionModel(string errorKey, IFormElementModel questionModel)
    {
        if (questionModel is not FormElementModel fem || !fem.QuestionId.HasValue)
            return false;

        var questionId = fem.QuestionId.Value;

        if (errorKey.StartsWith("MultiQuestionViewModel.QuestionModels["))
        {
            var questionModels = MultiQuestionViewModel?.QuestionModels.ToList();
            if (questionModels != null)
            {
                var index = questionModels.IndexOf(questionModel);
                if (index >= 0 && errorKey.StartsWith($"MultiQuestionViewModel.QuestionModels[{index}]."))
                {
                    return true;
                }
            }
        }

        return errorKey.Contains(questionId.ToString()) ||
               IsPropertyOfQuestionModel(errorKey, questionModel);
    }

    private bool IsPropertyOfQuestionModel(string propertyName, IFormElementModel questionModel)
    {
        return questionModel switch
        {
            FormElementTextInputModel => propertyName.Contains("TextInput") || propertyName.Contains("HasValue"),
            FormElementYesNoInputModel => propertyName.Contains("YesNoInput"),
            FormElementDateInputModel => propertyName.Contains("Day") || propertyName.Contains("Month") || propertyName.Contains("Year") || propertyName.Contains("DateString") || propertyName.Contains("HasValue"),
            FormElementSingleChoiceModel => propertyName.Contains("SelectedOption"),
            FormElementGroupedSingleChoiceModel => propertyName.Contains("SelectedOption"),
            FormElementMultiLineInputModel => propertyName.Contains("TextInput"),
            FormElementUrlInputModel => propertyName.Contains("TextInput") || propertyName.Contains("HasValue"),
            FormElementAddressModel => propertyName.Contains("AddressLine1") || propertyName.Contains("TownOrCity") || propertyName.Contains("Postcode") || propertyName.Contains("Country"),
            FormElementCheckBoxInputModel => propertyName.Contains("CheckBoxInput"),
            FormElementFileUploadModel => propertyName.Contains("UploadedFileName") || propertyName.Contains("HasValue") || propertyName.Contains("UploadedFile"),
            _ => false
        };
    }

    private string GetMappedFieldName(string originalKey, IFormElementModel questionModel)
    {
        if (questionModel is not FormElementModel fem)
            return originalKey;

        var propertyName = ExtractPropertyName(originalKey, questionModel);

        return fem.GetFieldName(propertyName);
    }

    private string ExtractPropertyName(string originalKey, IFormElementModel questionModel)
    {
        if (originalKey.StartsWith("MultiQuestionViewModel.QuestionModels["))
        {
            var lastDotIndex = originalKey.LastIndexOf('.');
            if (lastDotIndex >= 0 && lastDotIndex < originalKey.Length - 1)
            {
                return originalKey.Substring(lastDotIndex + 1);
            }
        }

        return questionModel switch
        {
            FormElementTextInputModel when originalKey.Contains("TextInput") => "TextInput",
            FormElementTextInputModel when originalKey.Contains("HasValue") => "HasValue",
            FormElementDateInputModel when originalKey.Contains("Day") => "Day",
            FormElementDateInputModel when originalKey.Contains("Month") => "Month",
            FormElementDateInputModel when originalKey.Contains("Year") => "Year",
            FormElementDateInputModel when originalKey.Contains("DateString") => "DateString",
            FormElementDateInputModel when originalKey.Contains("HasValue") => "HasValue",
            FormElementSingleChoiceModel when originalKey.Contains("SelectedOption") => "SelectedOption",
            FormElementGroupedSingleChoiceModel when originalKey.Contains("SelectedOption") => "SelectedOption",
            FormElementMultiLineInputModel when originalKey.Contains("TextInput") => "TextInput",
            FormElementUrlInputModel when originalKey.Contains("TextInput") => "TextInput",
            FormElementUrlInputModel when originalKey.Contains("HasValue") => "HasValue",
            FormElementAddressModel when originalKey.Contains("AddressLine1") => "AddressLine1",
            FormElementAddressModel when originalKey.Contains("TownOrCity") => "TownOrCity",
            FormElementAddressModel when originalKey.Contains("Postcode") => "Postcode",
            FormElementAddressModel when originalKey.Contains("Country") => "Country",
            FormElementCheckBoxInputModel when originalKey.Contains("CheckBoxInput") => "CheckBoxInput",
            FormElementFileUploadModel when originalKey.Contains("UploadedFileName") => "UploadedFileName",
            FormElementFileUploadModel when originalKey.Contains("HasValue") => "HasValue",
            FormElementFileUploadModel when originalKey.Contains("UploadedFile") => "UploadedFile",
            FormElementYesNoInputModel when originalKey.Contains("YesNoInput") => "YesNoInput",
            _ => originalKey
        };
    }
    private async Task<IActionResult?> RedirectToPreviousQuestion()
    {
        var previousUnansweredQuestionId = await ValidateIfNeedRedirect();
        return previousUnansweredQuestionId != null
            ? RedirectToPage("FormsQuestionPage",
                new { OrganisationId, FormId, SectionId, CurrentQuestionId = previousUnansweredQuestionId })
            : null;
    }

    private async Task<(FormQuestion?, SectionQuestionsResponse)> InitializeForPost()
    {
        var sectionDetails = await formsEngine.GetFormSectionAsync(OrganisationId, FormId, SectionId);
        var currentQuestion = await InitModel();
        return (currentQuestion, sectionDetails);
    }

    private async Task<IActionResult?> ProcessQuestionSubmission(FormQuestion currentQuestion,
        SectionQuestionsResponse sectionDetails)
    {
        return (PartialViewModel, MultiQuestionViewModel, IsMultiQuestionPage) switch
        {
            (not null, _, _) => await HandleSingleQuestionPost(currentQuestion, sectionDetails),
            (null, not null, true) => await HandleMultiQuestionPost(currentQuestion),
            _ => null
        };
    }

    private async Task<IActionResult?> HandleSingleQuestionPost(FormQuestion currentQuestion,
        SectionQuestionsResponse sectionDetails)
    {
        var validationResult = await ValidateWithOrganisationLoad(PartialViewModel);
        if (!validationResult.IsValid)
            return Page();

        return await ProcessSingleQuestion(currentQuestion, sectionDetails);
    }

    private async Task<IActionResult?> ProcessSingleQuestion(FormQuestion currentQuestion,
        SectionQuestionsResponse sectionDetails)
    {
        var oldAnswer = GetAnswerFromTempData(currentQuestion);
        HandleBranchingLogicAnswerChange(currentQuestion, oldAnswer, sectionDetails.Questions);

        var answer = await ProcessAnswer(PartialViewModel!);
        SaveAnswerToTempData(currentQuestion, answer);

        return null;
    }

    private async Task<IActionResult?> HandleMultiQuestionPost(FormQuestion currentQuestion)
    {
        var validationResult = await ValidateWithOrganisationLoad(MultiQuestionViewModel);
        if (!validationResult.IsValid)
            return Page();

        await ProcessMultiQuestions(currentQuestion);
        return null;
    }

    private async Task ProcessMultiQuestions(FormQuestion currentQuestion)
    {
        var multiQuestionPage =
            await formsEngine.GetMultiQuestionPage(OrganisationId, FormId, SectionId, currentQuestion.Id);

        await ProcessQuestions(
            MultiQuestionViewModel!.QuestionModels,
            questionModel => Task.FromResult(
                multiQuestionPage.Questions.FirstOrDefault(q => q.Id == questionModel.QuestionId)
            ),
            SaveAnswerToTempData
        );

        var questionsWithModels = MultiQuestionViewModel.QuestionModels.Select(q => q.QuestionId).ToHashSet();
        var noInputQuestions = multiQuestionPage.Questions
            .Where(q => q.Type == FormQuestionType.NoInput && !questionsWithModels.Contains(q.Id));

        foreach (var noInputQuestion in noInputQuestions)
        {
            SaveAnswerToTempData(noInputQuestion, null);
        }
    }

    private async Task<string> GetAnswerString(QuestionAnswer questionAnswer, FormQuestion question)
    {
        var answer = questionAnswer.Answer;
        if (answer == null) return string.Empty;

        var boolPart = FormatBoolAnswer(answer.BoolValue);
        var valuePart = await FormatAnswerByType(answer, question);

        return CombineAnswerParts(boolPart, valuePart);
    }

    private string FormatBoolAnswer(bool? boolValue) =>
        boolValue switch
        {
            true => localizer[nameof(StaticTextResource.Global_Yes)],
            false => localizer[nameof(StaticTextResource.Global_No)],
            _ => string.Empty
        };

    private async Task<string> FormatAnswerByType(FormAnswer answer, FormQuestion question) =>
        question.Type switch
        {
            FormQuestionType.Text or FormQuestionType.FileUpload or FormQuestionType.MultiLine or FormQuestionType.Url
                => answer.TextValue ?? string.Empty,
            FormQuestionType.SingleChoice => await FormatSingleChoice(answer, question),
            FormQuestionType.Date => FormatDate(answer.DateValue),
            FormQuestionType.CheckBox => FormatCheckBox(answer, question),
            FormQuestionType.Address => FormatAddress(answer.AddressValue),
            FormQuestionType.GroupedSingleChoice => FormatGroupedSingleChoice(answer, question),
            _ => string.Empty
        };

    private async Task<string> FormatSingleChoice(FormAnswer answer, FormQuestion question)
    {
        var strategy = choiceProviderService.GetStrategy(question.Options.ChoiceProviderStrategy);
        return await strategy.RenderOption(answer) ?? string.Empty;
    }

    private static string FormatDate(DateTimeOffset? dateValue) =>
        dateValue?.ToString("dd/MM/yyyy") ?? string.Empty;

    private static string FormatCheckBox(FormAnswer answer, FormQuestion question) =>
        answer.BoolValue == true
            ? question.Options.Choices?.Values.FirstOrDefault() ?? string.Empty
            : string.Empty;

    private static string FormatAddress(Address? address)
    {
        if (address == null) return string.Empty;
        var parts = new[] { address.AddressLine1, address.TownOrCity, address.Postcode }
            .Where(p => !string.IsNullOrWhiteSpace(p));
        return string.Join(", ", parts);
    }

    private static string FormatGroupedSingleChoice(FormAnswer answer, FormQuestion question)
    {
        if (answer.OptionValue == null) return string.Empty;

        var choice = question.Options.Groups
            .SelectMany(g => g.Choices)
            .FirstOrDefault(c => c.Value == answer.OptionValue);

        return choice?.Title ?? answer.OptionValue ?? string.Empty;
    }

    private static string CombineAnswerParts(params string[] parts) =>
        string.Join(", ", parts.Where(s => !string.IsNullOrWhiteSpace(s)));

    private record ValidationResult(bool IsValid, WebApiClientOrganisation? Organisation = null)
    {
        public static ValidationResult Success() => new(true);
        public static ValidationResult Failed(WebApiClientOrganisation? org) => new(false, org);
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var redirectResult = await RedirectToPreviousQuestion();
        if (redirectResult != null) return redirectResult;

        var currentQuestion = await InitModel(true);
        if (currentQuestion == null) return Redirect("/page-not-found");

        await LoadOrganisation();
        return Page();
    }

    private async Task<Guid?> ValidateIfNeedRedirect()
    {
        var form = await formsEngine.GetFormSectionAsync(OrganisationId, FormId, SectionId);
        var answerState = tempDataService.PeekOrDefault<FormQuestionAnswerState>(FormQuestionAnswerStateKey);
        var currentQuestion = form.Questions.FirstOrDefault(q => q.Id == CurrentQuestionId);

        if (currentQuestion?.BranchType == FormQuestionBranchType.Alternative)
        {
            return null;
        }

        var previousUnansweredQuestionId =
            formsEngine.GetPreviousUnansweredQuestionId(form.Questions, CurrentQuestionId, answerState);

        var shouldRedirect = previousUnansweredQuestionId != null && previousUnansweredQuestionId != CurrentQuestionId;

        return shouldRedirect ? previousUnansweredQuestionId : null;
    }

    private async Task<FormQuestion?> InitModel(bool reset = false)
    {
        var form = await formsEngine.GetFormSectionAsync(OrganisationId, FormId, SectionId);
        var currentQuestion =
            await formsEngine.GetCurrentQuestion(OrganisationId, FormId, SectionId, CurrentQuestionId);

        return currentQuestion switch
        {
            null => null,
            _ => await InitializeModelState(form, currentQuestion, reset)
        };
    }

    private async Task<FormQuestion> InitializeModelState(SectionQuestionsResponse form, FormQuestion currentQuestion,
        bool reset)
    {
        var answerState = tempDataService.PeekOrDefault<FormQuestionAnswerState>(FormQuestionAnswerStateKey);

        SetCommonProperties(form, currentQuestion);
        await SetViewModelProperties(currentQuestion, answerState, reset);
        await SetNavigationProperties(form, currentQuestion, answerState);

        return currentQuestion;
    }

    private void SetCommonProperties(SectionQuestionsResponse form, FormQuestion currentQuestion)
    {
        FormSectionType = form.Section?.Type;
        CurrentFormQuestionType = currentQuestion.Type;
        IsMultiQuestionPage = currentQuestion.Options.Grouping?.Page != null;
    }

    private async Task SetViewModelProperties(FormQuestion currentQuestion, FormQuestionAnswerState answerState,
        bool reset)
    {
        if (currentQuestion.Options.Grouping?.Page != null)
        {
            await InitializeMultiQuestionView(currentQuestion, answerState);
        }
        else
        {
            InitializeSingleQuestionView(currentQuestion, reset);
        }
    }

    private async Task InitializeMultiQuestionView(FormQuestion currentQuestion, FormQuestionAnswerState answerState)
    {
        var multiQuestionPage =
            await formsEngine.GetMultiQuestionPage(OrganisationId, FormId, SectionId, currentQuestion.Id);
        var existingAnswers = GetExistingAnswersForMultiQuestion(multiQuestionPage.Questions, answerState);

        PartialViewName = "_FormElementMultiQuestion";
        MultiQuestionViewModel = MultiQuestionModel ?? new FormElementMultiQuestionModel();
        MultiQuestionViewModel.Initialize(multiQuestionPage, existingAnswers);
        PartialViewModel = null;
    }

    private void InitializeSingleQuestionView(FormQuestion currentQuestion, bool reset)
    {
        PartialViewName = GetPartialViewName(currentQuestion);
        PartialViewModel = GetPartialViewModel(currentQuestion, reset);
        MultiQuestionViewModel = null;
    }

    private async Task SetNavigationProperties(SectionQuestionsResponse form, FormQuestion currentQuestion,
        FormQuestionAnswerState answerState)
    {
        PreviousQuestion =
            await formsEngine.GetPreviousQuestion(OrganisationId, FormId, SectionId, currentQuestion.Id, answerState);
        CheckYourAnswerQuestionId = form.Questions.FirstOrDefault(q => q.Type == FormQuestionType.CheckYourAnswers)?.Id;
    }

    private static Dictionary<Guid, FormAnswer> GetExistingAnswersForMultiQuestion(List<FormQuestion> questions,
        FormQuestionAnswerState answerState)
    {
        return questions
            .Select(q => new
                { QuestionId = q.Id, answerState.Answers.FirstOrDefault(a => a.QuestionId == q.Id)?.Answer })
            .Where(x => x.Answer != null)
            .ToDictionary(x => x.QuestionId, x => x.Answer!);
    }

    private static string? GetPartialViewName(FormQuestion question)
    {
        if (question.Type == FormQuestionType.CheckYourAnswers)
            return null;

        var formQuestionPartials = new Dictionary<FormQuestionType, string>
        {
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
            { FormQuestionType.Url, "_FormElementUrlInput" }
        };

        return formQuestionPartials.TryGetValue(question.Type, out var partialView)
            ? partialView
            : throw new NotImplementedException($"Forms question: {question.Type} is not supported");
    }

    private IFormElementModel? GetPartialViewModel(FormQuestion question, bool reset)
    {
        if (question.Type == FormQuestionType.CheckYourAnswers)
            return null;

        IFormElementModel model = question.Type switch
        {
            FormQuestionType.NoInput => NoInputModel ?? new FormElementNoInputModel { Options = question.Options },
            FormQuestionType.Text => TextInputModel ?? new FormElementTextInputModel { Options = question.Options },
            FormQuestionType.FileUpload => FileUploadModel ?? new FormElementFileUploadModel { Options = question.Options },
            FormQuestionType.YesOrNo => YesNoInputModel ?? new FormElementYesNoInputModel { Options = question.Options },
            FormQuestionType.Date => DateInputModel ?? new FormElementDateInputModel { Options = question.Options },
            FormQuestionType.CheckBox => CheckBoxModel ?? new FormElementCheckBoxInputModel { Options = question.Options },
            FormQuestionType.Address => AddressModel ?? new FormElementAddressModel { Options = question.Options },
            FormQuestionType.SingleChoice => SingleChoiceModel ?? new FormElementSingleChoiceModel { Options = question.Options },
            FormQuestionType.MultiLine => MultiLineInputModel ?? new FormElementMultiLineInputModel { Options = question.Options },
            FormQuestionType.GroupedSingleChoice => GroupedSingleChoiceModel ?? new FormElementGroupedSingleChoiceModel { Options = question.Options },
            FormQuestionType.Url => UrlInputModel ?? new FormElementUrlInputModel { Options = question.Options },
            _ => throw new NotImplementedException($"Forms question: {question.Type} is not supported")
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
        return state.Answers.FirstOrDefault(a => a.QuestionId == question.Id)?.Answer;
    }

    private void HandleBranchingLogicAnswerChange(FormQuestion currentQuestion, FormAnswer? oldAnswerObject,
        List<FormQuestion> allQuestionsInSection)
    {
        if (!currentQuestion.NextQuestionAlternative.HasValue)
            return;

        var (oldAnswerIsYes, newAnswerIsYes) = currentQuestion.Type switch
        {
            FormQuestionType.YesOrNo => (
                oldAnswerObject?.BoolValue ?? false,
                YesNoInputModel?.GetAnswer()?.BoolValue ?? false
            ),
            FormQuestionType.FileUpload => (
                !string.IsNullOrEmpty(oldAnswerObject?.TextValue),
                (FileUploadModel?.UploadedFile is { Length: > 0 }) ||
                (!string.IsNullOrEmpty(FileUploadModel?.UploadedFileName) && FileUploadModel?.HasValue != false)
            ),
            _ => (false, false)
        };

        if (oldAnswerIsYes && !newAnswerIsYes && currentQuestion.NextQuestion.HasValue)
        {
            RemoveAnswersFromBranchPath(currentQuestion.NextQuestion.Value, allQuestionsInSection);
        }
        else if (!oldAnswerIsYes && newAnswerIsYes && currentQuestion.NextQuestionAlternative.HasValue)
        {
            RemoveAnswersFromBranchPath(currentQuestion.NextQuestionAlternative.Value, allQuestionsInSection);
        }
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

    private void RemoveAnswersFromBranchPath(Guid branchStartNodeId, List<FormQuestion> allQuestionsInSection)
    {
        var answerState = tempDataService.PeekOrDefault<FormQuestionAnswerState>(FormQuestionAnswerStateKey);
        if (answerState.Answers.Count == 0) return;

        var questionsMap = allQuestionsInSection.ToDictionary(q => q.Id);
        var questionsToClear = new HashSet<Guid>();
        var queue = new Queue<Guid>();

        if (questionsMap.ContainsKey(branchStartNodeId))
            queue.Enqueue(branchStartNodeId);

        while (queue.Count > 0)
        {
            var currentQId = queue.Dequeue();
            if (!questionsMap.TryGetValue(currentQId, out var questionNode) || !questionsToClear.Add(currentQId))
                continue;

            if (questionNode.NextQuestion.HasValue && questionsMap.ContainsKey(questionNode.NextQuestion.Value))
                queue.Enqueue(questionNode.NextQuestion.Value);
            if (questionNode.NextQuestionAlternative.HasValue &&
                questionsMap.ContainsKey(questionNode.NextQuestionAlternative.Value))
                queue.Enqueue(questionNode.NextQuestionAlternative.Value);
        }

        var initialCount = answerState.Answers.Count;
        answerState.Answers.RemoveAll(answer => questionsToClear.Contains(answer.QuestionId));

        if (answerState.Answers.Count < initialCount)
            tempDataService.Put(FormQuestionAnswerStateKey, answerState);
    }

    private IActionResult RedirectToNextQuestion(FormQuestion currentQuestion)
    {
        if (currentQuestion.Id == CheckYourAnswerQuestionId)
            return HandleCheckYourAnswers().GetAwaiter().GetResult();

        var nextQuestionId = RedirectFromCheckYourAnswerPage == true
            ? CheckYourAnswerQuestionId
            : GetNextQuestionId(currentQuestion);

        return RedirectToPage("FormsQuestionPage",
            new { OrganisationId, FormId, SectionId, CurrentQuestionId = nextQuestionId });
    }

    private Guid? GetNextQuestionId(FormQuestion currentQuestion)
    {
        var answerSet = tempDataService.PeekOrDefault<FormQuestionAnswerState>(FormQuestionAnswerStateKey);
        return formsEngine.GetNextQuestion(OrganisationId, FormId, SectionId, currentQuestion.Id, answerSet)
            .GetAwaiter().GetResult()?.Id;
    }

    private async Task<IActionResult> HandleCheckYourAnswers()
    {
        var answerSet = tempDataService.PeekOrDefault<FormQuestionAnswerState>(FormQuestionAnswerStateKey);
        await formsEngine.SaveUpdateAnswers(FormId, SectionId, OrganisationId, answerSet);
        tempDataService.Remove(FormQuestionAnswerStateKey);

        return FormSectionType == Models.FormSectionType.Declaration
            ? await CreateShareCodeAndRedirect()
            : RedirectToPage("FormsAnswerSetSummary", new { OrganisationId, FormId, SectionId });
    }

    private async Task<IActionResult> CreateShareCodeAndRedirect()
    {
        var shareCode = await formsEngine.CreateShareCodeAsync(FormId, OrganisationId);
        return RedirectToPage("/ShareInformation/ShareCodeConfirmation",
            new { OrganisationId, FormId, SectionId, shareCode });
    }

    private async Task<FormAnswer?> HandleFileUpload(FormElementFileUploadModel fileUploadModel)
    {
        var newFileInfo = fileUploadModel.GetUploadedFileInfo();
        if (newFileInfo != null)
        {
            return await ProcessNewFileUpload(newFileInfo.Value, fileUploadModel);
        }

        if (fileUploadModel.HasValue == false)
        {
            return new FormAnswer { BoolValue = false };
        }

        return !string.IsNullOrEmpty(fileUploadModel.UploadedFileName)
            ? new FormAnswer { BoolValue = true, TextValue = fileUploadModel.UploadedFileName }
            : null;
    }

    private async Task<FormAnswer> ProcessNewFileUpload(
        (IFormFile formFile, string filename, string contentType) fileInfo,
        FormElementFileUploadModel fileUploadModel)
    {
        await using var stream = fileInfo.formFile.OpenReadStream();
        await fileHostManager.UploadFile(stream, fileInfo.filename, fileInfo.contentType);

        var userInfo = await userInfoService.GetUserInfo();
        var organisation = await organisationClient.GetOrganisationAsync(OrganisationId);

        await publisher.Publish(new ScanFile
        {
            QueueFileName = fileInfo.filename,
            UploadedFileName = fileUploadModel.UploadedFile!.FileName,
            OrganisationId = OrganisationId,
            OrganisationEmailAddress = organisation.ContactPoint.Email,
            UserEmailAddress = userInfo.Email,
            OrganisationName = organisation.Name,
            FullName = userInfo.Name
        });

        return new FormAnswer { BoolValue = true, TextValue = fileInfo.filename };
    }

    public async Task<IEnumerable<AnswerSummary>> GetAnswers()
    {
        var answerSet = tempDataService.PeekOrDefault<FormQuestionAnswerState>(FormQuestionAnswerStateKey);
        var form = await formsEngine.GetFormSectionAsync(OrganisationId, FormId, SectionId);

        var summaryList = new List<AnswerSummary>();
        foreach (var answer in answerSet.Answers)
        {
            var question = form?.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
            if (question == null || question.Type == FormQuestionType.NoInput ||
                question.Type == FormQuestionType.CheckYourAnswers) continue;
            var summary = new AnswerSummary
            {
                Title = question.SummaryTitle ?? question.Title,
                Answer = await GetAnswerString(answer, question),
                ChangeLink =
                    $"/organisation/{OrganisationId}/forms/{FormId}/sections/{SectionId}/questions/{answer.QuestionId}?frm-chk-answer=true"
            };

            if (question.Type == FormQuestionType.Address && answer.Answer?.AddressValue != null
                                                          && answer.Answer.AddressValue.Country !=
                                                          Country.UKCountryCode)
            {
                summary.ChangeLink += "&UkOrNonUk=non-uk";
            }

            summaryList.Add(summary);
        }

        return summaryList;
    }

    public async Task<FormElementCheckYourAnswersModel> GetGroupedAnswersModel()
    {
        var answerSet = tempDataService.PeekOrDefault<FormQuestionAnswerState>(FormQuestionAnswerStateKey);
        var displayItems = await formsEngine.GetGroupedAnswerSummaries(OrganisationId, FormId, SectionId, answerSet);

        return new FormElementCheckYourAnswersModel
        {
            DisplayItems = displayItems,
            FormSectionType = FormSectionType
        };
    }

    public bool PreviousQuestionHasNonUkAddressAnswer()
    {
        if (PreviousQuestion is not { Type: FormQuestionType.Address }) return false;
        var answer = GetAnswerFromTempData(PreviousQuestion);
        if (answer?.AddressValue != null)
        {
            return answer.AddressValue.Country != Country.UKCountryCode;
        }

        return false;
    }
}