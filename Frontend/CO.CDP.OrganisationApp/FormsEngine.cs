using CO.CDP.OrganisationApp.Models;

namespace CO.CDP.OrganisationApp;

//public class FormEngin(FormsApiClient formsApiClient)
//{

//    public async Task<SectionQuestionsResponse> LoadFormSectionAsync(Guid formId, Guid sectionId)    
//    {
//        return await formsApiClient.GetFormSectionWithQuestionsAsync(formId, sectionId);
//    }

//}

// NOTE: we need to store in session the API data SectionQuestionsResponse
// So we do not have to call the API again again

public class FormsEngine : IFormsEngine
{
    private readonly Dictionary<Guid, SectionQuestionsResponse> _hardcodedResponses;

    public FormsEngine()
    {
        // Initialize with some hardcoded responses for demonstration
        _hardcodedResponses = new Dictionary<Guid, SectionQuestionsResponse>
            {
                {
                    Guid.Parse("00000000-0000-0000-0000-000000000000"),
                    new SectionQuestionsResponse
                    {
                        Section = new FormSection
                        {
                            Id = Guid.Parse("00000000-0000-0000-0000-000000000000"),
                            Title = "Financial Information",
                            AllowsMultipleAnswerSets = true
                        },
                        Questions = new List<FormQuestion>
                        {
                            new FormQuestion
                            {
                                Id = Guid.NewGuid(),
                                Title = "The financial information you will need",
                                Description = "Upload accounts or statements for your 2 most recent financial years. If you do not have 2 years, you can upload your most recent financial year. You will need to enter the financial year end date.",
                                Type = FormQuestionType.NoInput,
                                IsRequired = true,
                                Options = new FormQuestionOptions()
                            },
                            new FormQuestion
                            {
                                Id = Guid.NewGuid(),
                                Title = "Were your accounts audited?",
                                Type = FormQuestionType.YesOrNo,
                                IsRequired = true,
                                Options = new FormQuestionOptions()
                            },
                            new FormQuestion
                            {
                                Id = Guid.NewGuid(),
                                Title = "Upload your accounts",
                                Description = "Upload your most recent 2 financial years. If you do not have  2, upload your most recent financial year.",
                                Type = FormQuestionType.FileUpload,
                                IsRequired = true,
                                Options = new FormQuestionOptions()
                            },
                            new FormQuestion
                            {
                                Id = Guid.NewGuid(),
                                Title = "What is the financial year end date for the information you uploaded?",
                                Type = FormQuestionType.Date,
                                IsRequired = true,
                                Options = new FormQuestionOptions()
                            }
                        }
                    }
                }
            };
    }


    // Temporary hardcoded response until we get the FormsApiClient ready
    public async Task<SectionQuestionsResponse> LoadFormSectionAsync(Guid formId, Guid sectionId)
    {
        if (_hardcodedResponses.TryGetValue(sectionId, out var response))
        {
            return await Task.FromResult(response);
        }

        return new SectionQuestionsResponse
        {
            Section = new FormSection
            {
                Id = sectionId,
                Title = "Unknown Section",
                AllowsMultipleAnswerSets = false
            },
            Questions = new List<FormQuestion>()
        };
    }
    public async Task<FormQuestion?> GetNextQuestion(Guid formId, Guid sectionId, Guid currentQuestionId)
    {
        var section = await LoadFormSectionAsync(formId, sectionId);
        var questions = section.Questions;
        var currentIndex = questions.FindIndex(q => q.Id == currentQuestionId);

        if (currentIndex >= 0 && currentIndex < questions.Count - 1)
        {
            return questions[currentIndex + 1];
        }

        return null;
    }

    public async Task<FormQuestion?> GetPreviousQuestion(Guid formId, Guid sectionId, Guid currentQuestionId)
    {
        var section = await LoadFormSectionAsync(formId, sectionId);
        var questions = section.Questions;
        var currentIndex = questions.FindIndex(q => q.Id == currentQuestionId);

        if (currentIndex > 0)
        {
            return questions[currentIndex - 1];
        }

        return null;
    }

    public async Task<FormQuestion?> GetCurrentQuestion(Guid formId, Guid sectionId, Guid questionId)
    {
        var section = await LoadFormSectionAsync(formId, sectionId);
        return section.Questions?.FirstOrDefault(q => q.Id == questionId);
    }
}