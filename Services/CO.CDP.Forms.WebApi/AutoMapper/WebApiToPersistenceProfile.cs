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
            .ForMember(dest => dest.Description, opt =>  opt.MapFrom<NullableLocalizedPropertyResolver<Persistence.FormQuestion, Model.FormQuestion>, string?>(src => src.Description))
            .ForMember(dest => dest.Caption, opt => opt.MapFrom<NullableLocalizedPropertyResolver<Persistence.FormQuestion, Model.FormQuestion>, string?>(src => src.Caption))
            .ForMember(dest => dest.SummaryTitle, opt => opt.MapFrom<NullableLocalizedPropertyResolver<Persistence.FormQuestion, Model.FormQuestion>, string?>(src => src.SummaryTitle))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Guid))
            .ForMember(dest => dest.NextQuestion, opt => opt.MapFrom(src => src.NextQuestion != null ? src.NextQuestion.Guid : (Guid?)null))
            .ForMember(dest => dest.NextQuestionAlternative, opt => opt.MapFrom(src => src.NextQuestionAlternative != null ? src.NextQuestionAlternative.Guid : (Guid?)null))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (Model.FormQuestionType)src.Type))
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options));

        CreateMap<Persistence.FormQuestionOptions, Model.FormQuestionOptions>();

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

        CreateMap<Persistence.FormSectionConfiguration, Model.FormSectionConfiguration>()
            .ForMember(dest => dest.SingularSummaryHeading, opt => opt.MapFrom<NullableLocalizedPropertyResolver<Persistence.FormSectionConfiguration, Model.FormSectionConfiguration>, string?>(src => src.SingularSummaryHeading))
            .ForMember(dest => dest.PluralSummaryHeadingFormat, opt => opt.MapFrom<NullableLocalizedPropertyResolver<Persistence.FormSectionConfiguration, Model.FormSectionConfiguration>, string?>(src => src.PluralSummaryHeadingFormat))
            .ForMember(dest => dest.AddAnotherAnswerLabel, opt => opt.MapFrom<NullableLocalizedPropertyResolver<Persistence.FormSectionConfiguration, Model.FormSectionConfiguration>, string?>(src => src.AddAnotherAnswerLabel))
            .ForMember(dest => dest.RemoveConfirmationCaption, opt => opt.MapFrom<NullableLocalizedPropertyResolver<Persistence.FormSectionConfiguration, Model.FormSectionConfiguration>, string?>(src => src.RemoveConfirmationCaption))
            .ForMember(dest => dest.RemoveConfirmationHeading, opt => opt.MapFrom<NullableLocalizedPropertyResolver<Persistence.FormSectionConfiguration, Model.FormSectionConfiguration>, string?>(src => src.RemoveConfirmationHeading))
            .ForMember(dest => dest.FurtherQuestionsExemptedHeading, opt => opt.MapFrom<NullableLocalizedPropertyResolver<Persistence.FormSectionConfiguration, Model.FormSectionConfiguration>, string?>(src => src.FurtherQuestionsExemptedHeading));

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
