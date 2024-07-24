using CO.CDP.Forms.WebApiClient;
using SectionQuestionsResponse = CO.CDP.OrganisationApp.Models.SectionQuestionsResponse;

namespace CO.CDP.OrganisationApp;

public class FormsEngine(IFormsClient formsApiClient, ITempDataService tempDataService) : IFormsEngine
{
    //private readonly Guid SpecialQuestionId = new Guid("e2dce4aa-fc5e-4471-be32-1124d7221aaf");
    public async Task<SectionQuestionsResponse> LoadFormSectionAsync(Guid organisationId, Guid formId, Guid sectionId)
    {
        var sessionKey = $"Form_{organisationId}_{formId}_{sectionId}_Questions";
        var cachedResponse = tempDataService.Peek<SectionQuestionsResponse>(sessionKey);

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

    public async Task<Models.FormQuestion?> GetNextQuestion(Guid organisationId, Guid formId, Guid sectionId, Guid? currentQuestionId)
    {
        return await GetAdjacentQuestion(organisationId, formId, sectionId, currentQuestionId, 1);
    }

    public async Task<Models.FormQuestion?> GetPreviousQuestion(Guid organisationId, Guid formId, Guid sectionId, Guid? currentQuestionId)
    {
        return await GetAdjacentQuestion(organisationId, formId, sectionId, currentQuestionId, -1);
    }

    public async Task<Models.FormQuestion?> GetCurrentQuestion(Guid organisationId, Guid formId, Guid sectionId, Guid? questionId)
    {
        return await GetAdjacentQuestion(organisationId, formId, sectionId, questionId, 0);
    }

    public async Task<bool> IsCheckYourAnswersPage(Guid organisationId, Guid formId, Guid sectionId, Guid? questionId)
    {
        var currentQuestion = await GetCurrentQuestion(organisationId, formId, sectionId, questionId);
        return currentQuestion?.IsCheckYourAnswers ?? false;
    }

    private async Task<Models.FormQuestion?> GetAdjacentQuestion(Guid organisationId, Guid formId, Guid sectionId, Guid? currentQuestionId, int offset)
    {
        var section = await LoadFormSectionAsync(organisationId, formId, sectionId);
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
}