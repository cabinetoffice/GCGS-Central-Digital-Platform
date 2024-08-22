using AutoMapper;
using CO.CDP.DataSharing.WebApi.Model;

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
    }
}