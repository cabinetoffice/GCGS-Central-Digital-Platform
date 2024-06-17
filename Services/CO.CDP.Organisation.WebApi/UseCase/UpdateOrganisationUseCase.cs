using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class UpdateOrganisationUseCase(IOrganisationRepository organisationRepository, IMapper mapper)
            : IUseCase<(Guid organisationId, UpdateOrganisation updateOrganisation), bool>
{
    public async Task<bool> Execute((Guid organisationId, UpdateOrganisation updateOrganisation) command)
    {
        var organisation = await organisationRepository.Find(command.organisationId)
            ?? throw new UnknownOrganisationException($"Unknown organisation {command.organisationId}.");

        var updateObject = command.updateOrganisation.Organisation;

        switch (command.updateOrganisation.Type)
        {
            case OrganisationUpdateType.AdditionalIdentifiers:
                if (updateObject.AdditionalIdentifiers == null)
                {
                    throw new InvalidUpdateOrganisationCommand("Missing additional identifiers.");
                }
                foreach (var identifier in updateObject.AdditionalIdentifiers)
                {
                    var existingIdentifier = organisation.Identifiers.FirstOrDefault(i => i.Scheme == identifier.Scheme);
                    if (existingIdentifier != null)
                    {
                        existingIdentifier.IdentifierId = identifier.Id;
                        existingIdentifier.LegalName = identifier.LegalName;
                    }
                    else
                    {
                        organisation.Identifiers.Add(new OrganisationInformation.Persistence.Organisation.Identifier
                        {
                            IdentifierId = identifier.Id,
                            Primary = false,
                            LegalName = identifier.LegalName,
                            Scheme = identifier.Scheme
                        });
                    }
                }
                break;

            case OrganisationUpdateType.ContactPoint:
                if (updateObject.ContactPoint == null)
                {
                    throw new InvalidUpdateOrganisationCommand("Missing contact point.");
                }

                var existingContact = organisation.ContactPoints.FirstOrDefault();
                if (existingContact != null)
                {
                    existingContact.Name = updateObject.ContactPoint.Name;
                    existingContact.Email = updateObject.ContactPoint.Email;
                    existingContact.Telephone = updateObject.ContactPoint.Telephone;
                    existingContact.Url = updateObject.ContactPoint.Url;
                }
                else
                {
                    organisation.ContactPoints.Add(
                        mapper.Map<OrganisationInformation.Persistence.Organisation.ContactPoint>(updateObject.ContactPoint));
                }
                break;

            default:
                throw new InvalidUpdateOrganisationCommand("Unknown organisation update type.");
        }

        organisation.UpdateSupplierInformation();
        organisationRepository.Save(organisation);

        return await Task.FromResult(true);
    }
}