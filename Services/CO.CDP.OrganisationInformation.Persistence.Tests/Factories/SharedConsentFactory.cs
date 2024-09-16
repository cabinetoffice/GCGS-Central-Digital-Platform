using static CO.CDP.OrganisationInformation.Persistence.Forms.FormSectionType;
using static CO.CDP.OrganisationInformation.Persistence.Forms.SubmissionState;
using static CO.CDP.OrganisationInformation.Persistence.Forms.FormQuestionType;

namespace CO.CDP.OrganisationInformation.Persistence.Tests.Factories;

public static class SharedConsentFactory
{
    public static Persistence.Forms.SharedConsent GivenSharedConsent(
        Organisation? organisation = null,
        Persistence.Forms.Form? form = null,
        Persistence.Forms.SubmissionState? state = null)
    {
        var theOrganisation = organisation ?? GivenOrganisation();
        var theForm = form ?? GivenForm();
        return new Persistence.Forms.SharedConsent()
        {
            Guid = Guid.NewGuid(),
            OrganisationId = theOrganisation.Id,
            Organisation = theOrganisation,
            FormId = theForm.Id,
            Form = theForm,
            AnswerSets = [],
            SubmissionState = state ?? Draft,
            SubmittedAt = null,
            FormVersionId = "202405",
            ShareCode = null
        };
    }

    public static Persistence.Forms.Form GivenForm()
    {
        return new Persistence.Forms.Form
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            Name = "Sample Form",
            Version = "1.0",
            IsRequired = true,
            Scope = Persistence.Forms.FormScope.SupplierInformation,
            Sections = new List<Persistence.Forms.FormSection>()
        };
    }

    public static Persistence.Forms.FormSection GivenFormSection(
        Guid? sectionId = null,
        Persistence.Forms.Form? form = null,
        List<Persistence.Forms.FormQuestion>? questions = null,
        Persistence.Forms.FormSectionType type = Standard
    )
    {
        var theForm = form ?? GivenForm();
        var formSection = new Persistence.Forms.FormSection
        {
            Id = 1,
            Guid = sectionId ?? Guid.NewGuid(),
            Title = "Financial Information",
            FormId = theForm.Id,
            Form = theForm,
            Questions = questions ?? [],
            Type = type,
            AllowsMultipleAnswerSets = true,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow,
            Configuration = new Persistence.Forms.FormSectionConfiguration
            {
                PluralSummaryHeadingFormat = "You have added {0} files",
                SingularSummaryHeading = "You have added 1 file",
                AddAnotherAnswerLabel = "Add another file?",
                RemoveConfirmationCaption = "Economic and Financial Standing",
                RemoveConfirmationHeading = "Are you sure you want to remove this file?"
            }
        };
        questions?.ForEach(q => q.Section = formSection);
        theForm.Sections.Add(formSection);
        return formSection;
    }

    public static Persistence.Forms.FormQuestion GivenFormQuestion(
        Persistence.Forms.FormSection? section = null,
        Guid? questionId = null,
        Persistence.Forms.FormQuestionType? type = null
    )
    {
        var theSection = section ?? GivenFormSection();
        var question = new Persistence.Forms.FormQuestion
        {
            Guid = questionId ?? Guid.NewGuid(),
            Title = "Were your accounts audited?",
            Caption = "",
            Description = "",
            Type = type ?? YesOrNo,
            IsRequired = true,
            NextQuestion = null,
            NextQuestionAlternative = null,
            Options = new Persistence.Forms.FormQuestionOptions(),
            Section = theSection,
        };
        theSection.Questions.Add(question);
        return question;
    }

    public static Persistence.Forms.FormAnswerSet GivenAnswerSet(
        Persistence.Forms.SharedConsent sharedConsent,
        Persistence.Forms.FormSection? section = null,
        List<Persistence.Forms.FormAnswer>? answers = null
    )
    {
        var theSection = section ?? GivenFormSection();
        var existingAnswerSet = new Persistence.Forms.FormAnswerSet
        {
            Guid = Guid.NewGuid(),
            SharedConsentId = sharedConsent.Id,
            SharedConsent = sharedConsent,
            SectionId = theSection.Id,
            Section = theSection,
            Answers = answers ?? []
        };
        sharedConsent.AnswerSets.Add(existingAnswerSet);
        answers?.ForEach(a => a.FormAnswerSet = existingAnswerSet);
        return existingAnswerSet;
    }

    public static Persistence.Forms.FormAnswer GivenAnswer(
        Persistence.Forms.FormQuestion? question = null,
        bool? boolValue = null,
        double? numericValue = null,
        DateTime? dateValue = null,
        DateTime? startValue = null,
        DateTime? endValue = null,
        string? textValue = null,
        string? optionValue = null,
        Persistence.Forms.FormAddress? addressValue = null
    )
    {
        var theQuestion = question ?? GivenFormQuestion();
        return new Persistence.Forms.FormAnswer
        {
            Id = 0,
            Guid = Guid.NewGuid(),
            QuestionId = theQuestion.Id,
            Question = theQuestion,
            FormAnswerSetId = default,
            BoolValue = boolValue,
            NumericValue = numericValue,
            DateValue = dateValue,
            StartValue = startValue,
            EndValue = endValue,
            TextValue = textValue,
            OptionValue = optionValue,
            AddressValue = addressValue,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow,
        };
    }

    public static Organisation GivenOrganisation(Guid? organisationId = null)
    {
        var theOrganisationId = organisationId ?? Guid.NewGuid();
        return new Organisation
        {
            Guid = theOrganisationId,
            Name = $"Test Organisation {theOrganisationId}",
            Tenant = new Tenant
            {
                Guid = Guid.NewGuid(),
                Name = $"Test Tenant {theOrganisationId}"
            }
        };
    }
}