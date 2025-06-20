using CO.CDP.Forms.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;
using DataShareWebApiClient = CO.CDP.DataSharing.WebApiClient;
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
            Section = new Models.FormSection
            {
                Id = response.Section.Id,
                Type = (Models.FormSectionType)response.Section.Type,
                Title = response.Section.Title,
                AllowsMultipleAnswerSets = response.Section.AllowsMultipleAnswerSets
            },
            Questions = (await Task.WhenAll(response.Questions.Select(async q =>
            {
                IChoiceProviderStrategy choiceProviderStrategy =
                    choiceProviderService.GetStrategy(q.Options.ChoiceProviderStrategy);

                return new Models.FormQuestion
                {
                    Id = q.Id,
                    Title = q.Title,
                    Description = q.Description,
                    Caption = q.Caption,
                    SummaryTitle = q.SummaryTitle,
                    Type = (Models.FormQuestionType)q.Type,
                    IsRequired = q.IsRequired,
                    NextQuestion = q.NextQuestion,
                    NextQuestionAlternative = q.NextQuestionAlternative,
                    Options = new Models.FormQuestionOptions
                    {
                        Choices = await choiceProviderStrategy.Execute(q.Options),
                        ChoiceProviderStrategy = q.Options.ChoiceProviderStrategy,
                        Groups = q.Options.Groups.Select(g => new Models.FormQuestionGroup
                        {
                            Name = g.Name,
                            Hint = g.Hint,
                            Caption = g.Caption,
                            Choices = g.Choices.Select(c => new Models.FormQuestionGroupChoice
                            {
                                Title = c.Title,
                                Value = c.Value
                            }).ToList()
                        }).ToList(),
                        AnswerFieldName = q.Options.AnswerFieldName
                    }
                };
            }))).ToList()
        };

        SetAlternativePathQuestions(sectionQuestionsResponse.Questions);

        tempDataService.Put(sessionKey, sectionQuestionsResponse);
        return sectionQuestionsResponse;
    }

    public async Task<Models.FormQuestion?> GetNextQuestion(Guid organisationId, Guid formId, Guid sectionId,
        Guid currentQuestionId, FormQuestionAnswerState? answerState)
    {
        var section = await GetFormSectionAsync(organisationId, formId, sectionId);

        var currentQuestion = section.Questions.FirstOrDefault(q => q.Id == currentQuestionId);
        if (currentQuestion == null)
        {
            return null;
        }

        Guid? determinedNextQuestionId = currentQuestion.NextQuestion;

        if (currentQuestion.NextQuestionAlternative.HasValue && answerState != null)
        {
            var currentQuestionAnswer = answerState.Answers.FirstOrDefault(a => a.QuestionId == currentQuestionId);

            if (currentQuestionAnswer?.Answer?.BoolValue == false)
            {
                if (currentQuestion.Type == Models.FormQuestionType.YesOrNo ||
                    currentQuestion.Type == Models.FormQuestionType.FileUpload)
                {
                    determinedNextQuestionId = currentQuestion.NextQuestionAlternative;
                }
            }
        }

        if (!determinedNextQuestionId.HasValue)
        {
            return null;
        }

        return section.Questions.FirstOrDefault(q => q.Id == determinedNextQuestionId.Value);
    }

    public async Task<Models.FormQuestion?> GetPreviousQuestion(Guid organisationId, Guid formId, Guid sectionId,
        Guid currentQuestionId)
    {
        var section = await GetFormSectionAsync(organisationId, formId, sectionId);
        if (section.Questions == null)
        {
            return null;
        }

        var previousQuestionId = section.Questions.FirstOrDefault(q => q.NextQuestion == currentQuestionId)?.Id;
        return section.Questions.FirstOrDefault(q => q.Id == previousQuestionId);
    }

    public async Task<Models.FormQuestion?> GetCurrentQuestion(Guid organisationId, Guid formId, Guid sectionId,
        Guid? questionId)
    {
        var section = await GetFormSectionAsync(organisationId, formId, sectionId);
        if (section.Questions == null || !section.Questions.Any())
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

    public Guid? GetPreviousUnansweredQuestionId(List<Models.FormQuestion> allQuestions, Guid currentQuestionId, FormQuestionAnswerState answerState)
    {
        if (allQuestions.Count == 0)
        {
            return null;
        }

        List<Models.FormQuestion> pathTaken = BuildPathToQuestion(allQuestions, currentQuestionId, answerState);

        return FindFirstUnansweredQuestionInPath(pathTaken, answerState);
    }

    private Guid? FindFirstUnansweredQuestionInPath(List<Models.FormQuestion> pathTaken, FormQuestionAnswerState answerState)
    {
        var firstUnansweredValidQuestion = pathTaken
            .Where(q => q.Type != Models.FormQuestionType.CheckYourAnswers)
            .FirstOrDefault(q => !IsQuestionAnswered(q, answerState));

        return firstUnansweredValidQuestion?.Id;
    }

    private List<Models.FormQuestion> BuildPathToQuestion(List<Models.FormQuestion> allQuestions, Guid currentQuestionId, FormQuestionAnswerState answerState)
    {
        var questionsDictionary = allQuestions.ToDictionary(q => q.Id);
        var pathTaken = new List<Models.FormQuestion>();
        Models.FormQuestion? currentPathQuestion = GetFirstQuestion(allQuestions);

        while (currentPathQuestion != null && currentPathQuestion.Id != currentQuestionId)
        {
            pathTaken.Add(currentPathQuestion);
            Guid? nextQuestionIdOnPath = DetermineNextQuestionIdOnPath(currentPathQuestion, answerState);

            if (nextQuestionIdOnPath.HasValue && questionsDictionary.TryGetValue(nextQuestionIdOnPath.Value, out var nextQ))
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

    private static FormAddress? MapAddress(Address? formAdddress)
    {
        if (formAdddress == null)
            return null;
        return new FormAddress(
            streetAddress: formAdddress.AddressLine1,
            locality: formAdddress.TownOrCity,
            region: null,
            postalCode: formAdddress.Postcode,
            countryName: formAdddress.CountryName,
            country: formAdddress.Country
        );
    }

    private (HashSet<Guid> NextQuestionTargets, HashSet<Guid> AlternativeTargets) GetLinkedQuestionTargets(
        List<Models.FormQuestion> questions)
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

    private Models.FormQuestion? GetFirstQuestion(List<Models.FormQuestion> questions)
    {
        var (nextQuestionTargets, alternativeTargets) = GetLinkedQuestionTargets(questions);

        return questions.FirstOrDefault(q =>
            !nextQuestionTargets.Contains(q.Id) &&
            !alternativeTargets.Contains(q.Id));
    }

    private void SetAlternativePathQuestions(List<Models.FormQuestion> questions)
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
                question.BranchType = Models.FormQuestionBranchType.Alternative;
            }
        }
    }

    private bool IsQuestionAnswered(Models.FormQuestion questionOnPath, FormQuestionAnswerState? answerState)
    {
        if (questionOnPath.Type == Models.FormQuestionType.NoInput)
        {
            return answerState?.Answers.Any(a => a.QuestionId == questionOnPath.Id) ?? false;
        }
        return answerState?.Answers.Any(a =>
        {
            if (a.QuestionId != questionOnPath.Id || a.Answer == null) return false;

            return questionOnPath.Type switch
            {
                Models.FormQuestionType.Text => !string.IsNullOrWhiteSpace(a.Answer.TextValue),
                Models.FormQuestionType.MultiLine => !string.IsNullOrWhiteSpace(a.Answer.TextValue),
                Models.FormQuestionType.Url => !string.IsNullOrWhiteSpace(a.Answer.TextValue),
                Models.FormQuestionType.FileUpload => !string.IsNullOrWhiteSpace(a.Answer.TextValue),
                Models.FormQuestionType.YesOrNo => a.Answer.BoolValue.HasValue,
                Models.FormQuestionType.SingleChoice => !string.IsNullOrWhiteSpace(a.Answer.OptionValue) || !string.IsNullOrWhiteSpace(a.Answer.TextValue),
                Models.FormQuestionType.GroupedSingleChoice => !string.IsNullOrWhiteSpace(a.Answer.OptionValue),
                Models.FormQuestionType.Date => a.Answer.DateValue.HasValue,
                Models.FormQuestionType.CheckBox => a.Answer.BoolValue.HasValue,
                Models.FormQuestionType.Address => IsAddressAnswered(a.Answer.AddressValue),
                _ => false
            };
        }) ?? false;
    }

    private Guid? DetermineNextQuestionIdOnPath(Models.FormQuestion currentPathQuestion, FormQuestionAnswerState? answerState)
    {
        Guid? nextQuestionIdOnPath = currentPathQuestion.NextQuestion;

        if (currentPathQuestion.NextQuestionAlternative.HasValue && answerState?.Answers != null)
        {
            var answerToBranchingQuestion = answerState.Answers.FirstOrDefault(a => a.QuestionId == currentPathQuestion.Id);
            bool tookAlternativePath = false;

            if (answerToBranchingQuestion?.Answer != null)
            {
                switch (currentPathQuestion.Type)
                {
                    case Models.FormQuestionType.YesOrNo:
                        if (answerToBranchingQuestion.Answer.BoolValue == false)
                        {
                            tookAlternativePath = true;
                        }
                        break;
                    case Models.FormQuestionType.FileUpload:
                        if (string.IsNullOrEmpty(answerToBranchingQuestion.Answer.TextValue))
                        {
                            tookAlternativePath = true;
                        }
                        break;
                }
            }

            if (tookAlternativePath)
            {
                nextQuestionIdOnPath = currentPathQuestion.NextQuestionAlternative;
            }
        }
        return nextQuestionIdOnPath;
    }
}