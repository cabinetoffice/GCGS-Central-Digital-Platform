using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.AutoMapper;

public class WebApiToPersistenceProfile : Profile
{
    public WebApiToPersistenceProfile()
    {
        CreateMap<Persistence.Organisation, Model.Organisation>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid))
            .ForMember(m => m.Identifier, o => o.MapFrom(m => m.Identifiers.FirstOrDefault(i => i.Primary)))
            .ForMember(m => m.AdditionalIdentifiers, o => o.MapFrom(m => m.Identifiers.Where(i => !i.Primary)))
            .ForMember(m => m.ContactPoint, o => o.MapFrom(m => m.ContactPoints.FirstOrDefault() ?? new Persistence.Organisation.ContactPoint()));

        CreateMap<OrganisationIdentifier, Persistence.Organisation.Identifier>()
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.Primary, o => o.Ignore())
            .ForMember(m => m.Uri, o => o.Ignore())
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore())
            .ForMember(m => m.IdentifierId, o => o.MapFrom(m => m.Id))
            .ReverseMap()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.IdentifierId));

        CreateMap<Identifier, Persistence.Organisation.Identifier>()
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.Primary, o => o.Ignore())
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore())
            .ForMember(m => m.IdentifierId, o => o.MapFrom(m => m.Id))
            .ReverseMap()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.IdentifierId));

        CreateMap<OrganisationAddress, Persistence.Address>(MemberList.Source)
            .ForSourceMember(m => m.Type, o => o.DoNotValidate());

        CreateMap<OrganisationAddress, Persistence.Organisation.OrganisationAddress>()
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.Type, o => o.MapFrom(m => m.Type))
            .ForMember(m => m.Address, o => o.MapFrom(m => m));

        CreateMap<Persistence.Organisation.OrganisationAddress, Address>()
            .ForMember(m => m.Type, o => o.MapFrom(m => m.Type))
            .ForMember(m => m.StreetAddress, o => o.MapFrom(m => m.Address.StreetAddress))
            .ForMember(m => m.StreetAddress2, o => o.MapFrom(m => m.Address.StreetAddress2))
            .ForMember(m => m.Locality, o => o.MapFrom(m => m.Address.Locality))
            .ForMember(m => m.Region, o => o.MapFrom(m => m.Address.Region))
            .ForMember(m => m.PostalCode, o => o.MapFrom(m => m.Address.PostalCode))
            .ForMember(m => m.CountryName, o => o.MapFrom(m => m.Address.CountryName));

        CreateMap<OrganisationContactPoint, Persistence.Organisation.ContactPoint>()
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore())
            .ReverseMap();

        CreateMap<ContactPoint, Persistence.Organisation.ContactPoint>()
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore())
            .ReverseMap();

        CreateMap<RegisterOrganisation, Persistence.Organisation>()
            .ForMember(m => m.Guid, o => o.MapFrom((_, _, _, context) => context.Items["Guid"]))
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.Tenant, o => o.MapFrom((_, _, _, context) => context.Items["Tenant"]))
            .ForMember(m => m.Persons, o => o.Ignore())
            .ForMember(m => m.OrganisationPersons, o => o.Ignore())
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore())
            .ForMember(m => m.SupplierInfo, o => o.Ignore())
            .ForMember(m => m.BuyerInfo, o => o.Ignore())
            .ForMember(m => m.Identifiers, o => o.MapFrom<IdentifiersResolver>())
            .ForMember(m => m.ContactPoints, o => o.MapFrom(m => new[] { m.ContactPoint }));

        CreateMap<Persistence.Organisation.SupplierInformation, SupplierInformation>()
            .ForMember(m => m.OrganisationName, o => o.Ignore());

        CreateMap<TradeAssurance, Persistence.Organisation.TradeAssurance>()
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.Guid, o => o.MapFrom(_ => Guid.NewGuid()))
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore())
            .ReverseMap()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid));

        CreateMap<LegalForm, Persistence.Organisation.LegalForm>()
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore())
            .ReverseMap();

        CreateMap<Qualification, Persistence.Organisation.Qualification>()
           .ForMember(m => m.Id, o => o.Ignore())
           .ForMember(m => m.Guid, o => o.MapFrom(_ => Guid.NewGuid()))
           .ForMember(m => m.CreatedOn, o => o.Ignore())
           .ForMember(m => m.UpdatedOn, o => o.Ignore())
           .ReverseMap()
           .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid));

        ConnectedEntityMapping();
    }

    private void ConnectedEntityMapping()
    {
        CreateMap<Persistence.ConnectedEntity, ConnectedEntity>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid))
            .ReverseMap()
            .ForMember(m => m.Guid, o => o.MapFrom((_, _, _, context) => context.Items["Guid"]))
            .ForMember(m => m.Id, o => o.Ignore())
            // .ForMember(m => m.SupplierOrganisation, o => o.MapFrom(m => m.Organisation)) <--- Wrong
            .ForMember(m => m.SupplierOrganisation, o => o.Ignore())
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore());

        CreateMap<ConnectedEntityAddress, Persistence.ConnectedEntity.ConnectedEntityAddress>()
            .ReverseMap();

        CreateMap<Address, Persistence.Address>()
           .ForMember(m => m.Id, o => o.Ignore())
           .ForMember(m => m.CreatedOn, o => o.Ignore())
           .ForMember(m => m.UpdatedOn, o => o.Ignore())
           .ReverseMap()
           .ForMember(m => m.Type, o => o.Ignore()); // <--- Wrong

        CreateMap<ConnectedIndividualTrust, Persistence.ConnectedEntity.ConnectedIndividualTrust>()
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore())
            .ReverseMap();

        CreateMap<ConnectedOrganisation, Persistence.ConnectedEntity.ConnectedOrganisation>()
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore())
            .ReverseMap();
    }
    public class IdentifiersResolver : IValueResolver<RegisterOrganisation, Persistence.Organisation,
        ICollection<Persistence.Organisation.Identifier>>
    {
        public ICollection<Persistence.Organisation.Identifier> Resolve(
            RegisterOrganisation source, Persistence.Organisation destination,
            ICollection<Persistence.Organisation.Identifier> destMember,
            ResolutionContext context)
        {
            var pi = context.Mapper.Map<Persistence.Organisation.Identifier>(source.Identifier);
            pi.Primary = true;

            var ai = context.Mapper.Map<List<Persistence.Organisation.Identifier>>(source.AdditionalIdentifiers);
            ai.ForEach(i => i.Primary = false);

            return [pi, .. ai];
        }
    }
}