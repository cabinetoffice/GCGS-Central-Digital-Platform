using AutoMapper;
using CO.CDP.Forms.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using Persistence = CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.Forms.WebApi.UseCase;

public class UpdateFormSectionAnswersUseCase(
    IFormRepository formRepository,
    IOrganisationRepository organisationRepository,
    IMapper mapper)
    : IUseCase<(Guid formId, Guid sectionId, Guid answerSetId, Guid organisationId, UpdateFormSectionAnswers updateFormSectionAnswers), bool>
{
    public async Task<bool> Execute((Guid formId, Guid sectionId, Guid answerSetId, Guid organisationId, UpdateFormSectionAnswers updateFormSectionAnswers) input)
    {
        var (formId, sectionId, answerSetId, organisationId, updateFormSectionAnswers) = input;

        var organisation = await organisationRepository.Find(organisationId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {organisationId}.");

        var section = await formRepository.GetSectionAsync(formId, sectionId)
            ?? throw new UnknownSectionException($"Unknown section {sectionId} in form {formId}.");

        var sharedConsent = await formRepository.GetSharedConsentWithAnswersAsync(formId, organisationId)
            ?? CreateSharedConsent(organisation, section.Form);

        var (updatedSharedConsent, mappedAnswerSetId, updatedAnswers) =
            MapSharedConsent(sharedConsent, answerSetId, updateFormSectionAnswers.Answers ?? []);

        UpdateOrAddAnswers(mappedAnswerSetId, updatedAnswers, section, updatedSharedConsent, updateFormSectionAnswers.FurtherQuestionsExempted);
        UpdateAnswerSets(updatedSharedConsent);
        await formRepository.SaveSharedConsentAsync(updatedSharedConsent);
        return true;
    }

    private static void UpdateAnswerSets(Persistence.SharedConsent updatedSharedConsent)
    {
        foreach (var answerSets in updatedSharedConsent.AnswerSets.GroupBy(s => s.SectionId))
        {
            var activeAnswerSets = answerSets.Where(a => !a.Deleted);

            var answerSetsWithFurtherQuestionsExempted =
                activeAnswerSets.Where(a => a.FurtherQuestionsExempted == true);

            var answerSetsWithoutFurtherQuestionsExemptedCount =
                activeAnswerSets.Count(a => a.FurtherQuestionsExempted == false);

            if (answerSetsWithoutFurtherQuestionsExemptedCount > 0 && answerSetsWithFurtherQuestionsExempted.Any())
            {
                foreach (var answetSetWithFurtherQuestionsExempted in answerSetsWithFurtherQuestionsExempted)
                {
                    answetSetWithFurtherQuestionsExempted.Deleted = true;
                }
            }
        }
    }

    private static (Persistence.SharedConsent, Guid, List<FormAnswer>) MapSharedConsent(
        Persistence.SharedConsent sharedConsent, Guid answerSetId, List<FormAnswer> answers)
    {
        var newSharedConsent = Persistence.SharedConsentMapper.Map(sharedConsent);
        var allAnswers = newSharedConsent.AnswerSets.SelectMany(a => a.Answers);
        var newRequestAnswers = answers.Select(answer =>
        {
            var matched = allAnswers.FirstOrDefault(a => a.CreatedFrom == answer.Id);
            if (matched is not null)
            {
                return answer with { Id = matched.Guid };
            }

            return answer;
        }).ToList();
        return (
            newSharedConsent,
            newSharedConsent.AnswerSets
                .Where(s => s.CreatedFrom == answerSetId)
                .Select(a => a.Guid)
                .FirstOrDefault(answerSetId),
            newRequestAnswers
        );
    }

    private void UpdateOrAddAnswers(
        Guid answerSetId,
        List<FormAnswer> answers,
        Persistence.FormSection section,
        Persistence.SharedConsent sharedConsent,
        bool furtherQuestionsExempted)
    {
        var questionDictionary = section.Questions.ToDictionary(q => q.Guid);

        ValidateQuestions(answers, questionDictionary);

        var answerSet = sharedConsent.AnswerSets.FirstOrDefault(a => a.Guid == answerSetId)
            ?? CreateAnswerSet(answerSetId, sharedConsent, section, furtherQuestionsExempted);

        answerSet.Answers = MapAnswers(answers, questionDictionary, answerSet.Answers);
    }

    private static void ValidateQuestions(List<FormAnswer> answers, Dictionary<Guid, Persistence.FormQuestion> questionDictionary)
    {
        var invalidQuestions = answers.Select(a => a.QuestionId).Except(questionDictionary.Keys);
        if (invalidQuestions.Any())
        {
            throw new UnknownQuestionsException("One or more questions do not exist in the section.");
        }
    }

    private ICollection<Persistence.FormAnswer> MapAnswers(
        List<FormAnswer> answers,
        Dictionary<Guid, Persistence.FormQuestion> questionDictionary,
        ICollection<Persistence.FormAnswer> existingAnswers)
    {
        // first, include answers that were already there and update them if necessary
        var answersList = existingAnswers
            .Select(ea => UpdateAnswerValues(ea, answers.FirstOrDefault(a => a.Id == ea.Guid)));

        // next, include new answers making sure to filter updates to the existing ones (those were added in the previous step)
        var newAnswerList = answers
            .FindAll(answer => existingAnswers.All(ea => ea.Guid != answer.Id))
            .Select(answer =>
                {
                    var newAnswer = mapper.Map<Persistence.FormAnswer>(answer);
                    newAnswer.Guid = Guid.NewGuid();
                    newAnswer.QuestionId = questionDictionary[answer.QuestionId].Id;
                    newAnswer.Question = questionDictionary[answer.QuestionId];
                    newAnswer.FormAnswerSetId = default;

                    return newAnswer;
                }
            );

        return answersList.Concat(newAnswerList).ToList();
    }

    private static Persistence.FormAnswer UpdateAnswerValues(Persistence.FormAnswer existingAnswer, FormAnswer? answer)
    {
        if (answer is null)
        {
            return existingAnswer;
        }
        static bool AreSameAddress(Persistence.FormAddress? a, FormAddress? b) =>
            (a == null && b == null) || (a != null && b != null
                                                   && $"{a.StreetAddress}{a.Locality}{a.Region ?? ""}{a.PostalCode}{a.Country}"
                                                       .Equals($"{b.StreetAddress}{b.Locality}{b.Region ?? ""}{b.PostalCode ?? ""}{b.Country}"));

        if (existingAnswer.BoolValue != answer.BoolValue)
            existingAnswer.BoolValue = answer.BoolValue;
        if (existingAnswer.NumericValue != answer.NumericValue)
            existingAnswer.NumericValue = answer.NumericValue;
        if (existingAnswer.DateValue != answer.DateValue)
            existingAnswer.DateValue = answer.DateValue?.ToUniversalTime();
        if (existingAnswer.StartValue != answer.StartValue)
            existingAnswer.StartValue = answer.StartValue?.ToUniversalTime();
        if (existingAnswer.EndValue != answer.EndValue)
            existingAnswer.EndValue = answer.EndValue?.ToUniversalTime();
        if (existingAnswer.TextValue != answer.TextValue)
            existingAnswer.TextValue = answer.TextValue;
        if (existingAnswer.OptionValue != answer.OptionValue)
            existingAnswer.OptionValue = answer.OptionValue;
        if (existingAnswer.JsonValue != answer.JsonValue)
            existingAnswer.JsonValue = answer.JsonValue;
        if (!AreSameAddress(existingAnswer.AddressValue, answer.AddressValue))
        {
            existingAnswer.AddressValue = answer.AddressValue == null ? null : new Persistence.FormAddress
            {
                StreetAddress = answer.AddressValue.StreetAddress,
                Locality = answer.AddressValue.Locality,
                PostalCode = answer.AddressValue.PostalCode,
                Region = answer.AddressValue.Region,
                CountryName = answer.AddressValue.CountryName,
                Country = answer.AddressValue.Country
            };
        }
        return existingAnswer;
    }

    private Persistence.SharedConsent CreateSharedConsent(Organisation organisation, Persistence.Form form)
    {
        return new Persistence.SharedConsent
        {
            Guid = Guid.NewGuid(),
            OrganisationId = organisation.Id,
            Organisation = organisation,
            Form = form,
            FormId = form.Id,
            FormVersionId = form.Version,
            SubmissionState = Persistence.SubmissionState.Draft,
            SubmittedAt = DateTimeOffset.UtcNow,
            AnswerSets = new List<Persistence.FormAnswerSet>()
        };
    }

    private static Persistence.FormAnswerSet CreateAnswerSet(
        Guid answerSetId,
        Persistence.SharedConsent sharedConsent,
        Persistence.FormSection section,
        bool furtherQuestionsExempted)
    {
        var answerSet = new Persistence.FormAnswerSet
        {
            Guid = answerSetId,
            SharedConsentId = sharedConsent.Id,
            SharedConsent = sharedConsent,
            SectionId = section.Id,
            Section = section,
            Answers = [],
            FurtherQuestionsExempted = furtherQuestionsExempted,
        };
        sharedConsent.AnswerSets.Add(answerSet);
        return answerSet;
    }

}