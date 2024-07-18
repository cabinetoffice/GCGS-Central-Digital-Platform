using CO.CDP.Forms.WebApiClient;
using SectionQuestionsResponse = CO.CDP.OrganisationApp.Models.SectionQuestionsResponse;

namespace CO.CDP.OrganisationApp;

public class FormsEngine(IFormsClient formsApiClient, ITempDataService tempDataService) : IFormsEngine
{
    public async Task<SectionQuestionsResponse> LoadFormSectionAsync(Guid formId, Guid sectionId)
    {
        var sessionKey = $"SectionQuestionsResponse_{formId}_{sectionId}";
        var cachedResponse = tempDataService.Get<SectionQuestionsResponse>(sessionKey);

        if (cachedResponse != null)
        {
            return cachedResponse;
        }

        var response = await formsApiClient.GetFormSectionQuestionsAsync(formId, sectionId);

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

        tempDataService.Put(sessionKey, sectionQuestionsResponse);
        return sectionQuestionsResponse;
    }

    public async Task<Models.FormQuestion?> GetNextQuestion(Guid formId, Guid sectionId, Guid? currentQuestionId)
    {
        return await GetAdjacentQuestion(formId, sectionId, currentQuestionId, 1);
    }

    public async Task<Models.FormQuestion?> GetPreviousQuestion(Guid formId, Guid sectionId, Guid? currentQuestionId)
    {
        return await GetAdjacentQuestion(formId, sectionId, currentQuestionId, -1);
    }

    private async Task<Models.FormQuestion?> GetAdjacentQuestion(Guid formId, Guid sectionId, Guid? currentQuestionId, int offset)
    {
        var section = await LoadFormSectionAsync(formId, sectionId);
        if (section.Questions == null)
        {
            return null;
        }

        var currentIndex = 0;
        if (currentQuestionId == null)
        {
            if (offset == -1)
            {
                return null;
            }
            currentIndex = 0;
        }
        else
        {
            currentIndex = section.Questions.FindIndex(q => q.Id == currentQuestionId);
        }

        var newIndex = currentIndex + offset;
        if (newIndex >= 0 && newIndex < section.Questions.Count)
        {
            return section.Questions[newIndex];
        }

        return null;
    }

    public async Task<Models.FormQuestion?> GetCurrentQuestion(Guid formId, Guid sectionId, Guid? questionId)
    {
        return await GetAdjacentQuestion(formId, sectionId, questionId, 0);
        //var section = await LoadFormSectionAsync(formId, sectionId);

        //if (questionId.HasValue)
        //{
        //    return section.Questions?.FirstOrDefault(q => q.Id == questionId);
        //}
        //else
        //{
        //    return section.Questions?.FirstOrDefault();
        //}
    }
}