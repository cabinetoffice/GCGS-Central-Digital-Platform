using AutoMapper;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence.Forms;
using Persistence = CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.DataSharing.WebApi.AutoMapper;

public class DataSharingProfile : Profile
{
    public DataSharingProfile()
    {
        CreateMap<Persistence.SharedConsent, ShareReceipt>()
           .ForMember(m => m.FormId, o => o.MapFrom(m => m.Guid))
           .ForMember(m => m.FormVersionId, o => o.MapFrom(m => m.FormVersionId))
           .ForMember(m => m.ShareCode, o => o.MapFrom(m => m.BookingReference));

        CreateMap<Persistence.SharedConsent, Model.SharedConsent>()
          .ForMember(m => m.SubmittedAt, o => o.MapFrom(m => m.SubmittedAt))
          .ForMember(m => m.ShareCode, o => o.MapFrom(m => m.BookingReference));

        CreateMap<Persistence.SharedConsentQuestionAnswer, Model.SharedConsentQuestionAnswer>()
          .ForMember(m => m.QuestionId, o => o.MapFrom(m => m.QuestionId))
          .ForMember(m => m.Title, o => o.MapFrom(m => m.Title))
          .ForMember(m => m.Answer, o => o.MapFrom<CustomResolver>());

        CreateMap<Persistence.SharedConsentDetails, Model.SharedConsentDetails>()
          .ForMember(m => m.SubmittedAt, o => o.MapFrom(m => m.SubmittedAt))
          .ForMember(m => m.ShareCode, o => o.MapFrom(m => m.ShareCode));
    }
}
public class CustomResolver : IValueResolver<Persistence.SharedConsentQuestionAnswer, Model.SharedConsentQuestionAnswer, string?>
{
    public string? Resolve(Persistence.SharedConsentQuestionAnswer source, Model.SharedConsentQuestionAnswer destination, string? destMemb, ResolutionContext context)
    {
        switch (source.QuestionType)
        {
            case Persistence.FormQuestionType.Text:
            case Persistence.FormQuestionType.FileUpload:
                return source.Answer.TextValue;
            case Persistence.FormQuestionType.YesOrNo:
            case Persistence.FormQuestionType.CheckBox:
                return source.Answer.BoolValue.ToString();
            case Persistence.FormQuestionType.Date:
                return source.Answer.DateValue.ToString();
            case Persistence.FormQuestionType.Address:
                return source.Answer.AddressValue != null ? ToHtmlString(source.Answer.AddressValue) : "";
            default: return "";
        }
    }

    private string ToHtmlString(FormAddress source)
    {
        return $"{source.StreetAddress}<br/>{source.Locality}<br/>{source.PostalCode}<br/>{source.Region}<br/>{source.CountryName}";
    }
}