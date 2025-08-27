using AutoMapper;
using CO.CDP.Organisation.WebApi.Events;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation;
using Microsoft.OpenApi.Extensions;
using Address = CO.CDP.OrganisationInformation.Address;
using ContactPoint = CO.CDP.OrganisationInformation.ContactPoint;
using Identifier = CO.CDP.OrganisationInformation.Identifier;
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
            .ForMember(m => m.ContactPoint, o => o.MapFrom(m => m.ContactPoints.FirstOrDefault() ?? new Persistence.ContactPoint()))
            .ForMember(m => m.Details, o => o.MapFrom(m => new Details
            {
                PendingRoles = m.PendingRoles,
                BuyerInformation = m.BuyerInfo != null
                                    ? new BuyerInformation { BuyerType = m.BuyerInfo.BuyerType, DevolvedRegulations = m.BuyerInfo.DevolvedRegulations } : null,

                Scale = (m.SupplierInfo != null && m.SupplierInfo.OperationTypes != null && m.SupplierInfo.OperationTypes.Contains(OperationType.SmallOrMediumSized))
                ? "small"
                : ((m.SupplierInfo == null || (m.SupplierInfo.OperationTypes == null || m.SupplierInfo.OperationTypes.Count == 0)) ? null : "large"),

                Vcse = (m.SupplierInfo != null && m.SupplierInfo.OperationTypes != null && m.SupplierInfo.OperationTypes.Count > 0)
                ? m.SupplierInfo.OperationTypes.Contains(OperationType.NonGovernmental)
                : (bool?)null,

                ShelteredWorkshop = (m.SupplierInfo != null && (m.SupplierInfo.OperationTypes != null && m.SupplierInfo.OperationTypes.Count > 0))
                ? m.SupplierInfo.OperationTypes.Contains(OperationType.SupportedEmploymentProvider)
                : (bool?)null,

                PublicServiceMissionOrganization = (m.SupplierInfo != null && m.SupplierInfo.OperationTypes != null && m.SupplierInfo.OperationTypes.Count > 0)
                ? m.SupplierInfo.OperationTypes.Contains(OperationType.PublicService)
                : (bool?)null
            }));

        CreateMap<Persistence.Organisation, Model.OrganisationSearchResult>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid))
            .ForMember(m => m.Identifier, o => o.MapFrom(m => m.Identifiers.FirstOrDefault(i => i.Primary)));

        CreateMap<Persistence.Organisation, Model.OrganisationSearchByPponResult>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid))
            .ForMember(m => m.Identifiers, o => o.MapFrom(m => m.Identifiers))
            .ForMember(m => m.Name, o => o.MapFrom(m => m.Name))
            .ForMember(m => m.Addresses, o => o.MapFrom(m => m.Addresses.Select(a => new OrganisationAddress
            {
                Type = a.Type,
                StreetAddress = a.Address.StreetAddress,
                Locality = a.Address.Locality,
                Region = a.Address.Region,
                PostalCode = a.Address.PostalCode,
                CountryName = a.Address.CountryName,
                Country = a.Address.Country
            }).ToList()))
            .ForMember(m => m.PartyRoles, o => o.MapFrom(m =>
                m.Roles.Select(r => new PartyRoleWithStatus { Role = r, Status = PartyRoleStatus.Active })
                .Concat(m.PendingRoles.Select(r => new PartyRoleWithStatus { Role = r, Status = PartyRoleStatus.Pending }))
                .ToList()));

        CreateMap<Persistence.Organisation, Review>()
            .ForMember(m => m.ApprovedOn, o => o.MapFrom(m => m.ApprovedOn))
            .ForMember(m => m.ReviewedBy, o => o.MapFrom(m =>
                m.ReviewedBy != null ?
                new ReviewedBy
                {
                    Name = $"{m.ReviewedBy.FirstName} {m.ReviewedBy.LastName}",
                    Id = m.ReviewedBy.Guid
                } : null))
            .ForMember(m => m.Comment, o => o.MapFrom(m => m.ReviewComment))
            .ForMember(m => m.Status, o => o.MapFrom<ReviewStatusResolver>());

        CreateMap<Persistence.Organisation, OrganisationExtended>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid))
            .ForMember(m => m.Identifier, o => o.MapFrom(m => m.Identifiers.FirstOrDefault(i => i.Primary)))
            .ForMember(m => m.AdditionalIdentifiers, o => o.MapFrom(m => m.Identifiers.Where(i => !i.Primary)))
            .ForMember(m => m.ContactPoint, o => o.MapFrom(m => m.ContactPoints.FirstOrDefault() ?? new Persistence.ContactPoint()))
            .ForMember(m => m.AdminPerson, o => o.Ignore())
            .ForMember(m => m.Details, o => o.MapFrom(m => new Details
            {
                Approval = new Approval
                {
                    ApprovedOn = m.ApprovedOn,
                    ReviewedBy = m.ReviewedBy != null ? new ReviewedBy
                    {
                        Name = $"{m.ReviewedBy.FirstName} {m.ReviewedBy.LastName}",
                        Id = m.ReviewedBy.Guid
                    } : null,
                    Comment = m.ReviewComment
                }
            }));

        CreateMap<OrganisationIdentifier, Persistence.Identifier>()
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.OrganisationId, o => o.Ignore())
            .ForMember(m => m.Organisation, o => o.Ignore())
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.Primary, o => o.Ignore())
            .ForMember(m => m.Uri, o => o.Ignore())
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore())
            .ForMember(m => m.IdentifierId, o => o.MapFrom(m => m.Id))
            .ReverseMap()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.IdentifierId));

        Uri? tempResult;
        CreateMap<Identifier, Persistence.Identifier>()
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.OrganisationId, o => o.Ignore())
            .ForMember(m => m.Organisation, o => o.Ignore())
            .ForMember(m => m.Primary, o => o.Ignore())
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore())
            .ForMember(m => m.IdentifierId, o => o.MapFrom(m => m.Id))
            .ReverseMap()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.IdentifierId))
            .ForMember(m => m.Uri, o => o.MapFrom(m => Uri.TryCreate(m.Uri, UriKind.Absolute, out tempResult) ? tempResult : null));

        CreateMap<OrganisationAddress, Persistence.Address>(MemberList.Source)
            .ForSourceMember(m => m.Type, o => o.DoNotValidate());

        CreateMap<OrganisationAddress, Persistence.OrganisationAddress>()
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.OrganisationId, o => o.Ignore())
            .ForMember(m => m.Organisation, o => o.Ignore())
            .ForMember(m => m.Type, o => o.MapFrom(m => m.Type))
            .ForMember(m => m.Address, o => o.MapFrom(m => m));

        CreateMap<Persistence.OrganisationAddress, Address>()
            .ForMember(m => m.Type, o => o.MapFrom(m => m.Type))
            .ForMember(m => m.StreetAddress, o => o.MapFrom(m => m.Address.StreetAddress))
            .ForMember(m => m.Locality, o => o.MapFrom(m => m.Address.Locality))
            .ForMember(m => m.Region, o => o.MapFrom(m => m.Address.Region))
            .ForMember(m => m.PostalCode, o => o.MapFrom(m => m.Address.PostalCode))
            .ForMember(m => m.CountryName, o => o.MapFrom(m => m.Address.CountryName))
            .ForMember(m => m.Country, o => o.MapFrom(m => m.Address.Country));

        CreateMap<OrganisationContactPoint, Persistence.ContactPoint>()
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.OrganisationId, o => o.Ignore())
            .ForMember(m => m.Organisation, o => o.Ignore())
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore())
            .ReverseMap();

        CreateMap<ContactPoint, Persistence.ContactPoint>()
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.OrganisationId, o => o.Ignore())
            .ForMember(m => m.Organisation, o => o.Ignore())
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore())
            .ReverseMap();

        CreateMap<RegisterOrganisation, Persistence.Organisation>()
            .ForMember(m => m.Guid, o => o.MapFrom((_, _, _, context) => context.Items["Guid"]))
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.Tenant, o => o.MapFrom((_, _, _, context) => context.Items["Tenant"]))
            .ForMember(m => m.Roles, o => o.MapFrom(c => c.Roles.Where(r => r != PartyRole.Buyer).ToList()))
            .ForMember(m => m.PendingRoles, o => o.MapFrom(c => c.Roles.Where(r => r == PartyRole.Buyer).ToList()))
            .ForMember(m => m.Persons, o => o.Ignore())
            .ForMember(m => m.OrganisationPersons, o => o.Ignore())
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore())
            .ForMember(m => m.SupplierInfo, o => o.Ignore())
            .ForMember(m => m.BuyerInfo, o => o.Ignore())
            .ForMember(m => m.Identifiers, o => o.MapFrom<IdentifiersResolver>())
            .ForMember(m => m.ContactPoints, o => o.MapFrom(m => new[] { m.ContactPoint }))
            .ForMember(m => m.ReviewedBy, o => o.Ignore())
            .ForMember(m => m.ReviewComment, o => o.Ignore())
            .ForMember(m => m.ApprovedOn, o => o.Ignore())
            .ForMember(m => m.ReviewedById, o => o.Ignore());

        CreateMap<Persistence.SupplierInformation, SupplierInformation>()
            .ForMember(m => m.OrganisationName, o => o.Ignore());

        CreateMap<Persistence.BuyerInformation, BuyerInformation>();

        CreateMap<LegalForm, Persistence.LegalForm>()
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore())
            .ReverseMap();

        CreateMap<Persistence.Person, Person>()
            .ForMember(m => m.Scopes, o => o.Ignore())
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid));

        CreateMap<Persistence.PersonInvite, PersonInviteModel>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid));

        CreateMap<Persistence.AuthenticationKey, AuthenticationKey>();

        CreateMap<Persistence.OrganisationJoinRequest, OrganisationJoinRequest>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid))
            .ForMember(dest => dest.RequestCreated, opt => opt.Ignore());

        CreateMap<Persistence.OrganisationJoinRequest, JoinRequestLookUp>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid));

        ConnectedEntityMapping();
        OrganisationEventsMapping();
        OrganisationPartiesMapping();
        MouSignatureMapping();
    }

    private void OrganisationPartiesMapping()
    {
        CreateMap<Persistence.OrganisationParty, OrganisationParty>()
            .ForMember(m => m.Name, o => o.MapFrom(m => m.ChildOrganisation!.Name))
            .ForMember(m => m.Id, o => o.MapFrom(m => m.ChildOrganisation!.Guid))
            .ForMember(m => m.ShareCode, o => o.MapFrom(m => m.SharedConsent));

        CreateMap<Persistence.Forms.SharedConsent, OrganisationPartyShareCode>()
            .ForMember(m => m.Value, o => o.MapFrom(m => m.ShareCode))
            .ForMember(m => m.SubmittedAt, o => o.MapFrom(m => m.SubmittedAt));

    }
    private void MouSignatureMapping()
    {
        CreateMap<Persistence.MouSignature, Model.MouSignature>()
          .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.SignatureGuid))
            .ForMember(dest => dest.Mou, opt => opt.MapFrom(src => src.Mou))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
            .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.JobTitle))
            .ForMember(dest => dest.SignatureOn, opt => opt.MapFrom(src => src.CreatedOn));

        CreateMap<Persistence.MouSignature, Model.MouSignatureLatest>()
          .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.SignatureGuid))
            .ForMember(dest => dest.Mou, opt => opt.MapFrom(src => src.Mou))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
            .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.JobTitle))
            .ForMember(dest => dest.SignatureOn, opt => opt.MapFrom(src => src.CreatedOn))
            .ForMember(dest => dest.IsLatest, opt => opt.Ignore());

        CreateMap<Persistence.Mou, Model.Mou>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Guid))
            .ForMember(dest => dest.FilePath, opt => opt.MapFrom(src => src.FilePath))
            .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => src.CreatedOn));
    }

    private void ConnectedEntityMapping()
    {
        CreateMap<RegisterConnectedEntity, Persistence.ConnectedEntity>()
            .ForMember(m => m.Guid, o => o.MapFrom((_, _, _, context) => context.Items["Guid"]))
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.SupplierOrganisation, o => o.Ignore())
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore());

        CreateMap<UpdateConnectedEntity, Persistence.ConnectedEntity>()
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.Guid, o => o.Ignore())
            .ForMember(m => m.SupplierOrganisation, o => o.Ignore())
            .ForMember(m => m.Addresses, o => o.Ignore())
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore());

        CreateMap<Persistence.ConnectedEntity, ConnectedEntity>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid));

        CreateMap<CreateConnectedIndividualTrust, Persistence.ConnectedEntity.ConnectedIndividualTrust>()
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore())
            .ReverseMap();

        CreateMap<CreateConnectedOrganisation, Persistence.ConnectedEntity.ConnectedOrganisation>()
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore())
            .ReverseMap();

        CreateMap<UpdateConnectedIndividualTrust, Persistence.ConnectedEntity.ConnectedIndividualTrust>()
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore())
            .ReverseMap();

        CreateMap<UpdateConnectedOrganisation, Persistence.ConnectedEntity.ConnectedOrganisation>()
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore())
            .ReverseMap();

        CreateMap<ConnectedIndividualTrust, Persistence.ConnectedEntity.ConnectedIndividualTrust>()
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore())
            .ForMember(m => m.Category, o => o.MapFrom(m => MapCategory(m.Category)))
            .ReverseMap();

        CreateMap<ConnectedOrganisation, Persistence.ConnectedEntity.ConnectedOrganisation>()
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.CreatedOn, o => o.Ignore())
            .ForMember(m => m.UpdatedOn, o => o.Ignore())
            .ReverseMap();

        CreateMap<Address, Persistence.Address>(MemberList.Source)
            .ForSourceMember(m => m.Type, o => o.DoNotValidate());

        CreateMap<Address, Persistence.ConnectedEntity.ConnectedEntityAddress>()
            .ForMember(m => m.Id, o => o.Ignore())
            .ForMember(m => m.Type, o => o.MapFrom(m => m.Type))
            .ForMember(m => m.Address, o => o.MapFrom(m => m));

        CreateMap<Persistence.ConnectedEntity.ConnectedEntityAddress, Address>()
            .ForMember(m => m.Type, o => o.MapFrom(m => m.Type))
            .ForMember(m => m.StreetAddress, o => o.MapFrom(m => m.Address.StreetAddress))
            .ForMember(m => m.Locality, o => o.MapFrom(m => m.Address.Locality))
            .ForMember(m => m.Region, o => o.MapFrom(m => m.Address.Region))
            .ForMember(m => m.PostalCode, o => o.MapFrom(m => m.Address.PostalCode))
            .ForMember(m => m.CountryName, o => o.MapFrom(m => m.Address.CountryName))
            .ForMember(m => m.Country, o => o.MapFrom(m => m.Address.Country));

        CreateMap<Persistence.ConnectedEntityLookup, ConnectedEntityLookup>()
            .ForMember(m => m.Uri, o => o.MapFrom((src, _, _, context) => new Uri($"https://cdp.cabinetoffice.gov.uk/organisations/{context.Items["OrganisationId"]}/connected-entities/{src.EntityId}")))
            .ForMember(m => m.IsInUse, o => o.Ignore())
            .ForMember(m => m.FormGuid, o => o.Ignore())
            .ForMember(m => m.SectionGuid, o => o.Ignore())
            .ReverseMap();
    }

    private static Persistence.ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType MapCategory(ConnectedIndividualAndTrustCategory category)
    {
        return category switch
        {
            ConnectedIndividualAndTrustCategory.PersonWithSignificantControlForIndividual => Persistence.ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndiv,
            ConnectedIndividualAndTrustCategory.DirectorOrIndividualWithTheSameResponsibilitiesForIndividual => Persistence.ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndivWithTheSameResponsibilitiesForIndiv,
            ConnectedIndividualAndTrustCategory.AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual => Persistence.ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndivWithSignificantInfluenceOrControlForIndiv,
            ConnectedIndividualAndTrustCategory.PersonWithSignificantControlForTrust => Persistence.ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust,
            ConnectedIndividualAndTrustCategory.DirectorOrIndividualWithTheSameResponsibilitiesForTrust => Persistence.ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndivWithTheSameResponsibilitiesForTrust,
            ConnectedIndividualAndTrustCategory.AnyOtherIndividualWithSignificantInfluenceOrControlForTrust => Persistence.ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndivWithSignificantInfluenceOrControlForTrust,
            _ => Persistence.ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndiv,
        };
    }

    private void OrganisationEventsMapping()
    {
        CreateMap<Persistence.Organisation, OrganisationRegistered>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid))
            .ForMember(m => m.Identifier, o => o.MapFrom(m => m.Identifiers.FirstOrDefault(i => i.Primary)))
            .ForMember(m => m.AdditionalIdentifiers, o => o.MapFrom(m => m.Identifiers.Where(i => !i.Primary)))
            .ForMember(m => m.ContactPoint, o => o.MapFrom(m => m.ContactPoints.FirstOrDefault() ?? new Persistence.ContactPoint()))
            .ForMember(m => m.Addresses, o => o.MapFrom(m => m.Addresses))
            .ForMember(m => m.Type, o => o.MapFrom(m => m.Type))
            .ForMember(m => m.Roles, o => o.MapFrom(m => m.Roles.Select(r => r.AsCode())));
        CreateMap<Persistence.Organisation, OrganisationUpdated>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.Guid))
            .ForMember(m => m.Identifier, o => o.MapFrom(m => m.Identifiers.FirstOrDefault(i => i.Primary)))
            .ForMember(m => m.AdditionalIdentifiers, o => o.MapFrom(m => m.Identifiers.Where(i => !i.Primary)))
            .ForMember(m => m.ContactPoint, o => o.MapFrom(m => m.ContactPoints.FirstOrDefault() ?? new Persistence.ContactPoint()))
            .ForMember(m => m.Addresses, o => o.MapFrom(m => m.Addresses))
            .ForMember(m => m.Roles, o => o.MapFrom(m => m.Roles.Select(r => r.AsCode())));
        CreateMap<Persistence.OrganisationAddress, Events.Address>()
            .ForMember(m => m.Type, o => o.MapFrom(m => m.Type.GetDisplayName()))
            .ForMember(m => m.StreetAddress, o => o.MapFrom(m => m.Address.StreetAddress))
            .ForMember(m => m.Locality, o => o.MapFrom(m => m.Address.Locality))
            .ForMember(m => m.Region, o => o.MapFrom(m => m.Address.Region))
            .ForMember(m => m.PostalCode, o => o.MapFrom(m => m.Address.PostalCode))
            .ForMember(m => m.CountryName, o => o.MapFrom(m => m.Address.CountryName))
            .ForMember(m => m.Country, o => o.MapFrom(m => m.Address.Country));
        CreateMap<Persistence.Identifier, Events.Identifier>()
            .ForMember(m => m.Id, o => o.MapFrom(m => m.IdentifierId));
        CreateMap<Persistence.ContactPoint, Events.ContactPoint>();
    }

    public class IdentifiersResolver : IValueResolver<RegisterOrganisation, Persistence.Organisation,
        IList<Persistence.Identifier>>
    {
        public IList<Persistence.Identifier> Resolve(
            RegisterOrganisation source, Persistence.Organisation destination,
            IList<Persistence.Identifier> destMember,
            ResolutionContext context)
        {
            var pi = context.Mapper.Map<Persistence.Identifier>(source.Identifier);
            pi.Primary = true;

            var ai = context.Mapper.Map<List<Persistence.Identifier>>(source.AdditionalIdentifiers);
            ai.ForEach(i => i.Primary = false);

            return [pi, .. ai];
        }
    }

    public class ReviewStatusResolver : IValueResolver<Persistence.Organisation, Review, ReviewStatus>
    {
        public ReviewStatus Resolve(
            Persistence.Organisation organisation,
            Review review,
            ReviewStatus status,
            ResolutionContext context)
        {
            if (organisation.PendingRoles.Count > 0)
            {
                if (organisation.ReviewedBy == null)
                {
                    return ReviewStatus.Pending;
                }
                else
                {
                    return ReviewStatus.Rejected;
                }
            }
            else
            {
                return ReviewStatus.Approved;
            }
        }
    }
}
