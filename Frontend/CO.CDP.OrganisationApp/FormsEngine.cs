using CO.CDP.Forms.WebApiClient;
using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Extensions;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;
using CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;
using DataShareWebApiClient = CO.CDP.DataSharing.WebApiClient;
using DateValidationType = CO.CDP.OrganisationApp.Models.DateValidationType;
using FormAnswer = CO.CDP.OrganisationApp.Models.FormAnswer;
using FormQuestion = CO.CDP.OrganisationApp.Models.FormQuestion;
using FormQuestionGroup = CO.CDP.OrganisationApp.Models.FormQuestionGroup;
using FormQuestionGroupChoice = CO.CDP.OrganisationApp.Models.FormQuestionGroupChoice;
using FormQuestionGrouping = CO.CDP.OrganisationApp.Models.FormQuestionGrouping;
using FormQuestionOptions = CO.CDP.OrganisationApp.Models.FormQuestionOptions;
using FormQuestionType = CO.CDP.OrganisationApp.Models.FormQuestionType;
using FormSection = CO.CDP.OrganisationApp.Models.FormSection;
using FormSectionType = CO.CDP.OrganisationApp.Models.FormSectionType;
using InputWidthType = CO.CDP.OrganisationApp.Models.InputWidthType;
using LayoutOptions = CO.CDP.OrganisationApp.Models.LayoutOptions;
using SectionQuestionsResponse = CO.CDP.OrganisationApp.Models.SectionQuestionsResponse;
using TextValidationType = CO.CDP.OrganisationApp.Models.TextValidationType;
using ValidationOptions = CO.CDP.OrganisationApp.Models.ValidationOptions;

namespace CO.CDP.OrganisationApp;

