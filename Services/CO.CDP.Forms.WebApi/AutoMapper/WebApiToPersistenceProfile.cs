using AutoMapper;
using Persistence = CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.Forms.WebApi.AutoMapper;

public class WebApiToPersistenceProfile : Profile
{
    public WebApiToPersistenceProfile()
    {
        CreateMap<Persistence.FormSection, Model.FormSection>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom<LocalizedPropertyResolver<Persistence.FormSection, Model.FormSection>, string>(src => src.Title))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Guid));

        CreateMap<Persistence.FormQuestion, Model.FormQuestion>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom<LocalizedPropertyResolver<Persistence.FormQuestion, Model.FormQuestion>, string>(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom<NullableLocalizedPropertyResolver<Persistence.FormQuestion, Model.FormQuestion>, string?>(src => src.Description))
            .ForMember(dest => dest.Caption, opt => opt.MapFrom<NullableLocalizedPropertyResolver<Persistence.FormQuestion, Model.FormQuestion>, string?>(src => src.Caption))
            .ForMember(dest => dest.SummaryTitle, opt => opt.MapFrom<NullableLocalizedPropertyResolver<Persistence.FormQuestion, Model.FormQuestion>, string?>(src => src.SummaryTitle))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Guid))
            .ForMember(dest => dest.NextQuestion, opt => opt.MapFrom(src => src.NextQuestion != null ? src.NextQuestion.Guid : (Guid?)null))
            .ForMember(dest => dest.NextQuestionAlternative, opt => opt.MapFrom(src => src.NextQuestionAlternative != null ? src.NextQuestionAlternative.Guid : (Guid?)null))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (Model.FormQuestionType)src.Type))
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

        CreateMap<Persistence.FormQuestionOptions, Model.FormQuestionOptions>()
            .ForMember(dest => dest.Layout, opt => opt.MapFrom(src => src.Layout))
            .ForMember(dest => dest.Validation, opt => opt.MapFrom(src => src.Validation));

        CreateMap<Persistence.LayoutOptions, Model.LayoutOptions>()
            .ForMember(dest => dest.CustomYesText, opt => opt.MapFrom(src => src.CustomYesText))
            .ForMember(dest => dest.CustomNoText, opt => opt.MapFrom(src => src.CustomNoText))
            .ForMember(dest => dest.InputWidth, opt => opt.MapFrom(src => src.InputWidth))
            .ForMember(dest => dest.InputSuffix, opt => opt.MapFrom(src => src.InputSuffix))
            .ForMember(dest => dest.CustomCssClasses, opt => opt.MapFrom(src => src.CustomCssClasses))
            .ForMember(dest => dest.BeforeTitleContent, opt => opt.MapFrom(src => src.BeforeTitleContent))
            .ForMember(dest => dest.BeforeButtonContent, opt => opt.MapFrom(src => src.BeforeButtonContent))
            .ForMember(dest => dest.AfterButtonContent, opt => opt.MapFrom(src => src.AfterButtonContent))
            .ForMember(dest => dest.PrimaryButtonText, opt => opt.MapFrom(src => src.PrimaryButtonText));
        CreateMap<Persistence.InputSuffixOptions, Model.InputSuffixOptions>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.Text, opt => opt.MapFrom<NullableLocalizedPropertyResolver<Persistence.InputSuffixOptions, Model.InputSuffixOptions>, string?>(src => src.Text));

        CreateMap<Persistence.ValidationOptions, Model.ValidationOptions>()
            .ForMember(dest => dest.DateValidationType, opt => opt.MapFrom(src => src.DateValidationType))
            .ForMember(dest => dest.MinDate, opt => opt.MapFrom(src => src.MinDate))
            .ForMember(dest => dest.MaxDate, opt => opt.MapFrom(src => src.MaxDate))
            .ForMember(dest => dest.TextValidationType, opt => opt.MapFrom(src => src.TextValidationType));

        CreateMap<Persistence.FormQuestionChoice, Model.FormQuestionChoice>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom<LocalizedPropertyResolver<Persistence.FormQuestionChoice, Model.FormQuestionChoice>, string>(src => src.Title))
            .ForMember(dest => dest.GroupName, opt => opt.MapFrom<NullableLocalizedPropertyResolver<Persistence.FormQuestionChoice, Model.FormQuestionChoice>, string?>(src => src.GroupName))
            .ForMember(dest => dest.Hint, opt => opt.MapFrom(src => src.Hint))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

        CreateMap<Persistence.FormQuestionChoiceHint, Model.FormQuestionChoiceHint>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom<NullableLocalizedPropertyResolver<Persistence.FormQuestionChoiceHint, Model.FormQuestionChoiceHint>, string?>(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom<LocalizedPropertyResolver<Persistence.FormQuestionChoiceHint, Model.FormQuestionChoiceHint>, string>(src => src.Description));

        CreateMap<Persistence.FormQuestionGroup, Model.FormQuestionGroup>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom<LocalizedPropertyResolver<Persistence.FormQuestionGroup, Model.FormQuestionGroup>, string>(src => src.Name))
            .ForMember(dest => dest.Hint, opt => opt.MapFrom<LocalizedPropertyResolver<Persistence.FormQuestionGroup, Model.FormQuestionGroup>, string>(src => src.Hint))
            .ForMember(dest => dest.Caption, opt => opt.MapFrom<LocalizedPropertyResolver<Persistence.FormQuestionGroup, Model.FormQuestionGroup>, string>(src => src.Caption));

        CreateMap<Persistence.FormQuestionGroupChoice, Model.FormQuestionGroupChoice>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom<NullableLocalizedPropertyResolver<Persistence.FormQuestionGroupChoice, Model.FormQuestionGroupChoice>, string?>(src => src.Title));

        CreateMap<Persistence.FormQuestionGrouping, Model.FormQuestionGrouping>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CheckYourAnswers, opt => opt.MapFrom(src => src.CheckYourAnswers))
            .ForMember(dest => dest.Page, opt => opt.MapFrom(src => src.Page))
            .ForMember(dest => dest.SummaryTitle, opt => opt.MapFrom<NullableLocalizedPropertyResolver<Persistence.FormQuestionGrouping, Model.FormQuestionGrouping>, string?>(src => src.SummaryTitle));

        CreateMap<Persistence.FormSectionConfiguration, Model.FormSectionConfiguration>()
            .ForMember(dest => dest.SingularSummaryHeadingHint, opt => opt.MapFrom<NullableLocalizedPropertyResolver<Persistence.FormSectionConfiguration, Model.FormSectionConfiguration>, string?>(src => src.SingularSummaryHeadingHint))
            .ForMember(dest => dest.SingularSummaryHeading, opt => opt.MapFrom<NullableLocalizedPropertyResolver<Persistence.FormSectionConfiguration, Model.FormSectionConfiguration>, string?>(src => src.SingularSummaryHeading))
            .ForMember(dest => dest.PluralSummaryHeadingFormat, opt => opt.MapFrom<NullableLocalizedPropertyResolver<Persistence.FormSectionConfiguration, Model.FormSectionConfiguration>, string?>(src => src.PluralSummaryHeadingFormat))
            .ForMember(dest => dest.PluralSummaryHeadingHintFormat, opt => opt.MapFrom<NullableLocalizedPropertyResolver<Persistence.FormSectionConfiguration, Model.FormSectionConfiguration>, string?>(src => src.PluralSummaryHeadingHintFormat))
            .ForMember(dest => dest.AddAnotherAnswerLabel, opt => opt.MapFrom<NullableLocalizedPropertyResolver<Persistence.FormSectionConfiguration, Model.FormSectionConfiguration>, string?>(src => src.AddAnotherAnswerLabel))
            .ForMember(dest => dest.RemoveConfirmationCaption, opt => opt.MapFrom<NullableLocalizedPropertyResolver<Persistence.FormSectionConfiguration, Model.FormSectionConfiguration>, string?>(src => src.RemoveConfirmationCaption))
            .ForMember(dest => dest.RemoveConfirmationHeading, opt => opt.MapFrom<NullableLocalizedPropertyResolver<Persistence.FormSectionConfiguration, Model.FormSectionConfiguration>, string?>(src => src.RemoveConfirmationHeading))
            .ForMember(dest => dest.FurtherQuestionsExemptedHeading, opt => opt.MapFrom<NullableLocalizedPropertyResolver<Persistence.FormSectionConfiguration, Model.FormSectionConfiguration>, string?>(src => src.FurtherQuestionsExemptedHeading))
            .ForMember(dest => dest.FurtherQuestionsExemptedHint, opt => opt.MapFrom<NullableLocalizedPropertyResolver<Persistence.FormSectionConfiguration, Model.FormSectionConfiguration>, string?>(src => src.FurtherQuestionsExemptedHint))
            .ForMember(dest => dest.SummaryRenderFormatter, opt => opt.MapFrom(src => src.SummaryRenderFormatter));

        CreateMap<Persistence.SummaryRenderFormatter, Model.SummaryRenderFormatter>()
            .ForMember(dest => dest.KeyExpression, opt => opt.MapFrom<NullableLocalizedPropertyResolver<Persistence.SummaryRenderFormatter, Model.SummaryRenderFormatter>, string?>(src => src.KeyExpression))
            .ForMember(dest => dest.ValueExpression, opt => opt.MapFrom<NullableLocalizedPropertyResolver<Persistence.SummaryRenderFormatter, Model.SummaryRenderFormatter>, string?>(src => src.ValueExpression))
            .ForMember(dest => dest.KeyExpressionOperation, opt => opt.MapFrom(src => (Model.ExpressionOperationType)Enum.Parse(typeof(Model.ExpressionOperationType), src.KeyExpressionOperation)))
            .ForMember(dest => dest.ValueExpressionOperation, opt => opt.MapFrom(src => (Model.ExpressionOperationType)Enum.Parse(typeof(Model.ExpressionOperationType), src.ValueExpressionOperation)));

        CreateMap<Model.FormAnswer, Persistence.FormAnswer>()
            .ForMember(dest => dest.Guid, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.QuestionId, opt => opt.Ignore())
            .ForMember(dest => dest.Question, opt => opt.Ignore())
            .ForMember(dest => dest.FormAnswerSetId, opt => opt.Ignore())
            .ForMember(dest => dest.FormAnswerSet, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedFrom, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.StartValue, opt => opt.MapFrom(src => ToUtc(src.StartValue)))
            .ForMember(dest => dest.EndValue, opt => opt.MapFrom(src => ToUtc(src.EndValue)))
            .ForMember(dest => dest.DateValue, opt => opt.MapFrom(src => ToUtc(src.DateValue)));

        CreateMap<Persistence.FormAnswer, Model.FormAnswer>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Guid))
            .ForMember(dest => dest.QuestionId, opt => opt.MapFrom(src => src.Question.Guid));

        CreateMap<Persistence.FormAddress, Model.FormAddress>().ReverseMap();

        CreateMap<Persistence.FormAnswerSet, Model.FormAnswerSet>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Guid));

        CreateMap<Persistence.FormSectionSummary, Model.FormSectionSummary>()
            .ForMember(dest => dest.SectionName, opt => opt.MapFrom<LocalizedPropertyResolver<Persistence.FormSectionSummary, Model.FormSectionSummary>, string>(src => src.SectionName));
    }

    private static DateTime? ToUtc(DateTime? dateTime) => dateTime?.ToUniversalTime();
}
