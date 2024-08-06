using AutoMapper;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.DataSharing.WebApi.AutoMapper;

public class DataSharingProfile : Profile
{
    public DataSharingProfile()
    {
        CreateMap<SharedConsent, ShareReceipt>()
           .ForMember(m => m.FormId, o => o.MapFrom(m => m.Guid))
           .ForMember(m => m.FormVersionId, o => o.MapFrom(m => m.FormVersionId))
           .ForMember(m => m.ShareCode, o => o.MapFrom(m => m.BookingReference));
    }
}