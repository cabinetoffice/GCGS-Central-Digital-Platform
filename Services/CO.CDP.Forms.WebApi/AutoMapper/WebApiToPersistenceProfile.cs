using AutoMapper;
using Persistence = CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.Forms.WebApi.AutoMapper;

public class WebApiToPersistenceProfile : Profile
{
    public WebApiToPersistenceProfile()
    {
        CreateMap<Persistence.FormSection, Model.FormSection>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Guid));

        CreateMap<Persistence.FormQuestion, Model.FormQuestion>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Guid))
            .ForMember(dest => dest.NextQuestion, opt => opt.MapFrom(src => src.NextQuestion != null ? src.NextQuestion.Guid : (Guid?)null))
            .ForMember(dest => dest.NextQuestionAlternative, opt => opt.MapFrom(src => src.NextQuestionAlternative != null ? src.NextQuestionAlternative.Guid : (Guid?)null))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (Model.FormQuestionType)src.Type))
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options));

        CreateMap<Persistence.FormQuestionOptions, Model.FormQuestionOptions>();

        CreateMap<Persistence.FormQuestionChoice, Model.FormQuestionChoice>();

        CreateMap<Persistence.FormQuestionChoiceHint, Model.FormQuestionChoiceHint>();

        CreateMap<Persistence.FormSectionConfiguration, Model.FormSectionConfiguration>();        

        CreateMap<Model.FormAnswer, Persistence.FormAnswer>()
            .ForMember(dest => dest.Guid, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Question, opt => opt.Ignore())
            .ForMember(dest => dest.FormAnswerSet, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<Persistence.FormAnswer, Model.FormAnswer>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Guid))
            .ForMember(dest => dest.QuestionId, opt => opt.MapFrom(src => src.Question.Guid));
            

        CreateMap<Persistence.FormAnswerSet, Model.FormAnswerSet>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Guid))
              .ForMember(dest => dest.Answers, opt => opt.MapFrom(src => src.Answers.ToList()));
    }
}