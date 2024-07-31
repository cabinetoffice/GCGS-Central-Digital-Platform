using CO.CDP.Forms.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using SectionQuestionsResponse = CO.CDP.OrganisationApp.Models.SectionQuestionsResponse;

namespace CO.CDP.OrganisationApp;

public class FormsEngine(IFormsClient formsApiClient, ITempDataService tempDataService) : IFormsEngine
{
    public async Task<SectionQuestionsResponse> LoadFormSectionAsync(Guid organisationId, Guid formId, Guid sectionId)
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
                Title = response.Section.Title,
                AllowsMultipleAnswerSets = response.Section.AllowsMultipleAnswerSets
            },
            Questions = response.Questions.Select(q => new Models.FormQuestion
            {
                Id = q.Id,
                Title = q.Title,
                Description = q.Description,
                Type = (Models.FormQuestionType)q.Type,
                IsRequired = q.IsRequired,
                NextQuestion = q.NextQuestion,
                NextQuestionAlternative = q.NextQuestionAlternative,
                Options = new Models.FormQuestionOptions
                {
                    Choices = q.Options.Choices.Select(c => c.Title).ToList(),
                    ChoiceProviderStrategy = q.Options.ChoiceProviderStrategy
                }
            }).ToList()
        };

        tempDataService.Put(sessionKey, sectionQuestionsResponse);
        return sectionQuestionsResponse;
    }

    public async Task<Models.FormQuestion?> GetNextQuestion(Guid organisationId, Guid formId, Guid sectionId, Guid currentQuestionId)
    {
        var section = await LoadFormSectionAsync(organisationId, formId, sectionId);
        if (section.Questions == null)
        {
            return null;
        }

        var nextQuestionId = section.Questions.FirstOrDefault(q => q.Id == currentQuestionId)?.NextQuestion;
        return section.Questions.FirstOrDefault(q => q.Id == nextQuestionId);
    }

    public async Task<Models.FormQuestion?> GetPreviousQuestion(Guid organisationId, Guid formId, Guid sectionId, Guid currentQuestionId)
    {

        var section = await LoadFormSectionAsync(organisationId, formId, sectionId);
        if (section.Questions == null)
        {
            return null;
        }

        var previousQuestionId = section.Questions.FirstOrDefault(q => q.NextQuestion == currentQuestionId)?.Id;
        return section.Questions.FirstOrDefault(q => q.Id == previousQuestionId);
    }

    public async Task<Models.FormQuestion?> GetCurrentQuestion(Guid organisationId, Guid formId, Guid sectionId, Guid? questionId)
    {
        var section = await LoadFormSectionAsync(organisationId, formId, sectionId);
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
                dateValue: a.Answer?.DateValue != null ? a.Answer?.DateValue.Value.ToUniversalTime() : null,
                startValue: a.Answer?.StartValue,
                endValue: a.Answer?.EndValue,
                textValue: a.Answer?.TextValue,
                optionValue: a.Answer?.OptionValue,
                questionId: a.QuestionId
            )).ToArray()
        );

        await formsApiClient.PutFormSectionAnswersAsync(
            formId,
            sectionId,
            answerSet.AnswerSetId ?? Guid.NewGuid(),
            organisationId,
            answersPayload);
    }
}