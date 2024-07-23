using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CO.CDP.Forms.WebApi.Model;
using CO.CDP.Authentication;
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
            Answers = answers
        };
    }

    private List<FormAnswer> GetMockAnswers(List<FormQuestion> questions)
    {
        var answerSetGuid = Guid.NewGuid();
        var formAnswerSet = new FormAnswerSet
        {
            Id = answerSetGuid,
            Section = questions.First().Section,
            Answers = new List<FormAnswer>()
        };

        var answers = new List<FormAnswer>();

        foreach (var question in questions)
        {
            FormAnswer? answer = question.Type switch
            {
                FormQuestionType.YesOrNo => new FormAnswer
                {
                    Id = Guid.NewGuid(),
                    Question = question,
                    FormAnswerSet = formAnswerSet,
                    BoolValue = true
                },

                FormQuestionType.Text => new FormAnswer
                {
                    Id = Guid.NewGuid(),
                    Question = question,
                    FormAnswerSet = formAnswerSet,
                    TextValue = "Our financial status is stable with a steady growth."
                },

                FormQuestionType.FileUpload => new FormAnswer
                {
                    Id = Guid.NewGuid(),
                    Question = question,
                    FormAnswerSet = formAnswerSet,
                    TextValue = "file://path/to/uploaded/accounts.pdf"
                },

                FormQuestionType.Date => new FormAnswer
                {
                    Id = Guid.NewGuid(),
                    Question = question,
                    FormAnswerSet = formAnswerSet,
                    DateValue = DateTime.Today
                },

                FormQuestionType.SingleChoice => new FormAnswer
                {
                    Id = Guid.NewGuid(),
                    Question = question,
                    FormAnswerSet = formAnswerSet,
                    OptionValue = "Yes"
                },

                FormQuestionType.MultipleChoice => new FormAnswer
                {
                    Id = Guid.NewGuid(),
                    Question = question,
                    FormAnswerSet = formAnswerSet,
                    OptionValue = "accountsLastTwoYearsAudited"
                },

                FormQuestionType.CheckYourAnswers => new FormAnswer
                {
                    Id = Guid.NewGuid(),
                    Question = question,
                    FormAnswerSet = formAnswerSet,
                    TextValue = "Please check your answers"
                },

                _ => null
            };

            if (answer != null)
            {
                answers.Add(answer);
            }
        }

        formAnswerSet.Answers.AddRange(answers);

        return answers;
    }

}

