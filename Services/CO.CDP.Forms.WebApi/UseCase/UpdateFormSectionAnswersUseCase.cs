using AutoMapper;
using CO.CDP.AwsServices;
using CO.CDP.Forms.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using Persistence = CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.Forms.WebApi.UseCase;

public class UpdateFormSectionAnswersUseCase(
    IFormRepository formRepository,
    IOrganisationRepository organisationRepository,
    IMapper mapper,
    IFileHostManager fileHostManager)
    : IUseCase<(Guid formId, Guid sectionId, Guid answerSetId, Guid organisationId, List<FormAnswer> answers), bool>
{
    public async Task<bool> Execute((Guid formId, Guid sectionId, Guid answerSetId, Guid organisationId, List<FormAnswer> answers) input)
    {
        var (formId, sectionId, answerSetId, organisationId, answers) = input;

        var organisation = await organisationRepository.Find(organisationId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {organisationId}.");

        var section = await formRepository.GetSectionAsync(formId, sectionId)
            ?? throw new UnknownSectionException($"Unknown section {sectionId} in form {formId}.");

        var sharedConsent = await formRepository.GetSharedConsentWithAnswersAsync(formId, organisationId);
        if (sharedConsent == null)
        {
            sharedConsent = CreateSharedConsent(organisation, section.Form);
        }
        else if (sharedConsent.SubmissionState == Persistence.SubmissionState.Submitted)
        {
            formRepository.ClearTracker();
            sharedConsent.AnswerSets = sharedConsent.AnswerSets.Where(x => x.Section.Type != Persistence.FormSectionType.Declaration).ToList();

            await UpdateOrAddAnswers(formRepository, answerSetId, answers, section, sharedConsent);
            ClearSharedConsent(sharedConsent);
            await formRepository.SaveSharedConsentAsync(sharedConsent);

            return true;
        }
        await UpdateOrAddAnswers(formRepository, answerSetId, answers, section, sharedConsent);
        await formRepository.SaveSharedConsentAsync(sharedConsent);

        return true;
    }

    private static void ClearSharedConsent(Persistence.SharedConsent sharedConsent)
    {
        sharedConsent.SubmittedAt = default;
        sharedConsent.ShareCode = default;
        sharedConsent.SubmissionState = default;
        sharedConsent.Id = default;
        sharedConsent.Guid = Guid.NewGuid();
        foreach (var answerSet in sharedConsent.AnswerSets)
        {
            answerSet.Id = default;
            answerSet.Guid = Guid.NewGuid();
            foreach (var answer in answerSet.Answers)
            {
                answer.Id = default;
                answer.Guid = Guid.NewGuid();
            }
        }
    }

    private async Task UpdateOrAddAnswers(IFormRepository formRepository, Guid answerSetId, List<FormAnswer> answers, Persistence.FormSection section, Persistence.SharedConsent sharedConsent)
    {
        var questionDictionary = section.Questions.ToDictionary(q => q.Guid);

        ValidateQuestions(answers, questionDictionary);

        var answerSet = sharedConsent.AnswerSets.FirstOrDefault(a => a.Guid == answerSetId)
            ?? CreateAnswerSet(answerSetId, sharedConsent, section);

        await UploadFileIfRequired(answers, questionDictionary, answerSet);

        answerSet.Answers = MapAnswers(answers, questionDictionary, answerSet.Answers);
    }

    private async Task UploadFileIfRequired(
        List<FormAnswer> answers,
        Dictionary<Guid, Persistence.FormQuestion> questionDictionary,
        Persistence.FormAnswerSet? existingAnswerSet)
    {
        var fileUploadAnswers = answers.Where(a => questionDictionary[a.QuestionId].Type == Persistence.FormQuestionType.FileUpload);

        foreach (var fileUploadAnswer in fileUploadAnswers)
        {
            var newFilename = fileUploadAnswer.TextValue;
            var existingFilename = existingAnswerSet?.Answers?.FirstOrDefault(a => a.Guid == fileUploadAnswer.Id)?.TextValue;

            if (newFilename != existingFilename)
            {
                if (!string.IsNullOrWhiteSpace(existingFilename))
                {
                    await fileHostManager.RemoveFromPermanentBucket(existingFilename);
                }

                if (!string.IsNullOrWhiteSpace(newFilename))
                {
                    await fileHostManager.CopyToPermanentBucket(newFilename);
                }
            }
        }
    }

    private static void ValidateQuestions(List<FormAnswer> answers, Dictionary<Guid, Persistence.FormQuestion> questionDictionary)
    {
        var invalidQuestions = answers.Select(a => a.QuestionId).Except(questionDictionary.Keys);
        if (invalidQuestions.Any())
        {
            throw new UnknownQuestionsException("One or more questions do not exist in the section.");
        }
    }

    private List<Persistence.FormAnswer> MapAnswers(
        List<FormAnswer> answers,
        Dictionary<Guid, Persistence.FormQuestion> questionDictionary,
        ICollection<Persistence.FormAnswer> existingAnswers)
    {
        List<Persistence.FormAnswer> answersList = [];

        static bool areSameAddress(Persistence.FormAddress? a, FormAddress? b) =>
            (a == null && b == null) || (a != null && b != null
                && $"{a.StreetAddress}{a.Locality}{a.Region ?? ""}{a.PostalCode}{a.Country}"
                    .Equals($"{b.StreetAddress}{b.Locality}{b.Region ?? ""}{b.PostalCode}{b.Country}"));

        foreach (var answer in answers)
        {
            var existingAnswer = existingAnswers.FirstOrDefault(ea => ea.Guid == answer.Id);

            if (existingAnswer != null)
            {
                if (existingAnswer.BoolValue != answer.BoolValue) existingAnswer.BoolValue = answer.BoolValue;
                if (existingAnswer.NumericValue != answer.NumericValue) existingAnswer.NumericValue = answer.NumericValue;
                if (existingAnswer.DateValue != answer.DateValue) existingAnswer.DateValue = answer.DateValue?.ToUniversalTime();
                if (existingAnswer.StartValue != answer.StartValue) existingAnswer.StartValue = answer.StartValue?.ToUniversalTime();
                if (existingAnswer.EndValue != answer.EndValue) existingAnswer.EndValue = answer.EndValue?.ToUniversalTime();
                if (existingAnswer.TextValue != answer.TextValue) existingAnswer.TextValue = answer.TextValue;
                if (existingAnswer.OptionValue != answer.OptionValue) existingAnswer.OptionValue = answer.OptionValue;
                if (!areSameAddress(existingAnswer.AddressValue, answer.AddressValue))
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
                answersList.Add(existingAnswer);
            }
            else
            {
                var formAnswer = mapper.Map<Persistence.FormAnswer>(answer);
                formAnswer.Question = questionDictionary[answer.QuestionId];
                answersList.Add(formAnswer);
            }
        }

        return answersList;
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

    private static Persistence.FormAnswerSet CreateAnswerSet(Guid answerSetId, Persistence.SharedConsent sharedConsent, Persistence.FormSection section)
    {
        Persistence.FormAnswerSet answerSet;
        answerSet = new Persistence.FormAnswerSet
        {
            Guid = answerSetId,
            SharedConsentId = sharedConsent.Id,
            SharedConsent = sharedConsent,
            SectionId = section.Id,
            Section = section,
            Answers = [],
        };
        sharedConsent.AnswerSets.Add(answerSet);
        return answerSet;
    }

}