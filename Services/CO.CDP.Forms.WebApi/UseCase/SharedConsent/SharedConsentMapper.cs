using CO.CDP.Forms.WebApi.Model;
using static CO.CDP.OrganisationInformation.Persistence.Forms.FormSectionType;
using static CO.CDP.OrganisationInformation.Persistence.Forms.SubmissionState;
using Persistence = CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.Forms.WebApi.UseCase.SharedConsent;

public static class SharedConsentMapper
{
    public static (Persistence.SharedConsent, Guid, List<FormAnswer>) Map(
        Persistence.SharedConsent sharedConsent,
        Guid answerSetId,
        List<FormAnswer> requestAnswers
    )
    {
        if (sharedConsent.SubmissionState != Submitted)
        {
            return (sharedConsent, answerSetId, requestAnswers);
        }

        var oldToNewGuids = new Dictionary<Guid, Guid>();

        var newSharedConsent = CreateNewSharedConsent(sharedConsent);

        foreach (var answerSet in sharedConsent.AnswerSets.Where(x => x.Section.Type != Declaration))
        {
            var newAnswerSet = CreateNewAnswerSet(newSharedConsent, answerSet);
            oldToNewGuids.Add(answerSet.Guid, newAnswerSet.Guid);

            foreach (var answer in answerSet.Answers)
            {
                var newAnswer = CreateNewAnswer(newAnswerSet, answer);
                oldToNewGuids.Add(answer.Guid, newAnswer.Guid);
                newAnswerSet.Answers.Add(newAnswer);
            }

            newSharedConsent.AnswerSets.Add(newAnswerSet);
        }

        var newRequestAnswers = requestAnswers.Select(answer =>
        {
            if (oldToNewGuids.TryGetValue(answer.Id, out var newId))
            {
                return answer with { Id = newId };
            }

            return answer;
        }).ToList();

        return (newSharedConsent, oldToNewGuids.GetValueOrDefault(answerSetId, answerSetId), newRequestAnswers);
    }

    private static Persistence.FormAnswer CreateNewAnswer(
        Persistence.FormAnswerSet newAnswerSet,
        Persistence.FormAnswer answer
    ) => new()
    {
        Guid = Guid.NewGuid(),
        QuestionId = answer.QuestionId,
        Question = answer.Question,
        FormAnswerSetId = newAnswerSet.Id,
        FormAnswerSet = newAnswerSet,
        BoolValue = answer.BoolValue,
        DateValue = answer.DateValue,
        TextValue = answer.TextValue,
        OptionValue = answer.OptionValue,
        NumericValue = answer.NumericValue,
        StartValue = answer.StartValue,
        EndValue = answer.EndValue,
        AddressValue = answer.AddressValue
    };

    private static Persistence.SharedConsent CreateNewSharedConsent(Persistence.SharedConsent sharedConsent) =>
        new()
        {
            Id = default,
            Guid = Guid.NewGuid(),
            OrganisationId = sharedConsent.OrganisationId,
            Organisation = sharedConsent.Organisation,
            FormId = sharedConsent.FormId,
            Form = sharedConsent.Form,
            SubmissionState = Draft,
            FormVersionId = sharedConsent.FormVersionId
        };

    private static Persistence.FormAnswerSet CreateNewAnswerSet(
        Persistence.SharedConsent sharedConsent, Persistence.FormAnswerSet answerSet
    ) => new()
    {
        Guid = Guid.NewGuid(),
        SharedConsentId = sharedConsent.Id,
        SharedConsent = sharedConsent,
        SectionId = answerSet.SectionId,
        Section = answerSet.Section,
        FurtherQuestionsExempted = answerSet.FurtherQuestionsExempted
    };
}