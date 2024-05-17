using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.AutoMapper;

public class WebApiToPersistenceProfile : Profile
{
    public WebApiToPersistenceProfile()
    {
        CreateMap<Persistence.Organisation, Model.Organisation>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid))
            .ForMember(m => m.Identifier, o => o.MapFrom(m => m.Identifiers.FirstOrDefault(i => i.Primary)))
            .ForMember(m => m.AdditionalIdentifiers, o => o.MapFrom(m => m.Identifiers.Where(i => !i.Primary)));

        CreateMap<OrganisationIdentifier, Persistence.Organisation.OrganisationIdentifier>()
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.Primary, o => o.Ignore())
            .ForMember(m => m.IdentifierId, o => o.MapFrom(m => m.Id))
            .ReverseMap()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.IdentifierId));

        CreateMap<OrganisationAddress, Persistence.Organisation.OrganisationAddress>()
            .ReverseMap();

        CreateMap<OrganisationContactPoint, Persistence.Organisation.OrganisationContactPoint>()
            .ReverseMap();

        CreateMap<RegisterOrganisation, Persistence.Organisation>()
            .ForMember(m => m.Guid, o => o.MapFrom((_, _, _, context) => context.Items["Guid"]))
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.Tenant, o => o.MapFrom((_, _, _, context) => context.Items["Tenant"]))
            .ForMember(m => m.Persons, o => o.Ignore())
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore())
            .ForMember(m => m.Identifiers, o => o.MapFrom<IdentifiersResolver>());
    }

    public class IdentifiersResolver : IValueResolver<RegisterOrganisation, Persistence.Organisation,
        ICollection<Persistence.Organisation.OrganisationIdentifier>>
    {
        public ICollection<Persistence.Organisation.OrganisationIdentifier> Resolve(
            RegisterOrganisation source, Persistence.Organisation destination,
            ICollection<Persistence.Organisation.OrganisationIdentifier> destMember,
            ResolutionContext context)
        {
            var pi = context.Mapper.Map<Persistence.Organisation.OrganisationIdentifier>(source.Identifier);
            pi.Primary = true;

            var ai = context.Mapper.Map<List<Persistence.Organisation.OrganisationIdentifier>>(source.AdditionalIdentifiers);
            ai.ForEach(i => i.Primary = false);

            return [pi, .. ai];
        }
    }
}