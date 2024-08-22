using Amazon.Auth.AccessControlPolicy;
using AutoMapper;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence.Forms;
using System.Data;
using System.Text.Json;

namespace CO.CDP.DataSharing.WebApi.AutoMapper;

public class DataSharingProfile : Profile
{
    public DataSharingProfile()
    {
        CreateMap<OrganisationInformation.Persistence.Forms.SharedConsent, ShareReceipt>()
           .ForMember(m => m.FormId, o => o.MapFrom(m => m.Guid))
           .ForMember(m => m.FormVersionId, o => o.MapFrom(m => m.FormVersionId))
           .ForMember(m => m.ShareCode, o => o.MapFrom(m => m.BookingReference));

        CreateMap<OrganisationInformation.Persistence.Forms.SharedConsent, Model.SharedConsent>()
          .ForMember(m => m.SubmittedAt, o => o.MapFrom(m => m.SubmittedAt))
          .ForMember(m => m.ShareCode, o => o.MapFrom(m => m.BookingReference));

        CreateMap<OrganisationInformation.Persistence.Forms.SharedConsentQuestionAnswer, Model.SharedConsentQuestionAnswer>()
          .ForMember(m => m.QuestionId, o => o.MapFrom(m => m.QuestionId))
          .ForMember(m => m.Title, o => o.MapFrom(m => m.Title))
          .ForMember(m => m.Answer, o => o.MapFrom<CustomResolver>());

        CreateMap<OrganisationInformation.Persistence.Forms.SharedConsentDetails, Model.SharedConsentDetails>()
          .ForMember(m => m.SubmittedAt, o => o.MapFrom(m => m.SubmittedAt))
          .ForMember(m => m.ShareCode, o => o.MapFrom(m => m.ShareCode));
    }


}
public class CustomResolver : IValueResolver<OrganisationInformation.Persistence.Forms.SharedConsentQuestionAnswer, Model.SharedConsentQuestionAnswer, string?>
{
    public string? Resolve(OrganisationInformation.Persistence.Forms.SharedConsentQuestionAnswer source, Model.SharedConsentQuestionAnswer destination, string? destMemb, ResolutionContext context)
    {
        switch (source.QuestionType)
        {
            case CO.CDP.OrganisationInformation.Persistence.Forms.FormQuestionType.Text:
            case CO.CDP.OrganisationInformation.Persistence.Forms.FormQuestionType.FileUpload:
                return source.Answer.TextValue;
            case CO.CDP.OrganisationInformation.Persistence.Forms.FormQuestionType.YesOrNo:
            case CO.CDP.OrganisationInformation.Persistence.Forms.FormQuestionType.CheckBox:
                return source.Answer.BoolValue.ToString();
            case CO.CDP.OrganisationInformation.Persistence.Forms.FormQuestionType.Date:
                return source.Answer.DateValue.ToString();
            case CO.CDP.OrganisationInformation.Persistence.Forms.FormQuestionType.Address:
                return source.Answer.AddressValue != null ? ToHtmlString(source.Answer.AddressValue) : "";
            default: return "";
        }
    }

    private string ToHtmlString(FormAddress source)
    {
        return $"{source.StreetAddress}<br/>{source.Locality}<br/>{source.PostalCode}<br/>{source.Region}<br/>{source.CountryName}";
    }
}
