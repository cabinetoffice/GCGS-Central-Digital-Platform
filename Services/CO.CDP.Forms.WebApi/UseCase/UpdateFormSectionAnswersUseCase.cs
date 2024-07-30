using AutoMapper;
using CO.CDP.Forms.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using Persistence = CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.Forms.WebApi.UseCase;

public class UpdateFormSectionAnswersUseCase(IFormRepository formRepository, IOrganisationRepository organisationRepository, IMapper mapper)
    : IUseCase<(Guid formId, Guid sectionId, Guid answerSetId, Guid organisationId, List<FormAnswer> answers), bool>
{
    public async Task<bool> Execute((Guid formId, Guid sectionId, Guid answerSetId, Guid organisationId, List<FormAnswer> answers) input)
    {
        var (formId, sectionId, answerSetId, organisationId, answers) = input;

        var organisation = await organisationRepository.Find(organisationId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {organisationId}.");

        var section = await formRepository.GetSectionAsync(formId, sectionId)
            ?? throw new UnknownSectionException($"Unknown section {sectionId} in form {formId}.");


        var questionDictionary = section.Questions.ToDictionary(q => q.Guid);

        ValidateQuestions(answers, questionDictionary);

        var existingAnswerSet = await formRepository.GetFormAnswerSetAsync(sectionId, organisationId, answerSetId);

        if (existingAnswerSet != null)
        {
            existingAnswerSet.Answers = MapAnswers(answers, questionDictionary);
        }
        else
        {
            existingAnswerSet = new Persistence.FormAnswerSet
            {
                Guid = answerSetId,
                OrganisationId = organisation.Id,
                Organisation = organisation,
                Section = section,
                Answers = MapAnswers(answers, questionDictionary),
            };
        }

        await formRepository.SaveAnswerSet(existingAnswerSet);

        return true;
    }

    private void ValidateQuestions(List<FormAnswer> answers, Dictionary<Guid, Persistence.FormQuestion> questionDictionary)
    {
        var invalidQuestions = answers.Select(a => a.QuestionId).Except(questionDictionary.Keys).ToList();
        if (invalidQuestions.Any())
        {
            throw new UnknownQuestionsException("One or more questions do not exist in the section.");
        }
    }

    private List<Persistence.FormAnswer> MapAnswers(List<FormAnswer> answers, Dictionary<Guid, Persistence.FormQuestion> questionDictionary)
    {
        return answers.Select(answer =>
        {
            var formAnswer = mapper.Map<Persistence.FormAnswer>(answer);
            formAnswer.Question = questionDictionary[answer.QuestionId];
            return formAnswer;
        }).ToList();
    }
}