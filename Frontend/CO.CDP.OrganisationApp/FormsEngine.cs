using CO.CDP.Forms.WebApiClient;
using SectionQuestionsResponse = CO.CDP.OrganisationApp.Models.SectionQuestionsResponse;

namespace CO.CDP.OrganisationApp;

public class FormsEngine(IFormsClient formsApiClient) : IFormsEngine
{
    public async Task<SectionQuestionsResponse> LoadFormSectionAsync(Guid formId, Guid sectionId)
    {
        var response = await formsApiClient.GetFormSectionQuestionsAsync(formId, sectionId);

        // NOTE: we need to store in session the API data SectionQuestionsResponse
        // So we do not have to call the API again again

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
                NextQuestion = q.NextQuestion != null ? new Models.FormQuestion { Id = q.NextQuestion.Id } : null,
                NextQuestionAlternative = q.NextQuestionAlternative != null ? new Models.FormQuestion { Id = q.NextQuestionAlternative.Id } : null,
                Options = new Models.FormQuestionOptions
                {
                    Choices = q.Options.Choices.Select(c => c.Title).ToList(),
                    ChoiceProviderStrategy = q.Options.ChoiceProviderStrategy
                }
            }).ToList()
        };

        return sectionQuestionsResponse;
    }

    public async Task<Models.FormQuestion?> GetNextQuestion(Guid formId, Guid sectionId, Guid currentQuestionId)
    {
        var section = await LoadFormSectionAsync(formId, sectionId);
        var questions = section.Questions;

        if (questions == null)
        {
            return null;
        }

        var currentIndex = questions.FindIndex(q => q.Id == currentQuestionId);

        if (currentIndex >= 0 && currentIndex < questions.Count - 1)
        {
            return questions[currentIndex + 1];
        }

        return null;
    }

    public async Task<Models.FormQuestion?> GetPreviousQuestion(Guid formId, Guid sectionId, Guid currentQuestionId)
    {
        var section = await LoadFormSectionAsync(formId, sectionId);
        var questions = section.Questions;

        if (questions == null)
        {
            return null;
        }

        var currentIndex = questions.FindIndex(q => q.Id == currentQuestionId);

        if (currentIndex > 0)
        {
            return questions[currentIndex - 1];
        }

        return null;
    }

    public async Task<Models.FormQuestion?> GetCurrentQuestion(Guid formId, Guid sectionId, Guid questionId)
    {
        var section = await LoadFormSectionAsync(formId, sectionId);
        return section.Questions?.FirstOrDefault(q => q.Id == questionId);
    }
}