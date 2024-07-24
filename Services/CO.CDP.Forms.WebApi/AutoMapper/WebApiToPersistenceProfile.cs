using AutoMapper;
using Persistence = CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.Forms.WebApi.AutoMapper;

public class WebApiToPersistenceProfile : Profile
{
    public WebApiToPersistenceProfile()
    {
        CreateMap<Persistence.FormSection, CO.CDP.Forms.WebApi.Model.FormSection>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Guid));

        CreateMap<Persistence.FormQuestion, CO.CDP.Forms.WebApi.Model.FormQuestion>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Guid))
            .ForMember(dest => dest.NextQuestion, opt => opt.MapFrom(src => src.NextQuestion.Guid))
            .ForMember(dest => dest.NextQuestionAlternative, opt => opt.MapFrom(src => src.NextQuestionAlternative.Guid))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (CO.CDP.Forms.WebApi.Model.FormQuestionType)src.Type))
            .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options));

        CreateMap<Persistence.FormQuestionOptions, CO.CDP.Forms.WebApi.Model.FormQuestionOptions>()
            .ForMember(dest => dest.Choices, opt => opt.MapFrom(src => src.Choices))
            .ForMember(dest => dest.ChoiceProviderStrategy, opt => opt.MapFrom(src => src.ChoiceProviderStrategy));

        CreateMap<Persistence.FormQuestionChoice, CO.CDP.Forms.WebApi.Model.FormQuestionChoice>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.GroupName))
            .ForMember(dest => dest.Hint, opt => opt.MapFrom(src => src.Hint));

        CreateMap<Persistence.FormQuestionChoiceHint, CO.CDP.Forms.WebApi.Model.FormQuestionChoiceHint>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
    }
}