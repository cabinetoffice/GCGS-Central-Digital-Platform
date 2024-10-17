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
                IChoiceProviderStrategy choiceProviderStrategy = choiceProviderService.GetStrategy(q.Options.ChoiceProviderStrategy);

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
                        Groups = q.Options.Groups?.Select(g => new Models.FormQuestionGroup
                        {
                            Name = g.Name,
                            Hint = g.Hint,
                            Caption = g.Caption,
                            Choices = g.Choices?.Select(c => new Models.FormQuestionGroupChoice
                            {
                                Title = c.Title,
                                Value = c.Value
                            }).ToList()
                        }).ToList(),
                        ChoiceAnswerFieldName = choiceProviderStrategy.AnswerFieldName
                    }
                };
            }))).ToList()
        };

        tempDataService.Put(sessionKey, sectionQuestionsResponse);
        return sectionQuestionsResponse;
    }

    public async Task<Models.FormQuestion?> GetNextQuestion(Guid organisationId, Guid formId, Guid sectionId, Guid currentQuestionId)
    {
        var section = await GetFormSectionAsync(organisationId, formId, sectionId);
        if (section.Questions == null)
        {
            return null;
        }

        var nextQuestionId = section.Questions.FirstOrDefault(q => q.Id == currentQuestionId)?.NextQuestion;
        return section.Questions.FirstOrDefault(q => q.Id == nextQuestionId);
    }

    public async Task<Models.FormQuestion?> GetPreviousQuestion(Guid organisationId, Guid formId, Guid sectionId, Guid currentQuestionId)
    {

        var section = await GetFormSectionAsync(organisationId, formId, sectionId);
        if (section.Questions == null)
        {
            return null;
        }

        var previousQuestionId = section.Questions.FirstOrDefault(q => q.NextQuestion == currentQuestionId)?.Id;
        return section.Questions.FirstOrDefault(q => q.Id == previousQuestionId);
    }

    public async Task<Models.FormQuestion?> GetCurrentQuestion(Guid organisationId, Guid formId, Guid sectionId, Guid? questionId)
    {
        var section = await GetFormSectionAsync(organisationId, formId, sectionId);
        if (section.Questions == null)
        {
            return null;
        }

        if (questionId == null)
        {
            var nextQuestionGuids = section.Questions.Select(sq => sq.NextQuestion);
            questionId = section.Questions.FirstOrDefault(q => !nextQuestionGuids.Contains(q.Id))?.Id;
        }

        return section.Questions.FirstOrDefault(q => q.Id == questionId);
    }

    public async Task SaveUpdateAnswers(Guid formId, Guid sectionId, Guid organisationId, FormQuestionAnswerState answerSet)
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

    public Guid? GetPreviousUnansweredQuestionId(List<Models.FormQuestion> questions, Guid currentQuestionId, FormQuestionAnswerState answerState)
    {
        var answeredQuestionIds = answerState.Answers.Select(a => a.QuestionId).ToList();

        var questionsInOrder = GetSortedFormQuestions(questions);

        foreach (var question in questionsInOrder)
        {
            if (question.Id == currentQuestionId)
            {
                break;
            }

            if (answerState?.Answers.Any(t => t.QuestionId == question.Id) == false)
            {
                return question.Id;
            }
        }

        return null;
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

    private List<Models.FormQuestion> GetSortedFormQuestions(List<Models.FormQuestion> questions)
    {
        var dict = questions.ToDictionary(q => q.Id, q => q);
        var start = questions.First(q => !questions.Any(x => x.NextQuestion == q.Id));
        var result = new List<Models.FormQuestion>();
        var current = start;
        while (current != null)
        {
            result.Add(current);
            current = current.NextQuestion.HasValue ? dict[current.NextQuestion.Value] : null;
        }

        return result;
    }

}