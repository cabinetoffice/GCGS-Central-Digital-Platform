using AutoMapper;
using Model = CO.CDP.DataSharing.WebApi.Model;
using Persistence = CO.CDP.OrganisationInformation.Persistence;
using OrgInfo = CO.CDP.OrganisationInformation;

namespace CO.CDP.DataSharing.WebApi.AutoMapper;

public class WebApiToPersistenceProfile : Profile
{
    public WebApiToPersistenceProfile()
    {
        CreateMap<Persistence.Forms.SharedConsent, Model.ShareReceipt>()
           .ForMember(m => m.FormId, o => o.MapFrom(m => m.Guid))
           .ForMember(m => m.FormVersionId, o => o.MapFrom(m => m.FormVersionId));

        CreateMap<Persistence.Forms.SharedConsent, Model.SharedConsent>()
          .ForMember(m => m.SubmittedAt, o => o.MapFrom(m => m.SubmittedAt));

        CreateMap<Persistence.Forms.SharedConsentQuestionAnswer, Model.SharedConsentQuestionAnswer>()
          .ForMember(m => m.QuestionId, o => o.MapFrom(m => m.QuestionId))
          .ForMember(m => m.Title, o => o.MapFrom(m => m.Title))
          .ForMember(m => m.Answer, o => o.MapFrom<CustomResolver>());

        CreateMap<Persistence.Forms.SharedConsentDetails, Model.SharedConsentDetails>()
          .ForMember(m => m.SubmittedAt, o => o.MapFrom(m => m.SubmittedAt))
          .ForMember(m => m.ShareCode, o => o.MapFrom(m => m.ShareCode));

        CreateMap<Persistence.Forms.SharedConsent, Model.SupplierInformation>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Organisation.Guid))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Organisation.Name))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Organisation.Addresses.FirstOrDefault(i => i.Type == OrgInfo.AddressType.Registered)))
            .ForMember(dest => dest.ContactPoint, opt => opt.MapFrom(src => src.Organisation.ContactPoints.FirstOrDefault()))
            .ForMember(m => m.Identifier, o => o.MapFrom(src => src.Organisation.Identifiers.FirstOrDefault(i => i.Primary)))
            .ForMember(dest => dest.AdditionalIdentifiers, opt => opt.MapFrom(src => src.Organisation.Identifiers.Where(i => !i.Primary).ToList()))
            .ForMember(dest => dest.Details, opt => opt.MapFrom(src => new Model.Details()));

        CreateMap<Persistence.Organisation.OrganisationAddress, OrgInfo.Address>()
            .ForMember(m => m.Type, o => o.MapFrom(m => m.Type))
            .ForMember(m => m.StreetAddress, o => o.MapFrom(m => m.Address.StreetAddress))
            .ForMember(m => m.Locality, o => o.MapFrom(m => m.Address.Locality))
            .ForMember(m => m.Region, o => o.MapFrom(m => m.Address.Region))
            .ForMember(m => m.PostalCode, o => o.MapFrom(m => m.Address.PostalCode))
            .ForMember(m => m.CountryName, o => o.MapFrom(m => m.Address.CountryName))
            .ForMember(m => m.Country, o => o.MapFrom(m => m.Address.Country));

        CreateMap<Persistence.Organisation.ContactPoint, OrgInfo.ContactPoint>()
                .ForMember(m => m.Name, o => o.MapFrom(m => m.Name))
                .ForMember(m => m.Email, o => o.MapFrom(m => m.Email))
                .ForMember(m => m.Telephone, o => o.MapFrom(m => m.Telephone))
                .ForMember(m => m.Url, o => o.MapFrom(m => string.IsNullOrEmpty(m.Url) ? null : new Uri(m.Url)));

        CreateMap<Persistence.Organisation.Identifier, OrgInfo.Identifier>()
                       .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.IdentifierId))
                       .ForMember(dest => dest.Scheme, opt => opt.MapFrom(src => src.Scheme))
                       .ForMember(dest => dest.LegalName, opt => opt.MapFrom(src => src.LegalName))
                       .ForMember(dest => dest.Uri, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Uri) ? null : new Uri(src.Uri)))
                       .ReverseMap();

    }
}
public class CustomResolver : IValueResolver<Persistence.Forms.SharedConsentQuestionAnswer, Model.SharedConsentQuestionAnswer, string?>
{
    public string? Resolve(Persistence.Forms.SharedConsentQuestionAnswer source, Model.SharedConsentQuestionAnswer destination, string? destMemb, ResolutionContext context)
    {
        switch (source.QuestionType)
        {
            case Persistence.Forms.FormQuestionType.Text:
            case Persistence.Forms.FormQuestionType.FileUpload:
                return source.Answer.TextValue;
            case Persistence.Forms.FormQuestionType.YesOrNo:
            case Persistence.Forms.FormQuestionType.CheckBox:
                return source.Answer.BoolValue.ToString();
            case Persistence.Forms.FormQuestionType.Date:
                return source.Answer.DateValue.ToString();
            case Persistence.Forms.FormQuestionType.Address:
                return source.Answer.AddressValue != null ? ToHtmlString(source.Answer.AddressValue) : "";
            default: return "";
        }
    }

    private string ToHtmlString(Persistence.Forms.FormAddress source)
    {
        return $"{source.StreetAddress}<br/>{source.Locality}<br/>{source.PostalCode}<br/>{source.Region}<br/>{source.CountryName}";
    }
}