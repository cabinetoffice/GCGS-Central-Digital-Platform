using CO.CDP.Forms.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Extensions;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;
using CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;
using DataShareWebApiClient = CO.CDP.DataSharing.WebApiClient;
using FormAnswer = CO.CDP.OrganisationApp.Models.FormAnswer;
using FormQuestion = CO.CDP.OrganisationApp.Models.FormQuestion;
using FormQuestionGroup = CO.CDP.OrganisationApp.Models.FormQuestionGroup;
using FormQuestionGroupChoice = CO.CDP.OrganisationApp.Models.FormQuestionGroupChoice;
using FormQuestionGrouping = CO.CDP.OrganisationApp.Models.FormQuestionGrouping;
using FormQuestionOptions = CO.CDP.OrganisationApp.Models.FormQuestionOptions;
using FormQuestionType = CO.CDP.OrganisationApp.Models.FormQuestionType;
using FormSection = CO.CDP.OrganisationApp.Models.FormSection;
using FormSectionType = CO.CDP.OrganisationApp.Models.FormSectionType;
using SectionQuestionsResponse = CO.CDP.OrganisationApp.Models.SectionQuestionsResponse;

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
                        Choices = await choiceProviderStrategy.Execute(q.Options),
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
                        Grouping = q.Options.Grouping != null ? new FormQuestionGrouping
                        {
                            Page = q.Options.Grouping.Page != null ? new Models.PageGrouping
                            {
                                NextQuestionsToDisplay = q.Options.Grouping.Page.NextQuestionsToDisplay,
                                PageTitleResourceKey = q.Options.Grouping.Page.PageTitleResourceKey,
                                SubmitButtonTextResourceKey = q.Options.Grouping.Page.SubmitButtonTextResourceKey
                            } : null,
                            CheckYourAnswers = q.Options.Grouping.CheckYourAnswers != null ? new Models.CheckYourAnswersGrouping
                            {
                                GroupTitleResourceKey = q.Options.Grouping.CheckYourAnswers.GroupTitleResourceKey
                            } : null
                        } : null
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

        Guid? determinedNextQuestionId = DetermineNextQuestionId(currentQuestion, answerState);

        if (!determinedNextQuestionId.HasValue)
        {
            return null;
        }

        var grouping = currentQuestion.Options.Grouping;
        if (grouping?.Page != null)
        {
            determinedNextQuestionId = SkipMultiQuestionPageQuestions(determinedNextQuestionId.Value, section.Questions, grouping.Page.NextQuestionsToDisplay);
        }

        return !determinedNextQuestionId.HasValue ? null : section.Questions.FirstOrDefault(q => q.Id == determinedNextQuestionId.Value);
    }

    private static Guid? SkipMultiQuestionPageQuestions(Guid startQuestionId, List<FormQuestion> allQuestions, int questionsToSkip)
    {
        var questionLookup = allQuestions.ToDictionary(q => q.Id);
        Guid? currentQuestionId = startQuestionId;

        for (int i = 0; i < questionsToSkip && currentQuestionId.HasValue; i++)
        {
            if (!questionLookup.TryGetValue(currentQuestionId.Value, out var question))
                return currentQuestionId;

            currentQuestionId = question.NextQuestion;
        }

        return currentQuestionId;
    }

    public async Task<FormQuestion?> GetPreviousQuestion(Guid organisationId, Guid formId, Guid sectionId,
        Guid currentQuestionId, FormQuestionAnswerState? answerState)
    {
        var section = await GetFormSectionAsync(organisationId, formId, sectionId);

        var path = BuildPathToQuestion(section.Questions, currentQuestionId,
            answerState ?? new FormQuestionAnswerState());
        return path.LastOrDefault();
    }

    public async Task<FormQuestion?> GetCurrentQuestion(Guid organisationId, Guid formId, Guid sectionId,
        Guid? questionId)
    {
        var section = await GetFormSectionAsync(organisationId, formId, sectionId);
        if (!section.Questions.Any())
        {
            return null;
        }

        if (questionId == null)
        {
            return GetFirstQuestion(section.Questions);
        }

        return section.Questions.FirstOrDefault(q => q.Id == questionId.Value);
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

        List<FormQuestion> pathTaken = BuildPathToQuestion(allQuestions, currentQuestionId, answerState);

        return FindFirstUnansweredQuestionInPath(pathTaken, answerState);
    }

    private Guid? FindFirstUnansweredQuestionInPath(List<FormQuestion> pathTaken,
        FormQuestionAnswerState answerState)
    {
        var firstUnansweredValidQuestion = pathTaken
            .Where(q => q.Type != FormQuestionType.CheckYourAnswers)
            .FirstOrDefault(q => !IsQuestionAnswered(q, answerState));

        return firstUnansweredValidQuestion?.Id;
    }

    private List<FormQuestion> BuildPathToQuestion(List<FormQuestion> allQuestions,
        Guid currentQuestionId, FormQuestionAnswerState answerState)
    {
        var questionsDictionary = allQuestions.ToDictionary(q => q.Id);
        var pathTaken = new List<FormQuestion>();
        FormQuestion? currentPathQuestion = GetFirstQuestion(allQuestions);

        while (currentPathQuestion != null && currentPathQuestion.Id != currentQuestionId)
        {
            pathTaken.Add(currentPathQuestion);
            Guid? nextQuestionIdOnPath = DetermineNextQuestionId(currentPathQuestion, answerState);

            if (nextQuestionIdOnPath.HasValue &&
                questionsDictionary.TryGetValue(nextQuestionIdOnPath.Value, out var nextQ))
            {
                currentPathQuestion = nextQ;
            }
            else
            {
                currentPathQuestion = null;
            }
        }

        return pathTaken;
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
                !string.IsNullOrWhiteSpace(answer.TextValue) || (!questionOnPath.IsRequired && answer.BoolValue == false),
            FormQuestionType.MultiLine => !string.IsNullOrWhiteSpace(answer.TextValue),
            FormQuestionType.YesOrNo or FormQuestionType.CheckBox => answer.BoolValue.HasValue,
            FormQuestionType.SingleChoice => !string.IsNullOrWhiteSpace(answer.OptionValue) || !string.IsNullOrWhiteSpace(answer.JsonValue),
            FormQuestionType.GroupedSingleChoice => !string.IsNullOrWhiteSpace(answer.OptionValue),
            FormQuestionType.Date => answer.DateValue.HasValue || (!questionOnPath.IsRequired && answer.BoolValue == false),
            FormQuestionType.Address => IsAddressAnswered(answer.AddressValue),
            _ => false
        };
    }

    private Guid? DetermineNextQuestionId(FormQuestion currentQuestion, FormQuestionAnswerState? answerState)
    {
        var answer = answerState?.Answers.FirstOrDefault(a => a.QuestionId == currentQuestion.Id);

        bool takeAlternativePath = false;
        switch (currentQuestion.Type)
        {
            case FormQuestionType.YesOrNo:
                if (answer?.Answer?.BoolValue == false && currentQuestion.NextQuestionAlternative.HasValue)
                {
                    takeAlternativePath = true;
                }

                break;

            case FormQuestionType.FileUpload:
                bool explicitlyAnsweredNo = answer?.Answer?.BoolValue == false;

                if (!currentQuestion.IsRequired && (explicitlyAnsweredNo) &&
                    currentQuestion.NextQuestionAlternative.HasValue)
                {
                    takeAlternativePath = true;
                }

                break;
        }

        var nextQuestionId = takeAlternativePath ? currentQuestion.NextQuestionAlternative : currentQuestion.NextQuestion;

        return nextQuestionId;
    }

    public async Task<MultiQuestionPageModel> GetMultiQuestionPage(Guid organisationId, Guid formId, Guid sectionId, Guid startingQuestionId)
    {
        var section = await GetFormSectionAsync(organisationId, formId, sectionId);

        var startingQuestion = section.Questions.FirstOrDefault(q => q.Id == startingQuestionId);

        return startingQuestion switch
        {
            null => new MultiQuestionPageModel { Questions = [] },
            _ => BuildMultiQuestionPage(startingQuestion, section.Questions)
        };
    }

    private MultiQuestionPageModel BuildMultiQuestionPage(FormQuestion startingQuestion, List<FormQuestion> allQuestions)
    {
        var grouping = startingQuestion.Options.Grouping;

        return grouping?.Page == null
            ? new MultiQuestionPageModel { Questions = [startingQuestion] }
            : new MultiQuestionPageModel
            {
                Questions = CollectQuestionsForPage(startingQuestion, allQuestions, grouping.Page.NextQuestionsToDisplay),
                PageTitleResourceKey = grouping.Page.PageTitleResourceKey,
                SubmitButtonTextResourceKey = grouping.Page.SubmitButtonTextResourceKey
            };
    }

    private static List<FormQuestion> CollectQuestionsForPage(FormQuestion startingQuestion, List<FormQuestion> allQuestions, int questionsToDisplay)
    {
        var questionLookup = allQuestions.ToDictionary(q => q.Id);
        var questions = new List<FormQuestion> { startingQuestion };
        var currentQuestionId = startingQuestion.NextQuestion;

        for (var i = 0; i < questionsToDisplay && currentQuestionId.HasValue; i++)
        {
            if (!questionLookup.TryGetValue(currentQuestionId.Value, out var nextQuestion))
                break;

            questions.Add(nextQuestion);
            currentQuestionId = nextQuestion.NextQuestion;
        }

        return questions;
    }

    public async Task<List<IAnswerDisplayItem>> GetGroupedAnswerSummaries(Guid organisationId, Guid formId, Guid sectionId, FormQuestionAnswerState answerState)
    {
        var form = await GetFormSectionAsync(organisationId, formId, sectionId);

        var relevantQuestions = form.Questions
            .Where(q => q.Type != FormQuestionType.NoInput && q.Type != FormQuestionType.CheckYourAnswers)
            .ToList();

        var displayItems = new List<IAnswerDisplayItem>();
        var processedQuestionIds = new HashSet<Guid>();

        foreach (var question in relevantQuestions)
        {
            if (processedQuestionIds.Contains(question.Id)) continue;

            var grouping = question.Options.Grouping;

            if (grouping?.CheckYourAnswers != null)
            {
                var group = await CreateMultiQuestionGroup(question, relevantQuestions, answerState, organisationId, formId, sectionId, grouping);

                if (!group.Answers.Any()) continue;
                displayItems.Add(group);
                var multiQuestionPage = BuildMultiQuestionPage(question, relevantQuestions);
                multiQuestionPage.Questions.ForEach(q => processedQuestionIds.Add(q.Id));
            }
            else
            {
                var individualAnswer = await CreateIndividualAnswerSummary(question, answerState, organisationId, formId, sectionId);
                if (individualAnswer == null) continue;
                displayItems.Add(individualAnswer);
                processedQuestionIds.Add(question.Id);
            }
        }

        return displayItems;
    }


    private async Task<GroupedAnswerSummary> CreateMultiQuestionGroup(
        FormQuestion startingQuestion, List<FormQuestion> allQuestions, FormQuestionAnswerState answerState,
        Guid organisationId, Guid formId, Guid sectionId, FormQuestionGrouping grouping)
    {
        var multiQuestionPage = BuildMultiQuestionPage(startingQuestion, allQuestions);

        var answerTasks = multiQuestionPage.Questions
            .Select(async q => await CreateIndividualAnswerSummary(q, answerState, organisationId, formId, sectionId));

        var answers = (await Task.WhenAll(answerTasks))
            .Where(answer => answer != null)
            .Cast<AnswerSummary>()
            .ToList();

        return new GroupedAnswerSummary
        {
            GroupTitle = !string.IsNullOrEmpty(grouping.CheckYourAnswers?.GroupTitleResourceKey) ? grouping.CheckYourAnswers.GroupTitleResourceKey : startingQuestion.Title,
            GroupChangeLink = $"/organisation/{organisationId}/forms/{formId}/sections/{sectionId}/questions/{startingQuestion.Id}",
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
        var changeLink = $"/organisation/{organisationId}/forms/{formId}/sections/{sectionId}/questions/{question.Id}?frm-chk-answer=true";

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