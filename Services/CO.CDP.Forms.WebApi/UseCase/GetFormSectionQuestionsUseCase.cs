using AutoMapper;
using CO.CDP.Forms.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Forms.WebApi.UseCase;

public class GetFormSectionQuestionsUseCase(IFormRepository formRepository, IMapper mapper)
    : IUseCase<(Guid formId, Guid sectionId), SectionQuestionsResponse?>
{

    public async Task<SectionQuestionsResponse?> Execute((Guid formId, Guid sectionId) input)
    {
        var (formId, sectionId) = input;

        var section = await formRepository.GetSectionAsync(formId, sectionId);

        if (section == null)
            return null;

        var questions = mapper.Map<List<FormQuestion>>(section.Questions);
        var answers = GetMockAnswers(questions);

        return new SectionQuestionsResponse
        {
            Section = mapper.Map<FormSection>(section),
            Questions = questions,
            AnswerSet = answers
        };
    }

    private FormAnswerSet GetMockAnswers(List<FormQuestion> questions)
    {
        var answers = new List<FormAnswer>();

        foreach (var question in questions)
        {
            FormAnswer? answer = question.Type switch
            {
                FormQuestionType.YesOrNo => new FormAnswer
                {
                    Id = Guid.NewGuid(),
                    QuestionId = question.Id,
                    BoolValue = true
                },

                FormQuestionType.Text => new FormAnswer
                {
                    Id = Guid.NewGuid(),
                    QuestionId = question.Id,
                    TextValue = "Our financial status is stable with a steady growth."
                },

                FormQuestionType.FileUpload => new FormAnswer
                {
                    Id = Guid.NewGuid(),
                    QuestionId = question.Id,
                    TextValue = "file://path/to/uploaded/accounts.pdf"
                },

                FormQuestionType.Date => new FormAnswer
                {
                    Id = Guid.NewGuid(),
                    QuestionId = question.Id,
                    DateValue = DateTime.Today
                },

                FormQuestionType.SingleChoice => new FormAnswer
                {
                    Id = Guid.NewGuid(),
                    QuestionId = question.Id,
                    OptionValue = "Yes"
                },

                FormQuestionType.MultipleChoice => new FormAnswer
                {
                    Id = Guid.NewGuid(),
                    QuestionId = question.Id,
                    OptionValue = "accountsLastTwoYearsAudited"
                },

                FormQuestionType.CheckYourAnswers => new FormAnswer
                {
                    Id = Guid.NewGuid(),
                    QuestionId = question.Id,
                    TextValue = "Please check your answers"
                },

                _ => null
            };

            if (answer != null)
            {
                answers.Add(answer);
            }
        }

        return new FormAnswerSet
        {
            Id = Guid.NewGuid(),
            Answers = answers
        };
    }
}