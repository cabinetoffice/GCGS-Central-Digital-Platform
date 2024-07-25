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
            .ForMember(dest => dest.NextQuestion, opt => opt.MapFrom(src => src.NextQuestion.Guid))
            .ForMember(dest => dest.NextQuestionAlternative, opt => opt.MapFrom(src => src.NextQuestionAlternative.Guid))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (Model.FormQuestionType)src.Type))
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options));

        CreateMap<Persistence.FormQuestionOptions, Model.FormQuestionOptions>();

        CreateMap<Persistence.FormQuestionChoice, Model.FormQuestionChoice>();

        CreateMap<Persistence.FormQuestionChoiceHint, Model.FormQuestionChoiceHint>();

        CreateMap<Persistence.FormSectionConfiguration, Model.FormSectionConfiguration>();
    }
}