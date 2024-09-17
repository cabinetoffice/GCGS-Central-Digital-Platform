using static CO.CDP.OrganisationInformation.Persistence.Forms.FormSectionType;
using static CO.CDP.OrganisationInformation.Persistence.Forms.SubmissionState;

namespace CO.CDP.OrganisationInformation.Persistence.Forms;

public static class SharedConsentMapper
{
    public static SharedConsent Map(SharedConsent sharedConsent)
    {
        if (sharedConsent.SubmissionState != Submitted)
        {
            return sharedConsent;
        }

        var newSharedConsent = CreateNewSharedConsent(sharedConsent);

        foreach (var answerSet in sharedConsent.AnswerSets.Where(x => x.Section.Type != Declaration))
        {
            var newAnswerSet = CreateNewAnswerSet(newSharedConsent, answerSet);

            foreach (var answer in answerSet.Answers)
            {
                newAnswerSet.Answers.Add(CreateNewAnswer(newAnswerSet, answer));
            }

            newSharedConsent.AnswerSets.Add(newAnswerSet);
        }

        return newSharedConsent;
    }

    private static FormAnswer CreateNewAnswer(
        FormAnswerSet newAnswerSet,
        FormAnswer answer
    ) => new()
    {
        Guid = Guid.NewGuid(),
        CreatedFrom = answer.Guid,
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

    private static SharedConsent CreateNewSharedConsent(SharedConsent sharedConsent) =>
        new()
        {
            Id = default,
            Guid = Guid.NewGuid(),
            CreatedFrom = sharedConsent.Guid,
            OrganisationId = sharedConsent.OrganisationId,
            Organisation = sharedConsent.Organisation,
            FormId = sharedConsent.FormId,
            Form = sharedConsent.Form,
            SubmissionState = Draft,
            FormVersionId = sharedConsent.FormVersionId
        };

    private static FormAnswerSet CreateNewAnswerSet(
        SharedConsent sharedConsent, FormAnswerSet answerSet
    ) => new()
    {
        Guid = Guid.NewGuid(),
        CreatedFrom = answerSet.Guid,
        SharedConsentId = sharedConsent.Id,
        SharedConsent = sharedConsent,
        SectionId = answerSet.SectionId,
        Section = answerSet.Section,
        Deleted = answerSet.Deleted
    };
}