public class FormsEngine(
    IFormsClient formsApiClient,
    ITempDataService tempDataService,
    IChoiceProviderService choiceProviderService,
    DataShareWebApiClient.IDataSharingClient dataSharingClient) : IFormsEngine
{
    public const string OrganisationSupplierInfoFormId = "0618b13e-eaf2-46e3-a7d2-6f2c44be7022";
    public const string OrganisationConsortiumFormId = "24482a2a-88a8-4432-b03c-4c966c9fce23";

    public async Task<SectionQuestionsResponse> GetFormSectionAsync(Guid organisationId, Guid formId, Guid sectionId)
    {
        var sessionKey = $"Form_{organisationId}_{formId}_{sectionId}_Questions";
        var cachedResponse = tempDataService.Peek<SectionQuestionsResponse>(sessionKey);

        if (cachedResponse != null)
        {
            return cachedResponse;
        }

        var response = await formsApiClient.GetFormSectionQuestionsAsync(formId, sectionId, organisationId);

        var sectionQuestionsResponse = new SectionQuestionsResponse
        {
            Section = new FormSection
            {
                Id = response.Section.Id,
                Type = (FormSectionType)response.Section.Type,
                Title = response.Section.Title,
                AllowsMultipleAnswerSets = response.Section.AllowsMultipleAnswerSets
            },
            Questions = (await Task.WhenAll(response.Questions.Select(async q =>
            {
                IChoiceProviderStrategy choiceProviderStrategy =
                    choiceProviderService.GetStrategy(q.Options.ChoiceProviderStrategy);

                var choices = await choiceProviderStrategy.Execute(q.Options);

                return new FormQuestion
                {
                    Id = q.Id,
                    Title = q.Title,
                    Description = q.Description,
                    Caption = q.Caption,
                    SummaryTitle = q.SummaryTitle,
                    Type = (FormQuestionType)q.Type,
                    IsRequired = q.IsRequired,
                    NextQuestion = q.NextQuestion,
                    NextQuestionAlternative = q.NextQuestionAlternative,
                    Options = new FormQuestionOptions
                    {
                        Choices = choices,
                        ChoiceProviderStrategy = q.Options.ChoiceProviderStrategy,
                        Groups = q.Options.Groups.Select(g => new FormQuestionGroup
                        {
                            Name = g.Name,
                            Hint = g.Hint,
                            Caption = g.Caption,
                            Choices = g.Choices.Select(c => new FormQuestionGroupChoice
                            {
                                Title = c.Title,
                                Value = c.Value
                            }).ToList()
                        }).ToList(),
                        AnswerFieldName = q.Options.AnswerFieldName,
                        Grouping = q.Options.Grouping != null
                            ? new FormQuestionGrouping
                            {
                                Id = q.Options.Grouping.Id,
                                Page = q.Options.Grouping.Page,
                                CheckYourAnswers = q.Options.Grouping.CheckYourAnswers,
                                SummaryTitle = q.Options.Grouping.SummaryTitle
                            }
                            : null,
                        Layout = q.Options.Layout != null
                            ? new LayoutOptions
                            {
                                CustomYesText = q.Options.Layout.CustomYesText,
                                CustomNoText = q.Options.Layout.CustomNoText,
                                InputWidth = q.Options.Layout.InputWidth.HasValue ? (InputWidthType)q.Options.Layout.InputWidth.Value : null,
                                InputSuffix = q.Options.Layout.InputSuffix,
                                CustomCssClasses = q.Options.Layout.CustomCssClasses,
                                BeforeTitleContent = q.Options.Layout.BeforeTitleContent,
                                BeforeButtonContent = q.Options.Layout.BeforeButtonContent,
                                AfterButtonContent = q.Options.Layout.AfterButtonContent,
                                PrimaryButtonText = q.Options.Layout.PrimaryButtonText
                            }
                            : null,
                        Validation = q.Options.Validation != null
                            ? new ValidationOptions
                            {
                                DateValidationType = q.Options.Validation.DateValidationType.HasValue ? (DateValidationType)q.Options.Validation.DateValidationType.Value : null,
                                MinDate = q.Options.Validation.MinDate,
                                MaxDate = q.Options.Validation.MaxDate,
                                TextValidationType = q.Options.Validation.TextValidationType.HasValue ? (TextValidationType)q.Options.Validation.TextValidationType.Value : null
                            }
                            : null
                    }
                };
            }))).ToList()
        };


        SetAlternativePathQuestions(sectionQuestionsResponse.Questions);
        tempDataService.Put(sessionKey, sectionQuestionsResponse);
        return sectionQuestionsResponse;
    }

    public async Task<FormQuestion?> GetNextQuestion(Guid organisationId, Guid formId, Guid sectionId,
        Guid currentQuestionId, FormQuestionAnswerState? answerState)
    {
        var section = await GetFormSectionAsync(organisationId, formId, sectionId);

        var currentQuestion = section.Questions.FirstOrDefault(q => q.Id == currentQuestionId);
        if (currentQuestion == null)
        {
            return null;
        }
        if (currentQuestion.Options.Grouping is { Page: true })
        {
            var groupNextQuestionId = GetMultiQuestionPageExitQuestion(currentQuestion, section.Questions);

            if (!groupNextQuestionId.HasValue)
            {
                return null;
            }

            return section.Questions.FirstOrDefault(q => q.Id == groupNextQuestionId.Value);
        }

        Guid? determinedNextQuestionId = DetermineNextQuestionId(currentQuestion, answerState);

        if (!determinedNextQuestionId.HasValue)
        {
            return null;
        }

        determinedNextQuestionId = SkipMultiQuestionPageQuestions(determinedNextQuestionId, section.Questions);

        return determinedNextQuestionId.HasValue
            ? section.Questions.FirstOrDefault(q => q.Id == determinedNextQuestionId.Value)
            : null;
    }

    private Guid? GetMultiQuestionPageExitQuestion(FormQuestion currentQuestion, List<FormQuestion> allQuestions)
    {

        if (currentQuestion.Options.Grouping is not { Page: true })
        {
            return null;
        }

        var groupId = currentQuestion.Options.Grouping?.Id;

        var questionsInGroup = allQuestions
            .Where(q => q.Options.Grouping?.Id == groupId && q.Options.Grouping?.Page == true)
            .OrderBy(q => q.NextQuestion.HasValue ? 0 : 1)
            .ToList();

        foreach (var question in questionsInGroup)
        {
            if (question.NextQuestion.HasValue)
            {
                var nextQuestion = allQuestions.FirstOrDefault(q => q.Id == question.NextQuestion.Value);
                if (nextQuestion == null || nextQuestion.Options.Grouping?.Id != groupId)
                {
                    return question.NextQuestion;
                }
            }
        }

        return null;
    }

    private Guid? SkipMultiQuestionPageQuestions(Guid? startQuestionId, List<FormQuestion> allQuestions)
    {
        if (!startQuestionId.HasValue)
            return null;

        var startQuestion = allQuestions.FirstOrDefault(q => q.Id == startQuestionId.Value);
        if (startQuestion?.Options.Grouping is not { Page: true })
        {
            return startQuestionId;
        }

        var currentGroupQuestions = allQuestions
            .Where(q => q.Options.Grouping?.Id == startQuestion.Options.Grouping?.Id && q.Options.Grouping?.Page == true)
            .ToList();

        var lastQuestionInGroup = currentGroupQuestions.LastOrDefault();
        return lastQuestionInGroup?.NextQuestion;
    }

    public async Task<FormQuestion?> GetPreviousQuestion(Guid organisationId, Guid formId, Guid sectionId,
        Guid currentQuestionId, FormQuestionAnswerState? answerState)
    {
        var section = await GetFormSectionAsync(organisationId, formId, sectionId);
        var currentQuestion = section.Questions.FirstOrDefault(q => q.Id == currentQuestionId);

        if (currentQuestion == null)
            return null;

        var effectiveCurrentQuestionId =
            GetMultiQuestionGroupStart(currentQuestion, section.Questions) ?? currentQuestionId;

        var path = BuildPathToQuestion(section.Questions, effectiveCurrentQuestionId,
            answerState ?? new FormQuestionAnswerState());
        return path.LastOrDefault();
    }

    public async Task<FormQuestion?> GetCurrentQuestion(Guid organisationId, Guid formId, Guid sectionId,
        Guid? questionId)
    {
        var section = await GetFormSectionAsync(organisationId, formId, sectionId);
        if (section.Questions.Count == 0)
        {
            return null;
        }

        return questionId == null
            ? GetFirstQuestion(section.Questions)
            : section.Questions.FirstOrDefault(q => q.Id == questionId.Value);
    }

    public async Task SaveUpdateAnswers(Guid formId, Guid sectionId, Guid organisationId,
        FormQuestionAnswerState answerSet)
    {
        var answersPayload = new UpdateFormSectionAnswers(
            answers: answerSet.Answers.Select(a => new Forms.WebApiClient.FormAnswer(
                id: a.AnswerId,
                boolValue: a.Answer?.BoolValue,
                numericValue: a.Answer?.NumericValue,
                dateValue: a.Answer?.DateValue,
                startValue: a.Answer?.StartValue,
                endValue: a.Answer?.EndValue,
                textValue: a.Answer?.TextValue,
                optionValue: a.Answer?.OptionValue,
                questionId: a.QuestionId,
                addressValue: MapAddress(a.Answer?.AddressValue),
                jsonValue: a.Answer?.JsonValue
            )).ToArray(),
            furtherQuestionsExempted: answerSet.FurtherQuestionsExempted
        );

        await formsApiClient.PutFormSectionAnswersAsync(
            formId,
            sectionId,
            answerSet.AnswerSetId ?? Guid.NewGuid(),
            organisationId,
            answersPayload);
    }

    public async Task<string> CreateShareCodeAsync(Guid formId, Guid organisationId)
    {
        var sharingDataDetails = await dataSharingClient.CreateSharedDataAsync(
            new DataShareWebApiClient.ShareRequest(formId, organisationId));

        return sharingDataDetails.ShareCode;
    }

    public Guid? GetPreviousUnansweredQuestionId(List<FormQuestion> allQuestions, Guid currentQuestionId,
        FormQuestionAnswerState answerState)
    {
        if (allQuestions.Count == 0)
        {
            return null;
        }

        var currentQuestion = allQuestions.FirstOrDefault(q => q.Id == currentQuestionId);
        if (currentQuestion == null)
            return null;

        var effectiveCurrentQuestionId = GetMultiQuestionGroupStart(currentQuestion, allQuestions) ?? currentQuestionId;

        var pathTaken = BuildPathToQuestion(allQuestions, effectiveCurrentQuestionId, answerState);

        return FindFirstUnansweredQuestionInPath(pathTaken, answerState, allQuestions);
    }

    private Guid? FindFirstUnansweredQuestionInPath(List<FormQuestion> pathTaken,
        FormQuestionAnswerState answerState, List<FormQuestion> allQuestions)
    {
        foreach (var question in pathTaken.Where(q => q.Type != FormQuestionType.CheckYourAnswers))
        {
            var grouping = question.Options.Grouping;
            if (grouping?.Page == true)
            {
                var multiQuestionPage = BuildMultiQuestionPage(question, allQuestions);

                var unansweredQuestions = multiQuestionPage.Questions
                    .Where(q => !IsQuestionAnswered(q, answerState))
                    .ToList();

                if (unansweredQuestions.Count != 0)
                {
                    return question.Id;
                }
            }
            else
            {
                var isAnswered = IsQuestionAnswered(question, answerState);

                if (!isAnswered)
                {
                    return question.Id;
                }
            }
        }

        return null;
    }

    private List<FormQuestion> BuildPathToQuestion(List<FormQuestion> allQuestions,
        Guid currentQuestionId, FormQuestionAnswerState answerState)
    {
        var questionsDictionary = allQuestions.ToDictionary(q => q.Id);
        var pathTaken = new List<FormQuestion>();
        var currentPathQuestion = GetFirstQuestion(allQuestions);

        while (currentPathQuestion != null && currentPathQuestion.Id != currentQuestionId)
        {
            pathTaken.Add(currentPathQuestion);

            var nextQuestionId = GetNextQuestionInPath(currentPathQuestion, allQuestions, answerState);

            currentPathQuestion = nextQuestionId.HasValue &&
                                  questionsDictionary.TryGetValue(nextQuestionId.Value, out var nextQuestion)
                ? nextQuestion
                : null;
        }

        return pathTaken;
    }

    private Guid? GetNextQuestionInPath(FormQuestion currentQuestion, List<FormQuestion> allQuestions,
        FormQuestionAnswerState answerState)
    {
        var nextQuestionId = DetermineNextQuestionId(currentQuestion, answerState);

        if (currentQuestion.Options.Grouping is not { Page: true })
        {
            return nextQuestionId;
        }
        var questionsInGroup = allQuestions
            .Where(q => q.Options.Grouping?.Id == currentQuestion.Options.Grouping?.Id &&
                        q.Options.Grouping?.Page == true)
            .ToList();

        var currentQuestionInGroup = questionsInGroup.FirstOrDefault(q => q.Id == currentQuestion.Id);
        while (currentQuestionInGroup != null)
        {
            var nextQuestion = allQuestions.FirstOrDefault(q => q.Id == currentQuestionInGroup.NextQuestion);
            if (nextQuestion == null || nextQuestion.Options.Grouping?.Id != currentQuestion.Options.Grouping?.Id)
            {
                var result = currentQuestionInGroup.NextQuestion;
                return result;
            }
            currentQuestionInGroup = nextQuestion;
        }

        var fallbackResult = questionsInGroup.LastOrDefault()?.NextQuestion;
        return fallbackResult;
    }

    private Guid? GetMultiQuestionGroupStart(FormQuestion currentQuestion, List<FormQuestion> allQuestions)
    {
        foreach (var question in allQuestions)
        {
            var grouping = question.Options.Grouping;
            if (grouping?.Page != true) continue;
            var multiQuestionPage = BuildMultiQuestionPage(question, allQuestions);
            if (multiQuestionPage.Questions.Any(q => q.Id == currentQuestion.Id))
            {
                return question.Id;
            }
        }

        return null;
    }

    private bool IsAddressAnswered(Address? address)
    {
        if (address == null) return false;
        return !string.IsNullOrWhiteSpace(address.AddressLine1) &&
               !string.IsNullOrWhiteSpace(address.Postcode);
    }

    private static FormAddress? MapAddress(Address? formAddress)
    {
        if (formAddress == null)
            return null;
        return new FormAddress(
            streetAddress: formAddress.AddressLine1,
            locality: formAddress.TownOrCity,
            region: null,
            postalCode: formAddress.Postcode,
            countryName: formAddress.CountryName,
            country: formAddress.Country
        );
    }

    private (HashSet<Guid> NextQuestionTargets, HashSet<Guid> AlternativeTargets) GetLinkedQuestionTargets(
        List<FormQuestion> questions)
    {
        var nextQuestionTargets = new HashSet<Guid>();
        var alternativeTargets = new HashSet<Guid>();

        foreach (var q in questions)
        {
            if (q.NextQuestion.HasValue)
            {
                nextQuestionTargets.Add(q.NextQuestion.Value);
            }

            if (q.NextQuestionAlternative.HasValue)
            {
                alternativeTargets.Add(q.NextQuestionAlternative.Value);
            }
        }

        return (nextQuestionTargets, alternativeTargets);
    }

    private FormQuestion? GetFirstQuestion(List<FormQuestion> questions)
    {
        var (nextQuestionTargets, alternativeTargets) = GetLinkedQuestionTargets(questions);

        return questions.FirstOrDefault(q =>
            !nextQuestionTargets.Contains(q.Id) &&
            !alternativeTargets.Contains(q.Id));
    }

    private void SetAlternativePathQuestions(List<FormQuestion> questions)
    {
        var (nextQuestionTargets, alternativeTargets) = GetLinkedQuestionTargets(questions);
        var mainPathQuestionIds = new HashSet<Guid>();
        var startQuestion = GetFirstQuestion(questions);
        if (startQuestion != null)
        {
            mainPathQuestionIds.Add(startQuestion.Id);
        }

        foreach (var targetId in nextQuestionTargets)
        {
            mainPathQuestionIds.Add(targetId);
        }

        foreach (var question in questions)
        {
            if ((alternativeTargets.Contains(question.Id) && !mainPathQuestionIds.Contains(question.Id)) ||
                (!mainPathQuestionIds.Contains(question.Id) && question.NextQuestion.HasValue))
            {
                question.BranchType = FormQuestionBranchType.Alternative;
            }
        }
    }

    private bool IsQuestionAnswered(FormQuestion questionOnPath, FormQuestionAnswerState? answerState)
    {
        var questionAnswer = answerState?.Answers.FirstOrDefault(a => a.QuestionId == questionOnPath.Id);

        if (questionOnPath.Type == FormQuestionType.NoInput)
        {
            return questionAnswer != null;
        }

        if (questionAnswer?.Answer == null)
        {
            return false;
        }

        var answer = questionAnswer.Answer;

        return questionOnPath.Type switch
        {
            FormQuestionType.Text or FormQuestionType.Url or FormQuestionType.FileUpload =>
                !string.IsNullOrWhiteSpace(answer.TextValue) ||
                (!questionOnPath.IsRequired && answer.BoolValue == false),
            FormQuestionType.MultiLine => !string.IsNullOrWhiteSpace(answer.TextValue),
            FormQuestionType.YesOrNo or FormQuestionType.CheckBox => answer.BoolValue.HasValue,
            FormQuestionType.SingleChoice => !string.IsNullOrWhiteSpace(answer.OptionValue) ||
                                             !string.IsNullOrWhiteSpace(answer.JsonValue),
            FormQuestionType.GroupedSingleChoice => !string.IsNullOrWhiteSpace(answer.OptionValue),
            FormQuestionType.Date => answer.DateValue.HasValue ||
                                     (!questionOnPath.IsRequired && answer.BoolValue == false),
            FormQuestionType.Address => IsAddressAnswered(answer.AddressValue),
            _ => false
        };
    }

    private Guid? DetermineNextQuestionId(FormQuestion currentQuestion, FormQuestionAnswerState? answerState)
    {
        var answer = answerState?.Answers.FirstOrDefault(a => a.QuestionId == currentQuestion.Id);

        var takeAlternativePath = false;
        switch (currentQuestion.Type)
        {
            case FormQuestionType.YesOrNo:
                if (answer?.Answer?.BoolValue == false && currentQuestion.NextQuestionAlternative.HasValue)
                {
                    takeAlternativePath = true;
                }

                break;

            case FormQuestionType.FileUpload:
                var explicitlyAnsweredNo = answer?.Answer?.BoolValue == false;

                if (!currentQuestion.IsRequired && (explicitlyAnsweredNo) &&
                    currentQuestion.NextQuestionAlternative.HasValue)
                {
                    takeAlternativePath = true;
                }

                break;
        }

        var nextQuestionId =
            takeAlternativePath ? currentQuestion.NextQuestionAlternative : currentQuestion.NextQuestion;

        return nextQuestionId;
    }

    public async Task<MultiQuestionPageModel> GetMultiQuestionPage(Guid organisationId, Guid formId, Guid sectionId,
        Guid startingQuestionId)
    {
        var section = await GetFormSectionAsync(organisationId, formId, sectionId);

        var startingQuestion = section.Questions.FirstOrDefault(q => q.Id == startingQuestionId);

        return startingQuestion switch
        {
            null => new MultiQuestionPageModel { Questions = [] },
            _ => BuildMultiQuestionPage(startingQuestion, section.Questions)
        };
    }

    private MultiQuestionPageModel BuildMultiQuestionPage(FormQuestion startingQuestion,
        List<FormQuestion> allQuestions)
    {
        var grouping = startingQuestion.Options.Grouping;

        return grouping?.Page != true
            ? new MultiQuestionPageModel { Questions = [startingQuestion] }
            : new MultiQuestionPageModel
            {
                Questions = CollectQuestionsForPage(startingQuestion, allQuestions)
            };
    }

    private static List<FormQuestion> CollectQuestionsForPage(FormQuestion startingQuestion,
        List<FormQuestion> allQuestions)
    {
        if (startingQuestion.Options.Grouping is null)
            return [startingQuestion];

        var questionsInGroup = allQuestions
            .Where(q => q.Options.Grouping?.Page == true &&
                        q.Options.Grouping?.Id == startingQuestion.Options.Grouping?.Id)
            .ToDictionary(q => q.Id);

        return BuildQuestionChain(startingQuestion, questionsInGroup);
    }

    private static List<FormQuestion> BuildQuestionChain(FormQuestion startingQuestion,
        IReadOnlyDictionary<Guid, FormQuestion> questionsInGroup)
    {
        var orderedQuestions = new List<FormQuestion>();
        var currentQuestion = startingQuestion;

        while (currentQuestion != null)
        {
            orderedQuestions.Add(currentQuestion);

            currentQuestion = currentQuestion.NextQuestion.HasValue &&
                             questionsInGroup.TryGetValue(currentQuestion.NextQuestion.Value, out var nextQuestion)
                ? nextQuestion
                : null;
        }

        return orderedQuestions;
    }

    private List<FormQuestion> BuildJourney(List<FormQuestion> allQuestions, FormQuestionAnswerState answerState)
    {
        var journey = new List<FormQuestion>();
        var question = GetFirstQuestion(allQuestions);

        while (question != null)
        {
            journey.Add(question);
            var nextQuestionId = DetermineNextQuestionId(question, answerState);
            question = nextQuestionId.HasValue
                ? allQuestions.FirstOrDefault(q => q.Id == nextQuestionId.Value)
                : null;
        }

        return journey;
    }

    public async Task<List<IAnswerDisplayItem>> GetGroupedAnswerSummaries(Guid organisationId, Guid formId,
        Guid sectionId, FormQuestionAnswerState answerState)
    {
        var form = await GetFormSectionAsync(organisationId, formId, sectionId);

        var journey = BuildJourney(form.Questions, answerState);

        var relevantQuestions = journey
            .Where(q => q.Type != FormQuestionType.NoInput && q.Type != FormQuestionType.CheckYourAnswers)
            .ToList();

        var displayItems = new List<IAnswerDisplayItem>();
        var processedQuestionIds = new HashSet<Guid>();

        foreach (var question in relevantQuestions)
        {
            if (processedQuestionIds.Contains(question.Id)) continue;

            var grouping = question.Options.Grouping;

            if (grouping?.CheckYourAnswers == true)
            {
                var group = await CreateMultiQuestionGroup(question, journey, answerState, organisationId,
                    formId, sectionId, grouping);

                if (group.Answers.Count == 0) continue;
                displayItems.Add(group);

                var groupQuestionIds = journey
                    .Where(q => q.Options.Grouping?.Id == grouping.Id)
                    .Select(q => q.Id);
                foreach (var id in groupQuestionIds)
                {
                    processedQuestionIds.Add(id);
                }
            }
            else
            {
                var individualAnswer =
                    await CreateIndividualAnswerSummary(question, answerState, organisationId, formId, sectionId);
                if (individualAnswer == null) continue;
                displayItems.Add(individualAnswer);
                processedQuestionIds.Add(question.Id);
            }
        }

        return displayItems;
    }


    private async Task<GroupedAnswerSummary> CreateMultiQuestionGroup(
        FormQuestion startingQuestion, List<FormQuestion> orderedJourney, FormQuestionAnswerState answerState,
        Guid organisationId, Guid formId, Guid sectionId, FormQuestionGrouping grouping)
    {
        var questionsInGroup = orderedJourney
            .Where(q => q.Options.Grouping?.Id == grouping.Id)
            .ToList();

        var answerTasks = questionsInGroup
            .Select(async q => await CreateIndividualAnswerSummary(q, answerState, organisationId, formId, sectionId));

        var answers = (await Task.WhenAll(answerTasks))
            .Where(answer => answer != null)
            .Cast<AnswerSummary>()
            .ToList();

        var groupChangeLink = grouping.Page
            ? $"/organisation/{organisationId}/forms/{formId}/sections/{sectionId}/questions/{startingQuestion.Id}"
            : null;

        if (!grouping.Page)
        {
            foreach (var answer in answers)
            {
                answer.ChangeLink = $"/organisation/{organisationId}/forms/{formId}/sections/{sectionId}/questions/{questionsInGroup.First(q => q.Title == answer.Title).Id}?frm-chk-answer=true";
            }
        }

        return new GroupedAnswerSummary
        {
            GroupTitle = !string.IsNullOrEmpty(grouping.SummaryTitle)
                ? grouping.SummaryTitle
                : startingQuestion.SummaryTitle,
            GroupChangeLink = groupChangeLink,
            Answers = answers
        };
    }

    private async Task<AnswerSummary?> CreateIndividualAnswerSummary(
        FormQuestion question, FormQuestionAnswerState answerState, Guid organisationId, Guid formId, Guid sectionId)
    {
        var questionAnswer = answerState.Answers
            .FirstOrDefault(a => a.QuestionId == question.Id && a.Answer != null);

        if (questionAnswer == null) return null;

        var answerString = await GetAnswerStringForSummary(questionAnswer, question);
        return string.IsNullOrWhiteSpace(answerString)
            ? null
            : CreateAnswerSummary(question, questionAnswer, answerString, organisationId, formId, sectionId);
    }

    private static AnswerSummary CreateAnswerSummary(
        FormQuestion question, QuestionAnswer questionAnswer, string answerString,
        Guid organisationId, Guid formId, Guid sectionId)
    {
        var changeLink =
            $"/organisation/{organisationId}/forms/{formId}/sections/{sectionId}/questions/{question.Id}?frm-chk-answer=true";

        var isNonUkAddress = question.Type == FormQuestionType.Address
                             && questionAnswer.Answer?.AddressValue?.Country != Country.UKCountryCode;

        if (isNonUkAddress)
        {
            changeLink += "&UkOrNonUk=non-uk";
        }

        return new AnswerSummary
        {
            Title = question.SummaryTitle ?? question.Title,
            Answer = answerString,
            ChangeLink = changeLink
        };
    }

    private async Task<string> GetAnswerStringForSummary(QuestionAnswer questionAnswer, FormQuestion question)
    {
        var answer = questionAnswer.Answer;
        if (answer == null) return "";

        var boolAnswerString = answer.BoolValue?.ToString() switch
        {
            "True" => "Yes",
            "False" => "No",
            _ => ""
        };

        var answerString = question.Type switch
        {
            FormQuestionType.Text or FormQuestionType.FileUpload or FormQuestionType.MultiLine or FormQuestionType.Url
                => answer.TextValue ?? "",
            FormQuestionType.SingleChoice => await GetSingleChoiceAnswerString(answer, question),
            FormQuestionType.Date => answer.DateValue?.ToFormattedString() ?? "",
            FormQuestionType.CheckBox => answer.BoolValue == true
                ? question.Options.Choices?.Values.FirstOrDefault() ?? ""
                : "",
            FormQuestionType.Address => answer.AddressValue?.ToHtmlString() ?? "",
            FormQuestionType.GroupedSingleChoice => GetGroupedSingleChoiceAnswerString(answer, question),
            _ => ""
        };

        return new[] { boolAnswerString, answerString }
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Aggregate("", (acc, s) => string.IsNullOrEmpty(acc) ? s : $"{acc}, {s}");
    }

    private async Task<string> GetSingleChoiceAnswerString(FormAnswer answer, FormQuestion question)
    {
        var choiceProviderStrategy = choiceProviderService.GetStrategy(question.Options.ChoiceProviderStrategy);
        return await choiceProviderStrategy.RenderOption(answer) ?? "";
    }

    private static string GetGroupedSingleChoiceAnswerString(FormAnswer? answer, FormQuestion question)
    {
        return question.Options.Groups
            .SelectMany(g => g.Choices)
            .Where(c => c.Value == answer?.OptionValue)
            .Select(c => c.Title)
            .FirstOrDefault() ?? answer?.OptionValue ?? "";
    }
}